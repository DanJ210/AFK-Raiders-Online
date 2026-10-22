using AFKRaiders.Server.AI;
using AFKRaiders.Server.Domain;
using AFKRaiders.Server.Realtime;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Channels;

var builder = WebApplication.CreateBuilder(args);

var narrativeChannel = Channel.CreateBounded<NarrativeEnrichmentRequest>(new BoundedChannelOptions(8)
{
    FullMode = BoundedChannelFullMode.Wait
});
builder.Services.AddSingleton(narrativeChannel);
builder.Services.AddSingleton(narrativeChannel.Reader);
builder.Services.AddSingleton(narrativeChannel.Writer);

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddSingleton<INarrativeGenerator>(sp =>
{
    var configuredPath = builder.Configuration["AI:ModelPath"];
    var modelPath = string.IsNullOrWhiteSpace(configuredPath)
        ? Environment.GetEnvironmentVariable("AFK_AI_MODEL_PATH")
        : configuredPath;
    var logger = sp.GetRequiredService<ILogger<OnnxGenAiNarrativeGenerator>>();
    return new OnnxGenAiNarrativeGenerator(modelPath, logger);
});

builder.Services.AddSingleton<IRaidNarrator>(sp =>
{
    var writer = sp.GetRequiredService<ChannelWriter<NarrativeEnrichmentRequest>>();
    return new TemplateRaidNarrator(writer);
});

// Validate bundled gameplay and narration catalogs before accepting requests.
builder.Services.AddSingleton<ILootCatalog>(LootCatalog.Default);
_ = ShopCatalog.GetStarterWeapon();
_ = AFKRaiders.Server.Content.NarrationContent.Default;
builder.Services.AddSingleton<RaidCoordinator>();
builder.Services.AddHostedService<NarrativeEnrichmentWorker>();
builder.Services.AddSignalR();
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHostedService<RaidClock>();
}
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.SetIsOriginAllowed(origin =>
                Uri.TryCreate(origin, UriKind.Absolute, out var uri)
                && uri.IsLoopback
                && uri.Scheme is "http" or "https")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()));

var app = builder.Build();
app.MapGet("/", () => Results.Ok(new
{
    service = "AFK Raiders Online",
    environment = app.Environment.EnvironmentName,
    rulesVersion = RaidSimulation.RulesVersion
}));

if (app.Environment.IsDevelopment())
{
    app.UseCors();
    app.MapGet("/api/dev/status", (INarrativeGenerator generator) => Results.Ok(new
    {
        rulesVersion = RaidSimulation.RulesVersion,
        narrationVersion = AFKRaiders.Server.Content.NarrationContent.Default.ContentVersion,
        ai = new
        {
            generator = generator.Name,
            available = generator.IsAvailable,
            mode = generator.IsAvailable ? "Live SLM (ONNX GenAI)" : "Offline (Template Fallback)"
        }
    }));
    app.MapGet("/api/dev/catalog", (ILootCatalog catalog) => Results.Ok(new
    {
        version = catalog.ContentVersion,
        items = catalog.Items,
        pool = catalog.Pool,
        opportunities = catalog.Opportunities
    }));
    app.MapPost("/api/dev/handlers/enroll", (EnrollHandlerRequest request, RaidCoordinator raids) =>
    {
        try
        {
            return Results.Ok(raids.EnrollHandler(request.Slot, request.RequestId));
        }
        catch (RaidAccessException exception)
        {
            return Results.Conflict(new { error = exception.Reason });
        }
        catch (RaidCommandException exception)
        {
            return Results.BadRequest(new { error = exception.Reason });
        }
    });
    app.MapGet("/api/dev/handler", (HttpRequest request, RaidCoordinator raids) =>
    {
        try
        {
            return Results.Ok(raids.GetHandlerProjection(ReadAccessToken(request)));
        }
        catch (RaidAccessException)
        {
            return Results.Unauthorized();
        }
    });
    app.MapGet("/api/dev/handler/comms", (HttpRequest request, RaidCoordinator raids, int? sinceSequence, int? limit) =>
    {
        try
        {
            return Results.Ok(raids.GetHandlerComms(ReadAccessToken(request), sinceSequence ?? 0, limit ?? 50));
        }
        catch (RaidAccessException)
        {
            return Results.Unauthorized();
        }
    });
    app.MapPut("/api/dev/handler/duty", (SetDutyRequest request, HttpRequest httpRequest, RaidCoordinator raids) =>
    {
        try
        {
            return Results.Ok(raids.SetDuty(ReadAccessToken(httpRequest), request.Enabled));
        }
        catch (RaidAccessException)
        {
            return Results.Unauthorized();
        }
    });
    app.MapPost("/api/dev/handler/ready-up", (ReadyUpRequest request, HttpRequest httpRequest, RaidCoordinator raids) =>
    {
        try
        {
            return Results.Ok(raids.ReadyUp(ReadAccessToken(httpRequest), request));
        }
        catch (RaidAccessException)
        {
            return Results.Unauthorized();
        }
        catch (RaidCommandException exception)
        {
            return Results.BadRequest(new { error = exception.Reason });
        }
    });
    app.MapPost("/api/dev/handler/item/appraise", (AppraiseItemRequest request, HttpRequest httpRequest, RaidCoordinator raids) =>
    {
        try
        {
            return Results.Ok(raids.AppraiseItem(ReadAccessToken(httpRequest), request.CatalogId, request.ItemName));
        }
        catch (RaidAccessException)
        {
            return Results.Unauthorized();
        }
        catch (RaidCommandException exception)
        {
            return Results.BadRequest(new { error = exception.Reason });
        }
    });
    app.MapPost("/api/dev/handler/support/action", (HandlerSupportActionRequest request, HttpRequest httpRequest, RaidCoordinator raids) =>
    {
        try
        {
            return Results.Ok(raids.ExecuteSupportAction(ReadAccessToken(httpRequest), request));
        }
        catch (RaidAccessException)
        {
            return Results.Unauthorized();
        }
        catch (RaidCommandException exception)
        {
            return Results.BadRequest(new { error = exception.Reason });
        }
    });
    app.MapPost("/api/dev/handler/pocket/designate", (DesignateHiddenPocketRequest request, HttpRequest httpRequest, RaidCoordinator raids) =>
    {
        try
        {
            return Results.Ok(raids.DesignateHiddenPocket(ReadAccessToken(httpRequest), request));
        }
        catch (RaidAccessException)
        {
            return Results.Unauthorized();
        }
        catch (RaidCommandException exception)
        {
            return Results.BadRequest(new { error = exception.Reason });
        }
    });

    app.MapPost("/api/dev/raids", (CreateRaidRequest request, RaidCoordinator raids) =>
    {
        try
        {
            return Results.Ok(raids.CreateRaid(request.Slot));
        }
        catch (RaidCommandException exception)
        {
            return Results.BadRequest(new { error = exception.Reason });
        }
    });

    app.MapPost("/api/dev/raids/join", async (
        JoinRaidRequest request,
        RaidCoordinator raids,
        IHubContext<RaidHub> hub) =>
    {
        try
        {
            var joined = raids.JoinRaid(request.InviteCode, request.Slot);
            var current = raids.GetProjection(joined.RaidId, joined.AccessToken);
            await hub.Clients.Group(joined.RaidId).SendAsync("RaidChanged", current.Revision);
            return Results.Ok(joined);
        }
        catch (RaidAccessException exception)
        {
            return Results.BadRequest(new { error = exception.Reason });
        }
        catch (RaidCommandException exception)
        {
            return Results.BadRequest(new { error = exception.Reason });
        }
    });

    app.MapGet("/api/raids/{raidId}", (string raidId, HttpRequest request, RaidCoordinator raids) =>
    {
        try
        {
            return Results.Ok(raids.GetProjection(raidId, ReadAccessToken(request)));
        }
        catch (RaidAccessException)
        {
            return Results.Unauthorized();
        }
    });

    app.MapHub<RaidHub>("/hubs/raid");
}

app.Run();

static string ReadAccessToken(HttpRequest request)
{
    var authorization = request.Headers.Authorization.ToString();
    return authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
        ? authorization["Bearer ".Length..].Trim()
        : string.Empty;
}

public sealed record CreateRaidRequest(string Slot);

public sealed record JoinRaidRequest(string InviteCode, string Slot);
public sealed record EnrollHandlerRequest(string Slot, string RequestId);
public sealed record SetDutyRequest(bool Enabled);
public sealed record AppraiseItemRequest(string CatalogId, string? ItemName = null);

public partial class Program;
