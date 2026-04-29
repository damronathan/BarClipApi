using BarClipApi.Core.Repositories;
using BarClipApi.Core.Services;
using BarClipApi.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BarClipApi.Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using BarClipApi.Models.Responses;

namespace BarClipApi.Api.Controllers;

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
        var sasRequest = new SasUrlRequest
        {
            Id = request.SessionId,
            ContainerName = "videos",
            Extension = ".mov"
        };
        var url = _storageService.GenerateDownloadSasUrl(sasRequest);

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

    [HttpGet("test")]
    public IActionResult Test()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Ok(new { Message = "Auth working", UserId = userId });
    }
}
