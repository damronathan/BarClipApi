using BarClipApi.Core.Repositories;
using BarClipApi.Data.Schema;
using BarClipApi.Models.Requests;
using BarClipApi.Models.Responses;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace BarClipApi.Core.Services;

public interface IVideoService
{
    Task<string> GetUploadSasUrl(SasUrlRequest request);
    Task<ICollection<VideoResponse>> GetVideos(GetVideosRequest request);
    Task SaveVideo(VideoRequest request);
}

public class VideoService : IVideoService
{
    private readonly StorageService? _storageService;
    private readonly VideoRepository _repo;

    public VideoService(StorageService storageService, VideoRepository repo)
    {
        _storageService = storageService;
        _repo = repo;
    }
    public async Task SaveVideo(VideoRequest request)
    {
        var url = _storageService.GenerateDownloadSasUrl(new SasUrlRequest { Id = request.VideoId, ContainerName = "videos", Extension = ".mov" });

        if (string.IsNullOrEmpty(url))
        {
            throw new ArgumentException("Failed to generate download SAS URL");
        }

        await _repo.SaveVideoAsync(request);
    }
    public async Task<string> GetUploadSasUrl(SasUrlRequest request)
    {
        var url = _storageService.GenerateUploadSasUrl(request);

        if (string.IsNullOrEmpty(url))
        {
            throw new InvalidOperationException("Failed to generate upload SAS URL");
        }

        return url;
    }

    public async Task<ICollection<VideoResponse>> GetVideos(GetVideosRequest request)
    {
        var videos = new List<Video>();

        if (request.SessionId is null)
        {
            videos = await _repo.GetAllVideosForUserAsync(request.UserId);
        }
        else
        {
            videos = await _repo.GetAllVideosForSessionAsync(request.SessionId);
        }

        var videoResponses = new List<VideoResponse>();

        foreach (var video in videos)
        {
            var videoResponse = new VideoResponse
            {
                Id = video.Id,
                OrderNumber = video.OrderNumber,
                IsFull = video.IsFull,
                VideoSasUrl = _storageService.GenerateDownloadSasUrl(new SasUrlRequest { Id = video.Id, ContainerName = "videos", Extension = ".mov" }),
                ThumbnailSasUrl = _storageService.GenerateDownloadSasUrl(new SasUrlRequest { Id = video.Id, ContainerName = "thumbnails", Extension = ".jpg" })
            };
            videoResponses.Add(videoResponse);
        }
        return videoResponses;
    }

}
