using Azure.Storage.Queues.Models;
using BarClip.Core.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BarClipFunction;

public class BarClipQueueTrigger
{
    private readonly ILogger<BarClipQueueTrigger> _logger;
    private FunctionService _functionService;
    private IApiClientService _client;

    public BarClipQueueTrigger(ILogger<BarClipQueueTrigger> logger, FunctionService functionService, IApiClientService client)
    {
        _logger = logger;
        _functionService = functionService;
        _client = client;
    }

    [Function(nameof(BarClipQueueTrigger))]

    public async Task Run(
    [QueueTrigger("new-video", Connection = "AzureWebJobsStorage")] QueueMessage message)
    {
        _logger.LogInformation("Processing new video message");

        var request = await _functionService.GetVideoRequestFromMessage(message.MessageText);

        var response = await _client.SaveVideoAsync(request);

        _logger.LogInformation("Successfully processed video {VideoId}", request.VideoId);    }

}