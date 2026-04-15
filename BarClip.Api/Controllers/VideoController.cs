using BarClip.Core.Repositories;
using BarClip.Core.Services;
using BarClip.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BarClip.Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

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

    [HttpPost("save-video")]
    public async Task<IActionResult> SaveVideo([FromBody] VideoRequest request)
    {
        var url = _storageService.GenerateDownloadSasUrl(request.VideoId);

        if (string.IsNullOrEmpty(url))
        {
            return BadRequest("Failed to generate download SAS URL");
        }

        await _videoService.SaveVideo(request);

        await _hubContext.Clients.User(request.UserId).SendAsync("VideoSaved", request, url);

        return Ok(new { Message = "Videos saved successfully." });
    }

    [HttpGet("upload-sas-url")]
    public UploadSasUrlResponse UploadSasUrl()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentException("User identification not found");
        }

        var url = _storageService.GenerateUploadSasUrl(Guid.NewGuid());

        if (string.IsNullOrEmpty(url))
        {
            throw new InvalidOperationException("Failed to generate upload SAS URL");
        }
        var response = new UploadSasUrlResponse
        {
            UserId = userId,
            UploadSasUrl = url
        };
        return response;
    }
    [HttpGet("test")]
    public IActionResult Test()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Ok(new { Message = "Auth working", UserId = userId });
    }
}
