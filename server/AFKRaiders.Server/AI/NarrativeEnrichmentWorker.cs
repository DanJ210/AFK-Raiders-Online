namespace AFKRaiders.Server.AI;

using AFKRaiders.Server.Domain;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Threading.Channels;

/// <summary>
/// Background worker that drains asynchronous narrative enrichment requests from a bounded channel,
/// delegating text generation to the INarrativeGenerator without blocking simulation ticks.
/// </summary>
public sealed class NarrativeEnrichmentWorker : BackgroundService
{
    private readonly ChannelReader<NarrativeEnrichmentRequest> reader;
    private readonly INarrativeGenerator generator;
    private readonly ILogger<NarrativeEnrichmentWorker> logger;

    public NarrativeEnrichmentWorker(
        ChannelReader<NarrativeEnrichmentRequest> reader,
        INarrativeGenerator generator,
        ILogger<NarrativeEnrichmentWorker>? logger = null)
    {
        this.reader = reader;
        this.generator = generator;
        this.logger = logger ?? NullLogger<NarrativeEnrichmentWorker>.Instance;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("NarrativeEnrichmentWorker started. Listening for narrative requests using generator: {GeneratorName}", generator.Name);
        try
        {
            while (await reader.WaitToReadAsync(stoppingToken))
            {
                while (reader.TryRead(out var request))
                {
                    try
                    {
                        if (request.IsCurrent is not null && !request.IsCurrent())
                        {
                            request.OnResult?.Invoke(new("", "Expired", "StaleBeforeGeneration"));
                            continue;
                        }
                        if (request.EventType == "AarDebrief" && request.EventPayload is AarDebriefContext ctx)
                        {
                            var debrief = await generator.GenerateAarDebriefAsync(ctx, stoppingToken);
                            var formatted = string.Join("\n", debrief.Bullets.Select(b => $"▸ {b}"));
                            if (request.OnResult is not null) request.OnResult(new(formatted, debrief.Source));
                            else request.OnGenerated(formatted);
                        }
                        else
                        {
                            var result = await generator.GenerateEventResultAsync(request, stoppingToken);
                            if (request.OnResult is not null) request.OnResult(result);
                            else request.OnGenerated(result.Text);
                        }
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Failed to enrich narrative request for raid {RaidId}", request.RaidId);
                        request.OnResult?.Invoke(new("", "CanonFallback", "WorkerError"));
                    }
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Graceful shutdown
        }
        logger.LogInformation("NarrativeEnrichmentWorker stopped.");
    }
}
