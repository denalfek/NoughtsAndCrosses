using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using NoughtsAndCrosses.Application.Services.Interfaces;
using NoughtsAndCrosses.Infrastructure.Data.Entities;

namespace NoughtsAndCrosses.API.Controllers;

public static class Game
{
    public static void Controllers(this WebApplication app)
    {
        app.MapPost(
            "/api/game",
            async (
                InitializeGameRequest request,
                [FromServices]IGameService gameService,
                HttpContext ctx,
                CancellationToken ct) =>
            {
                var result = await gameService.InitializeAsync(((User)ctx.User).Id, request.Side, ct);
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
                if (!ObjectId.TryParse(gameId, out var id)) { return Results.BadRequest("Invalid game id"); }
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
                var result =
                    await gameService.ProcessAsync(gameId, ((User)ctx.User).Id, request.CellId, ct);
                if (result.IsT1) { return Results.BadRequest(result.AsT1.Value); }
                var game = result.AsT0;
                return game.Winner is { } winner
                    ? Results.Ok(new ProcessGameResponse(new GameResponse(game.Id.ToString(), game.Field), winner.Side))
                    : Results.Ok(new ProcessGameResponse(new GameResponse(game.Id.ToString(), game.Field), null)); 
            });
    }
}