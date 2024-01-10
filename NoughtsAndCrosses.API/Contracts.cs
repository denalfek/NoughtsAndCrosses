using MongoDB.Bson;
using NoughtsAndCrosses.Infrastructure.Data.Entities;
// ReSharper disable ClassNeverInstantiated.Global

namespace NoughtsAndCrosses.API;

public record GameResponse(string GameId, Cell[] Field);

public record InitializeGameRequest(PlayerSide Side);
