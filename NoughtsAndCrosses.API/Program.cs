using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using NoughtsAndCrosses.API;
using NoughtsAndCrosses.Application.Services;
using NoughtsAndCrosses.Application.Services.Interfaces;
using NoughtsAndCrosses.Infrastructure.Data.Configs;
using NoughtsAndCrosses.Infrastructure.Data.Entities;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddSingleton<IMongoClient>(
        new MongoClient(builder.Configuration.GetConnectionString(MongoConfig.ConnectionStringName)));
builder.Services.AddTransient<IBot, Bot>();
builder.Services.AddScoped<IGameService, GameService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost(
    "/api/game",
    async (
        InitializeGameRequest request,
        HttpContext context,
        IMongoClient mongoClient,
        ILoggerFactory loggerFactory,
        CancellationToken ct) =>
    {
        var logger = loggerFactory.CreateLogger("POST:/api/game");
        var user = (User)context.User;
        switch (request.Side)
        {
            case PlayerSide.Cross:
            {
                logger.LogInformation("Test");
                var db = mongoClient
                    .GetDatabase(MongoConfig.DatabaseName);
                var game = new Game(new []{ user });
                var gameCollection = db.GetCollection<Game>(nameof(Game));
                await gameCollection.InsertOneAsync(game, cancellationToken: ct);
                return Results.Ok(new GameResponse(game.Id.ToString(), game.Field));
            }
            case PlayerSide.Nought:
            {
                logger.LogInformation("Test bot first");
                return Results.Ok();
            }
            default:
                logger.LogError(
                    "Unsupported player side: {Side}",
                    request.Side);
                throw new ArgumentOutOfRangeException(
                    nameof(PlayerSide),
                    "Unsupported player side");
        }
    });
app.MapGet(
    "/api/game",
    async (
        [Required][FromQuery]string gameId,
        HttpContext ctx,
        IMongoClient client,
        ILoggerFactory loggerFactory,
        CancellationToken ct) =>
    {
        if (!ObjectId.TryParse(gameId, out var id))
        {
            return Results.BadRequest("Invalid game id");
        }
        
        var logger = loggerFactory.CreateLogger("GET:/api/game");
        var user = (User)ctx.User;
        var gameCollection = client.GetDatabase(MongoConfig.DatabaseName).GetCollection<Game>(nameof(Game));

        if (await gameCollection
                .Find(x => x.Id == id)
                .FirstOrDefaultAsync(ct) is not { WinnerId: null } game)
        {
            return Results.BadRequest("Game not found");
        }
        
        if (game.Players.Any(p => p.Id == user.Id))
        {
            return Results.Ok(new GameResponse(game.Id.ToString(), game.Field));
        }
            
        logger.LogInformation(
            "User {UserId} trying to access game {GameId}",
            user.Id,
            id);
        return Results.BadRequest("Game not found");

    });
app.MapPatch(
    "/api/game",
    async (
        [Required][FromBody]HitRequest request,
        HttpContext ctx,
        [FromServices]IGameService gameService,
        CancellationToken ct) =>
    {
        if (!ObjectId.TryParse(request.GameId, out var gameId))
        {
            return Results.BadRequest("Invalid game id");
        }
        
        var user = (User)ctx.User;
        var result = await gameService.ProcessAsync(
            gameId,
            user,
            request.CellId,
            ct);

        return result.IsT0
            ? Results.Ok(new GameResponse(result.AsT0.Id.ToString(), result.AsT0.Field))
            : Results.BadRequest(result.AsT1.Value);
    });
app.UseMiddleware<InitializeUserMiddleware>();
app.Run();
