using Microsoft.AspNetCore.Mvc;
using MissionManagement.Api.DTOs;
using MissionManagement.Core.Commands;
using MissionManagement.Core.Enums;
using MissionManagement.Core.Queries;
using MissionManagement.Infrastructure.Handlers;

namespace MissionManagement.Api.Controllers;

/// <summary>
/// API controller for mission management operations.
/// </summary>
[ApiController]
[Route("v1/missions")]
[Produces("application/json")]
public sealed class MissionsController : ControllerBase
{
    private readonly CreateMissionCommandHandler _createHandler;
    private readonly UpdateMissionCommandHandler _updateHandler;
    private readonly DeleteMissionCommandHandler _deleteHandler;
    private readonly GetMissionByIdQueryHandler _getByIdHandler;
    private readonly ListMissionsQueryHandler _listHandler;
    private readonly ILogger<MissionsController> _logger;

    public MissionsController(
        CreateMissionCommandHandler createHandler,
        UpdateMissionCommandHandler updateHandler,
        DeleteMissionCommandHandler deleteHandler,
        GetMissionByIdQueryHandler getByIdHandler,
        ListMissionsQueryHandler listHandler,
        ILogger<MissionsController> logger)
    {
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _deleteHandler = deleteHandler;
        _getByIdHandler = getByIdHandler;
        _listHandler = listHandler;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new mission.
    /// </summary>
    /// <param name="request">Mission creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created mission ID</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CreateMissionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateMission(
        [FromBody] CreateMissionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // TODO: Get user ID from authentication context
            var userId = Guid.NewGuid(); // Placeholder

            var command = new CreateMissionCommand(
                request.Name,
                request.Type,
                request.StartEpoch,
                userId,
                request.Description,
                request.EndEpoch);

            var result = await _createHandler.HandleAsync(command, cancellationToken);

            _logger.LogInformation("Mission created: {MissionId}", result.MissionId);

            return CreatedAtAction(
                nameof(GetMission),
                new { id = result.MissionId },
                new CreateMissionResponse(result.MissionId));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to create mission");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets a mission by ID.
    /// </summary>
    /// <param name="id">Mission ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Mission details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MissionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMission(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            // TODO: Get user ID from authentication context
            var userId = Guid.NewGuid(); // Placeholder

            var query = new GetMissionByIdQuery(id, userId);
            var mission = await _getByIdHandler.HandleAsync(query, cancellationToken);

            if (mission == null)
            {
                return NotFound(new { error = "Mission not found" });
            }

            var response = new MissionResponse(
                mission.MissionId,
                mission.Name,
                mission.Description,
                mission.Type,
                mission.StartEpoch,
                mission.EndEpoch,
                mission.Status,
                mission.OwnerId,
                mission.CreatedAt,
                mission.UpdatedAt);

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access to mission {MissionId}", id);
            return Forbid();
        }
    }

    /// <summary>
    /// Lists missions for the current user with optional filtering and pagination.
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: 100)</param>
    /// <param name="status">Filter by status</param>
    /// <param name="searchTerm">Search by name</param>
    /// <param name="sortBy">Sort by field (name, createdAt, updatedAt)</param>
    /// <param name="sortDescending">Sort descending</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of missions</returns>
    [HttpGet]
    [ProducesResponseType(typeof(MissionListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListMissions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] MissionStatus? status = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        CancellationToken cancellationToken = default)
    {
        // TODO: Get user ID from authentication context
        var userId = Guid.NewGuid(); // Placeholder

        // Validate pagination
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = new ListMissionsQuery(
            userId,
            page,
            pageSize,
            status,
            searchTerm,
            sortBy,
            sortDescending);

        var result = await _listHandler.HandleAsync(query, cancellationToken);

        var missions = result.Missions.Select(m => new MissionResponse(
            m.MissionId,
            m.Name,
            m.Description,
            m.Type,
            m.StartEpoch,
            m.EndEpoch,
            m.Status,
            m.OwnerId,
            m.CreatedAt,
            m.UpdatedAt)).ToList();

        var response = new MissionListResponse(
            missions,
            result.TotalCount,
            result.Page,
            result.PageSize,
            result.TotalPages);

        return Ok(response);
    }

    /// <summary>
    /// Updates an existing mission.
    /// </summary>
    /// <param name="id">Mission ID</param>
    /// <param name="request">Mission update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateMission(
        [FromRoute] Guid id,
        [FromBody] UpdateMissionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // TODO: Get user ID from authentication context
            var userId = Guid.NewGuid(); // Placeholder

            var command = new UpdateMissionCommand(
                id,
                request.Name,
                request.Type,
                request.StartEpoch,
                userId,
                request.Description,
                request.EndEpoch);

            await _updateHandler.HandleAsync(command, cancellationToken);

            _logger.LogInformation("Mission updated: {MissionId}", id);

            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to update mission {MissionId}", id);
            return ex.Message.Contains("not found") ? NotFound(new { error = ex.Message }) : BadRequest(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access to mission {MissionId}", id);
            return Forbid();
        }
    }

    /// <summary>
    /// Deletes a mission (soft delete).
    /// </summary>
    /// <param name="id">Mission ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteMission(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            // TODO: Get user ID from authentication context
            var userId = Guid.NewGuid(); // Placeholder

            var command = new DeleteMissionCommand(id, userId);
            await _deleteHandler.HandleAsync(command, cancellationToken);

            _logger.LogInformation("Mission deleted: {MissionId}", id);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to delete mission {MissionId}", id);
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access to mission {MissionId}", id);
            return Forbid();
        }
    }
}
