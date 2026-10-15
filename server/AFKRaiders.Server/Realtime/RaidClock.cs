using AFKRaiders.Server.Domain;
using Microsoft.AspNetCore.SignalR;

namespace AFKRaiders.Server.Realtime;

public sealed class RaidClock(RaidCoordinator raids, IHubContext<RaidHub> hub, TimeProvider time) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(250), time);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            foreach (var revision in raids.AdvanceRaids())
            {
                await hub.Clients.Group(revision.RaidId).SendAsync("RaidChanged", revision.Revision, stoppingToken);
            }
        }
    }
}
