using BarClip.Core.Repositories;
using BarClip.Core.Services;
using BarClip.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BarClip.Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using BarClip.Models.Responses;

namespace BarClip.Api.Controllers;

[Route("api/video")]
[ApiController]
[Authorize]
public class VideoController : ControllerBase
{
    private readonly IVideoService _videoService;
    private readonly IHubContext<VideoStatusHub> _hubContext;
    private readonly StorageService _storageService;
    public VideoController(IVideoService videoService, IHubContext<VideoStatusHub> hubContext, StorageService storageService)
    {
        _videoService = videoService;
        _hubContext = hubContext;
        _storageService = storageService;
    }

    [HttpGet]
    public async Task<ActionResult<ICollection<VideoResponse>>> GetVideos([FromQuery] GetVideosRequest request)
    {
        var response = await _videoService.GetVideos(request);

        return Ok(response);
    }

    [HttpPost("save-video")]
    public async Task<IActionResult> SaveVideo([FromBody] VideoRequest request)
    {
        await _videoService.SaveVideo(request);
        await _hubContext.Clients.User(request.UserId).SendAsync("VideoSaved", request);

        return Ok(new { Message = "Videos saved successfully." });
    }

    [HttpGet("upload-sas-url")]
    public async Task<ActionResult<UploadSasUrlResponse>> UploadSasUrl([FromQuery] SasUrlRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User identification not found");
        }

        var url = await _videoService.GetUploadSasUrl(request);
        
        var response = new UploadSasUrlResponse
        {
            UserId = userId,
            UploadSasUrl = url
        };
        return response;
    }

    [HttpPost("upload-test")]
    [AllowAnonymous]
    public async Task<IActionResult> UploadTest([FromForm] TestRequest request)
    {
        var fileName = request.File.FileName;
        var fullPath = Path.Combine(Path.GetTempPath(), fileName);

        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await request.File.CopyToAsync(stream);
        }

        await _storageService.UploadVideoStringAsync(fileName, fullPath, "videos");


        return Ok(new { Message = "Videos saved successfully." });
    }

    [HttpGet("test")]
    public IActionResult Test()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Ok(new { Message = "Auth working", UserId = userId });
    }
}
