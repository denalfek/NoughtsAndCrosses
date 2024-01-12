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
        if (config is not null)
        {
            return config;
        }
        
        Console.WriteLine("MongoDb configuration is null");
        throw new Exception("MongoDb configuration is null");
    });
builder.Services
    .AddSingleton<IMongoClient>(
        provider =>
        {
            var connStr = builder.Environment.IsDevelopment()
                ? provider.GetRequiredService<MongoConfigurationOptions>().ConnectionStringDev
                : provider.GetRequiredService<MongoConfigurationOptions>().ConnectionStringProd;
            
            Console.WriteLine($"Environment is dev: {builder.Environment.IsDevelopment()}");
            Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
            Console.WriteLine($"Connection string: {connStr}");
            return new MongoClient(connStr);
        });
builder.Services.AddTransient<IBot, Bot>();
builder.Services.AddScoped<IGameService, GameService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
const string corsPolicyName = "myCorsPolicy";
builder.Services.AddCors(opts =>
{
    opts.AddPolicy(corsPolicyName, policyBuilder =>
    {
        policyBuilder
            .WithOrigins("http://localhost:3001")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
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
            //((User)ctx.User).Id,
            ObjectId.Parse("65a01c32481b96493ddc6351"),
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

        var result = await gameService.ResumeAsync(id, ObjectId.Parse("65a01c32481b96493ddc6351"), ct);
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
        Console.WriteLine("Hit request");
        
        if (!ObjectId.TryParse(request.GameId, out var gameId))
        {
            return Results.BadRequest("Invalid game id");
        }
        
//        var gamer = (User)ctx.User;
        var result = await gameService.ProcessAsync(
            gameId,
            ObjectId.Parse("65a01c32481b96493ddc6351"),
            request.CellId,
            ct);

        if (result.IsT1)
        {
            return Results.BadRequest(result.AsT1.Value);
        }
        
        var game = result.AsT0;
        if (game.WinnerId is { } winnerId)
        {
            return Results.Ok(new ProcessGameResponse(new GameResponse(game.Id.ToString(), game.Field), PlayerSide.Cross));
        }

        return Results.Ok(new ProcessGameResponse(new GameResponse(game.Id.ToString(), game.Field), null));
    });
//app.UseMiddleware<InitializeUserMiddleware>();
app.UseCors(corsPolicyName);
app.Run();
