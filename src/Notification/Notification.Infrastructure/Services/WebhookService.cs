using System.Diagnostics;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Notification.Core.Interfaces;
using Notification.Core.Models;
using Polly;
using Polly.Retry;

namespace Notification.Infrastructure.Services;

internal sealed class WebhookService : IWebhookService
{
    private readonly HttpClient _httpClient;
    private readonly WebhookOptions _options;
    private readonly ILogger<WebhookService> _logger;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

    public WebhookService(
        HttpClient httpClient,
        WebhookOptions options,
        ILogger<WebhookService> logger)
    {
        _httpClient = httpClient;
        _options = options;
        _logger = logger;

        _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);

        _retryPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(r => (int)r.StatusCode >= 500)
            .WaitAndRetryAsync(
                _options.MaxRetries,
                retryAttempt => TimeSpan.FromSeconds(_options.RetryDelaySeconds * Math.Pow(2, retryAttempt - 1)),
                onRetry: (outcome, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        "Webhook delivery attempt {RetryCount} failed with status {StatusCode}. Retrying in {RetryDelay}s",
                        retryCount,
                        outcome.Result?.StatusCode ?? 0,
                        timeSpan.TotalSeconds);
                });
    }

    public async Task<WebhookDeliveryResult> SendAsync(
        WebhookNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ValidateWebhookUrl(request.WebhookUrl))
        {
            return new WebhookDeliveryResult
            {
                Success = false,
                StatusCode = 0,
                ErrorMessage = "Invalid webhook URL"
            };
        }

        var payload = JsonSerializer.Serialize(request.Payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, request.WebhookUrl);
        httpRequest.Content = new StringContent(payload, Encoding.UTF8, "application/json");

        // Add standard headers
        httpRequest.Headers.Add(_options.EventTypeHeaderName, request.EventType);
        httpRequest.Headers.Add(_options.TimestampHeaderName, timestamp);

        // Add signature if secret is provided
        if (!string.IsNullOrEmpty(request.Secret))
        {
            var signaturePayload = $"{timestamp}.{payload}";
            var signature = GenerateSignature(signaturePayload, request.Secret);
            httpRequest.Headers.Add(_options.SignatureHeaderName, $"sha256={signature}");
        }

        // Add custom headers
        foreach (var (key, value) in request.Headers)
        {
            if (!httpRequest.Headers.TryAddWithoutValidation(key, value))
            {
                httpRequest.Content.Headers.TryAddWithoutValidation(key, value);
            }
        }

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await _retryPolicy.ExecuteAsync(async () =>
            {
                var req = await CloneHttpRequestAsync(httpRequest);
                return await _httpClient.SendAsync(req, cancellationToken);
            });

            stopwatch.Stop();

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            var success = response.IsSuccessStatusCode;

            if (success)
            {
                _logger.LogInformation(
                    "Webhook delivered successfully to {WebhookUrl} in {ResponseTime}ms",
                    request.WebhookUrl,
                    stopwatch.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogWarning(
                    "Webhook delivery to {WebhookUrl} returned non-success status {StatusCode}",
                    request.WebhookUrl,
                    (int)response.StatusCode);
            }

            return new WebhookDeliveryResult
            {
                Success = success,
                StatusCode = (int)response.StatusCode,
                ResponseBody = responseBody.Length > 8000 ? responseBody[..8000] : responseBody,
                ResponseTime = stopwatch.Elapsed
            };
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            stopwatch.Stop();
            _logger.LogError(
                "Webhook delivery to {WebhookUrl} timed out after {TimeoutSeconds}s",
                request.WebhookUrl,
                _options.TimeoutSeconds);

            return new WebhookDeliveryResult
            {
                Success = false,
                StatusCode = 0,
                ErrorMessage = $"Request timed out after {_options.TimeoutSeconds} seconds",
                ResponseTime = stopwatch.Elapsed
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "Webhook delivery to {WebhookUrl} failed after {MaxRetries} attempts",
                request.WebhookUrl,
                _options.MaxRetries);

            return new WebhookDeliveryResult
            {
                Success = false,
                StatusCode = 0,
                ErrorMessage = ex.Message,
                ResponseTime = stopwatch.Elapsed
            };
        }
    }

    public string GenerateSignature(string payload, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public bool ValidateWebhookUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;

        return uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp;
    }

    private static async Task<HttpRequestMessage> CloneHttpRequestAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);

        if (request.Content != null)
        {
            var content = await request.Content.ReadAsStringAsync();
            clone.Content = new StringContent(content, Encoding.UTF8, "application/json");
        }

        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clone;
    }
}
