using BarClipApi.Core.Repositories;
using BarClipApi.Data.Schema;
using BarClipApi.Models.Requests;
using BarClipApi.Models.Responses;

namespace BarClipApi.Core.Services;

public interface IVideoService
{
    Task<string> GetUploadSasUrl(Guid id, string containerName, string extension);
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
        var url = _storageService.GenerateDownloadSasUrl(request.VideoId, "videos", ".mov");

        if (string.IsNullOrEmpty(url))
        {
            throw new ArgumentException("Failed to generate download SAS URL");
        }

        await _repo.SaveVideoAsync(request);
    }
    public async Task<string> GetUploadSasUrl(Guid id, string containerName, string extension)
    {
        var url = _storageService.GenerateUploadSasUrl(id, containerName, extension);

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
                VideoSasUrl = _storageService.GenerateDownloadSasUrl(video.Id, "videos", ".mov"),
                ThumbnailSasUrl = _storageService.GenerateDownloadSasUrl(video.Id, "thumbnails", ".jpg")
            };
            videoResponses.Add(videoResponse);
        }
        return videoResponses;
    }

}
