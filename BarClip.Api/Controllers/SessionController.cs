using BarClip.Core.Services;
using BarClip.Models.Dto;
using BarClip.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BarClip.Api.Controllers;

[Route("api/sessions")]
[ApiController]
//[Authorize]
public class SessionController : ControllerBase
{
    private readonly SessionService _sessionService;
    public SessionController(SessionService sessionService)
    {
        _sessionService = sessionService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SessionDto>>> GetSessions()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User identification not found");
        }
        var sessions = await _sessionService.GetAllSessions(userId);
        return Ok(sessions);
    }
    [HttpPatch("{id}")]
    public async Task<ActionResult<SessionDto>> UpdateSession(Guid id, [FromBody] UpdateSessionRequest request)
    {
        var response = await _sessionService.UpdateSession(id, request);
        return Ok(response);
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSession(Guid id)
    {
        await _sessionService.DeleteSession(id);
        return Ok();
    }
}
