using System.ComponentModel.DataAnnotations;
using NoughtsAndCrosses.Infrastructure.Data.Entities;
// ReSharper disable ClassNeverInstantiated.Global

namespace NoughtsAndCrosses.API;

public record GameResponse(string GameId, Cell[] Field);

public record InitializeGameRequest(PlayerSide Side = PlayerSide.Cross);

public record HitRequest([Required]string GameId, [Required]int CellId);
