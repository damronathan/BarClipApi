using BarClip.Core.Services;
using BarClip.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BarClip.Api.Controllers;

[Route("api/user")]
[ApiController]
[Authorize]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    [HttpPatch]
    public async Task<ActionResult<string>> UpdateUser([FromBody] UpdateUserRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
        {
            return Unauthorized();
        }

        await _userService.UpdateUser(userId, request);
        return Ok(request.Email);
    }
    [HttpDelete]
    public async Task<IActionResult> DeleteUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId is null)
        {
            return Ok("User already does not exist.");
        }

        await _userService.DeleteUser(userId);

        return Ok();
    }
}
