namespace Shared.Domain.Common;

/// <summary>
/// Represents a paginated result set.
/// </summary>
/// <typeparam name="T">The type of items in the result.</typeparam>
public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; }
    public int TotalCount { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PagedResult(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    public static PagedResult<T> Empty(int pageNumber = 1, int pageSize = 10) =>
        new(Array.Empty<T>(), 0, pageNumber, pageSize);

    public PagedResult<TResult> Map<TResult>(Func<T, TResult> mapper) =>
        new(Items.Select(mapper).ToList(), TotalCount, PageNumber, PageSize);
}

/// <summary>
/// Parameters for pagination requests.
/// </summary>
public sealed record PaginationRequest(int PageNumber = 1, int PageSize = 10)
{
    public int Skip => (PageNumber - 1) * PageSize;
    public int Take => PageSize;

    public static PaginationRequest Default => new();
    public static PaginationRequest FirstPage(int pageSize = 10) => new(1, pageSize);
}
