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
builder.Configuration.AddEnvironmentVariables(prefix: "NOUGHTS_AND_CROSSES_API_");
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
builder.Services.AddSingleton<MongoConfigurationOptions>(
    provider =>
    {
        var configuration = provider.GetRequiredService<IConfiguration>();
        var config = configuration.GetSection("MongoDb").Get<MongoConfigurationOptions>();

        if (config is null)
        {
            Console.WriteLine("MongoDb configuration is null");
            throw new Exception("MongoDb configuration is null");
        }

        var connectionString = builder.Environment.IsProduction()
            ? config.ConnectionStringProd
            : config.ConnectionStringDev;

        Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
        Console.WriteLine($"MongoDb connection string: {connectionString}");
        return config;
    });
builder.Services
    .AddSingleton<IMongoClient>(
        provider => new MongoClient(builder.Environment.IsDevelopment()
            ? provider.GetRequiredService<MongoConfigurationOptions>().ConnectionStringDev
            : provider.GetRequiredService<MongoConfigurationOptions>().ConnectionStringProd));
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
        [FromServices]IGameService gameService,
        HttpContext ctx,
        CancellationToken ct) =>
    {
        var result = await gameService.InitializeAsync(
            ((User)ctx.User).Id,
            request.Side,
            ct);

        return result.IsT0
            ? Results.Ok(new GameResponse(result.AsT0.Id.ToString(), result.AsT0.Field))
            : Results.BadRequest(result.AsT1.Value);
    });
app.MapGet(
    "/api/game",
    async (
        [Required][FromQuery]string gameId,
        [FromServices]IGameService gameService,
        HttpContext ctx,
        CancellationToken ct) =>
    {
        if (!ObjectId.TryParse(gameId, out var id))
        {
            return Results.BadRequest("Invalid game id");
        }

        var result = await gameService.ResumeAsync(id, ((User)ctx.User).Id, ct);
        return result.IsT0
            ? Results.Ok(new GameResponse(result.AsT0.Id.ToString(), result.AsT0.Field))
            : Results.BadRequest(result.AsT1.Value);
    });
app.MapPatch(
    "/api/game",
    async (
        [Required][FromBody]HitRequest request,
        [FromServices]IGameService gameService,
        HttpContext ctx,
        CancellationToken ct) =>
    {
        if (!ObjectId.TryParse(request.GameId, out var gameId))
        {
            return Results.BadRequest("Invalid game id");
        }
        
        var gamer = (User)ctx.User;
        var result = await gameService.ProcessAsync(
            gameId,
            gamer.Id,
            request.CellId,
            ct);

        return result.IsT0
            ? Results.Ok(new GameResponse(result.AsT0.Id.ToString(), result.AsT0.Field))
            : Results.BadRequest(result.AsT1.Value);
    });
app.UseMiddleware<InitializeUserMiddleware>();
app.Run();
