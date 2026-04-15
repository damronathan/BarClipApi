using BarClip.Core.Helpers;
using BarClip.Core.Interfaces;
using BarClip.Core.Repositories;
using BarClip.Data.Schema;
using BarClip.Models.Requests;
using FFMpegCore;
using System.Globalization;
using System.Text.Json;
using static BarClip.Core.Helpers.FileHelper;

namespace BarClip.Core.Services;

public interface IVideoService
{
    string GetFileNameFromMessageText(string messageText);
    Task<VideoRequest> GetVideoRequestFromMessage(string fileName);
    Task<List<Video>> GetAllVideosForSession(Guid SessionId);
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
        await _repo.SaveVideoAsync(request);
    }
    public async Task<List<Video>> GetAllVideosForSession(Guid SessionId)
    {
        return await _repo.GetAllVideosForSessionAsync(SessionId);
    }
    public async Task<VideoRequest> GetVideoRequestFromMessage(string messsageText)
    {
        var fileName = GetFileNameFromMessageText(messsageText);

        var metadata = await _storageService.GetMetaDataAsync(fileName, "videos");

        var userId = GetValueFromMetadata(metadata, "UserId");
        var videoId = Guid.TryParse(GetValueFromMetadata(metadata, "VideoId"), out var videoIdResult) ? videoIdResult : Guid.Empty;
        var sessionId = Guid.TryParse(GetValueFromMetadata(metadata, "SessionId"), out var sessionIdResult) ? sessionIdResult : Guid.Empty;
        var createdAtString = GetValueFromMetadata(metadata, "CreatedAt");        
        var createdAt = DateTime.ParseExact(createdAtString, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        

        if (userId == null || videoId == Guid.Empty || sessionId == Guid.Empty)
        {
            throw new ArgumentException($"Metadata for file {fileName} is missing required fields. UserId: {userId}, VideoId: {videoId}, SessionId: {sessionId}, CreatedAt: {createdAt}");
        }
        var videoRequest = new VideoRequest()
        {
                UserId = userId,
                VideoId = videoId,
                SessionId = sessionId,
                CreatedAt = createdAt
        };
        return videoRequest;
    }
    public string GetFileNameFromMessageText(string messageText)
    {
        using var doc = JsonDocument.Parse(messageText);
        var root = doc.RootElement;

        if (!root.TryGetProperty("subject", out JsonElement subjectElement))
            throw new ArgumentException($"Message: {messageText} does not contain 'subject' property.");

        var subject = subjectElement.GetString()
            ?? throw new ArgumentException($"Message: {messageText} does not contain a valid 'subject' property.");

        const string prefix = "/blobServices/default/containers/videos/blobs/";

        if (subject.StartsWith(prefix))
        {
            // Exact case match - normal extraction
            return subject[prefix.Length..];
        }
        else
        {
            throw new ArgumentException($"Unexpected subject format: {subject}");
        }
    }
    private static string? GetValueFromMetadata(IDictionary<string, string> metadata, string key)
    {
        if (!metadata.TryGetValue(key, out var value))
            return null;

        return value;
    }
    

}
public class VideoRequest
{
    public string UserId { get; set; }
    public Guid VideoId { get; set; }
    public Guid SessionId { get; set; }
    public DateTime CreatedAt { get; set;  }
}
