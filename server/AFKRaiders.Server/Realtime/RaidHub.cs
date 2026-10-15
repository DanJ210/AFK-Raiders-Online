using AFKRaiders.Server.Domain;
using Microsoft.AspNetCore.SignalR;

namespace AFKRaiders.Server.Realtime;

public sealed class RaidHub(RaidCoordinator raids) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var raidId = Context.GetHttpContext()?.Request.Query["raidId"].ToString();
        var token = Context.GetHttpContext()?.Request.Query["access_token"].ToString();

        try
        {
            raids.GetRaidForConnection(token ?? string.Empty, raidId ?? string.Empty);
            await Groups.AddToGroupAsync(Context.ConnectionId, raidId!);
            await base.OnConnectedAsync();
        }
        catch (RaidAccessException)
        {
            Context.Abort();
        }
    }
}
