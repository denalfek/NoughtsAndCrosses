using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
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
    "/api/game/initialization",
    async (PlayerSide side, HttpContext context, IMongoClient mongoClient, ILogger logger) =>
    {
        var user = (User)context.User;
        switch (side)
        {
            case PlayerSide.Cross:
            {
                logger.LogInformation("Test");
                var db = mongoClient
                    .GetDatabase(MongoConfig.DatabaseName);
                var game = new Game(new []{ user });
                var gameCollection = db.GetCollection<Game>(nameof(Game));
                await gameCollection.InsertOneAsync(game);
                return;
            }
            case PlayerSide.Nought:
            {
                logger.LogInformation("Test bot first");
                return;
            }
            default:
                logger.LogError(
                    "Unsupported player side: {Side}",
                    side);
                throw new ArgumentOutOfRangeException(
                    nameof(PlayerSide),
                    "Unsupported player side");
        }
    });

app.UseMiddleware<InitializeUserMiddleware>();
app.Run();
