using MongoDB.Bson;
using MongoDB.Driver;
using NoughtsAndCrosses.API;
using NoughtsAndCrosses.API.Configs;
using NoughtsAndCrosses.Infrastructure.Data.Entities;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddSingleton<IMongoClient>(
        new MongoClient(builder.Configuration.GetConnectionString(MongoConfig.ConnectionStringName)));

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
    async (string gameId, HttpContext ctx, IMongoClient client, ILoggerFactory loggerFactory, CancellationToken ct) =>
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

app.UseMiddleware<InitializeUserMiddleware>();
app.Run();
