using MongoDB.Bson;
using NoughtsAndCrosses.Infrastructure.Data.Entities;
using OneOf;
using OneOf.Types;

namespace NoughtsAndCrosses.Application.Services.Interfaces;

using ProcessGameResponse = OneOf<Game, Error<string>>;

public interface IGameService
{
    Task<ProcessGameResponse> ProcessAsync(
        ObjectId gameId,
        User user,
        int cellId,
        CancellationToken ct = default);
}