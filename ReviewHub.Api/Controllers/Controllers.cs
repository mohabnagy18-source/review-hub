using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReviewHub.Application.DTOs;
using ReviewHub.Application.Services;

namespace ReviewHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AuthService auth) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest req, CancellationToken ct)
    {
        try { return Ok(await auth.RegisterAsync(req, ct)); }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { error = ex.Message }); }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest req, CancellationToken ct)
    {
        try { return Ok(await auth.LoginAsync(req, ct)); }
        catch (UnauthorizedAccessException ex) { return Unauthorized(new { error = ex.Message }); }
    }
}

[ApiController]
[Route("api/[controller]")]
public class PlacesController(PlaceService places) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? kind, [FromQuery] string? search, CancellationToken ct)
        => Ok(await places.GetPlacesAsync(kind, search, ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetOne(Guid id, CancellationToken ct)
        => await places.GetPlaceAsync(id, ct) is { } p ? Ok(p) : NotFound(new { error = "Place not found." });

    [Authorize]
    [HttpPost("{id:guid}/reviews")]
    public async Task<IActionResult> UpsertReview(Guid id, CreateReviewRequest req, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try { return Ok(await places.UpsertReviewAsync(userId, id, req, ct)); }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReviewsController(PlaceService places) : ControllerBase
{
    [HttpGet("mine")]
    public async Task<IActionResult> Mine(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await places.GetMyReviewsAsync(userId, ct));
    }
}
