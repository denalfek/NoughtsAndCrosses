using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using NoughtsAndCrosses.Application.Services.Interfaces;
using NoughtsAndCrosses.Infrastructure.Data.Configs;
using NoughtsAndCrosses.Infrastructure.Data.Entities;
using OneOf;
using OneOf.Types;


namespace NoughtsAndCrosses.Application.Services;

public class GameService : IGameService
{
    private readonly ILogger<GameService> _logger;
    private readonly IMongoCollection<Game> _gameCollection;
    private readonly IBot _bot;
    
    public GameService(IMongoClient mongoClient, ILogger<GameService> logger, IBot bot)
    {
        _logger = logger;
        _bot = bot;
        var db = mongoClient.GetDatabase(MongoConfig.DatabaseName);
        _gameCollection = db.GetCollection<Game>(nameof(Game));
    }

    public async Task<OneOf<Game, Error<string>>> InitializeAsync(
        ObjectId userId,
        PlayerSide side,
        CancellationToken ct = default)
    {
        var game = new Game(new []{ new Gamer
        {
            Id = userId,
            Side = side,
        } });
        
        if (side == PlayerSide.Nought)
        {
            var botHit = await _bot.HitAsync(game.Field, PlayerSide.Cross);
            game.Field[botHit.Id].Side = PlayerSide.Cross;
        }

        await _gameCollection.InsertOneAsync(game, cancellationToken: ct);
        return game;
    }

    public async Task<OneOf<Game, Error<string>>> ResumeAsync(ObjectId gameId, ObjectId userId, CancellationToken ct = default)
    {
        if (await _gameCollection
                .Find(x => x.Id == gameId)
                .FirstOrDefaultAsync(ct) is not { WinnerId: null } game)
        {
            return new Error<string>("Game not found or has finished already");
        }

        if (game.Gamers.Any(p => p.Id == userId))
        {
            return game;
        }
        
        _logger.LogInformation(
            "User {UserId} trying to access game {GameId} he doesn't belong to",
            userId,
            gameId);
            
        return new Error<string>("Game not found");
    }

    public async Task<OneOf<Game, Error<string>>> ProcessAsync(
        ObjectId gameId,
        ObjectId userId,
        int cellId,
        CancellationToken ct = default)
    {
        // ReSharper disable once MergeIntoPattern
        if (await Sanitize(gameId, userId, cellId, ct) is var result && result.IsT1)
        {
            return result;
        }
        
        var game = result.AsT0;
        var cell = game.Field[cellId];
        if (cell.Side is not null)
        {
            return new Error<string>("Cell is hit");
        }
        
        if (game.Gamers.FirstOrDefault(x => x.Id == userId) is not { } user)
        {
            _logger.LogInformation(
                "User {UserId} trying to access game {GameId}",
                userId,
                gameId);

            return new Error<string>("Something went wrong");
        }
        
        var botSide = user.Side == PlayerSide.Cross ? PlayerSide.Nought : PlayerSide.Cross;
        game.Field[cellId].Side = user.Side;
        await _gameCollection.UpdateOneAsync(
            x => x.Id == gameId,
            Builders<Game>.Update.Set(x => x.Field, game.Field),
            cancellationToken: ct);
        if (game.Field.Count(x => x.Side == user.Side) >= 3 && CheckWinner(game.Field, user.Side))
        {
            game.WinnerId = user.Id;
            game.FinishTime = DateTime.UtcNow;
            await _gameCollection.UpdateOneAsync(
                x => x.Id == gameId,
                Builders<Game>.Update.Set(x => x.WinnerId, user.Id),
                cancellationToken: ct);
            return game;
        }
        
        var botHit = await _bot.HitAsync(game.Field, botSide);
        game.Field[botHit.Id].Side = botSide;
        await _gameCollection.UpdateOneAsync(
            x => x.Id == gameId,
            Builders<Game>.Update.Set(x => x.Field, game.Field),
            cancellationToken: ct);

        if (game.Field.Count(x => x.Side == botSide) < 3 || !CheckWinner(game.Field, botSide))
        {
            return game;
        }
        
        game.FinishTime = DateTime.UtcNow;
        await _gameCollection.UpdateOneAsync(
            x => x.Id == gameId,
            Builders<Game>.Update.Set(x => x.WinnerId, ObjectId.Empty),
            cancellationToken: ct);
        return game;
    }

    private async Task<OneOf<Game, Error<string>>> Sanitize(
        ObjectId gameId,
        ObjectId userId,
        int cellId,
        CancellationToken ct = default)
    {
        if (await _gameCollection
                .Find(x => x.Id == gameId)
                .FirstOrDefaultAsync(ct) is not { WinnerId: null } game)
        {
            return new Error<string>("Game not found or has finished already");
        }
        
        if (game.Gamers.FirstOrDefault(x => x.Id == userId) is not { } user)
        {
            _logger.LogInformation(
                "User {UserId} trying to access game {GameId}",
                userId,
                gameId);
            return new Error<string>("Something went wrong");
        }
        
        if (game.Gamers.All(p => p.Id != user.Id))
        {
            _logger.LogInformation(
                "User {UserId} trying to access game {GameId}",
                user.Id,
                gameId);
            return new Error<string>("Game not found");
        }
        
        if (game.Field.Length <= cellId)
        {
            return new Error<string>("Invalid cell id");
        }
        
        if (game.Field.All(c => c.Side is not null))
        {
            return new Error<string>("Game finished");
        }

        return game;
    }
    
    private static bool CheckWinner(Cell[] field, PlayerSide side)
    {
      // Check rows
      for (int i = 0; i < 3; i++)
      {
          if (field[i * 3].Side == side && field[i * 3 + 1].Side == side && field[i * 3 + 2].Side == side)
              return true;
      }

      // Check columns
      for (int i = 0; i < 3; i++)
      {
          if (field[i].Side == side && field[i + 3].Side == side && field[i + 6].Side == side)
              return true;
      }

      // Check diagonals
      if (field[0].Side == side && field[4].Side == side && field[8].Side == side)
          return true;

      if (field[2].Side == side && field[4].Side == side && field[6].Side == side)
          return true;

      return false;      
    }
}