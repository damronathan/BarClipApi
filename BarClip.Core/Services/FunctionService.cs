using BarClipApi.Models.Requests;
using System.Globalization;
using System.Text.Json;

namespace BarClipApi.Core.Services;
public class FunctionService
{
    private readonly StorageService? _storageService;

    public FunctionService(StorageService storageService)
    {
        _storageService = storageService;

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
        var orderNumberString = GetValueFromMetadata(metadata, "OrderNumber");
        var orderNumber = int.TryParse(orderNumberString, out var orderNumberResult) ? orderNumberResult : 0;
        var isFullString = GetValueFromMetadata(metadata, "IsFull");
        var isFull = bool.TryParse(isFullString, out var isFullResult) ? isFullResult : false;

        if (userId == null || videoId == Guid.Empty || sessionId == Guid.Empty)
        {
            throw new ArgumentException($"Metadata for file {fileName} is missing required fields. UserId: {userId}, VideoId: {videoId}, SessionId: {sessionId}, CreatedAt: {createdAt}");
        }
        var videoRequest = new VideoRequest()
        {
            UserId = userId,
            VideoId = videoId,
            SessionId = sessionId,
            CreatedAt = createdAt,
            OrderNumber = orderNumber,
            IsFull = isFull,
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
