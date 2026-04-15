using Azure.Storage.Queues.Models;
using BarClip.Core.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BarClipFunction;

public class BarClipQueueTrigger
{
    private readonly ILogger<BarClipQueueTrigger> _logger;
    private IVideoService _videoService;
    private IApiClientService _client;

    public BarClipQueueTrigger(ILogger<BarClipQueueTrigger> logger, IVideoService videoService, IApiClientService client)
    {
        _logger = logger;
        _videoService = videoService;
        _client = client;
    }

    [Function(nameof(BarClipQueueTrigger))]

    public async Task Run(
    [QueueTrigger("new-video", Connection = "AzureWebJobsStorage")] QueueMessage message)
    {
        _logger.LogInformation("Processing new video message");

        var request = await _videoService.GetVideoRequestFromMessage(message.MessageText);

        await _client.SaveVideoAsync(request);

        _logger.LogInformation("Successfully processed video {VideoId}", request.VideoId);
    }

}