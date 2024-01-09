using System.Net;
using MongoDB.Bson;
using MongoDB.Driver;
using NoughtsAndCrosses.API.Configs;
using NoughtsAndCrosses.Infrastructure.Data.Entities;

namespace NoughtsAndCrosses.API;

public class InitializeUserMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<InitializeUserMiddleware> _logger;
    private readonly IMongoCollection<User> _collection;
    private readonly CancellationToken _ct = new();

    private const string AnonymousPath = "/api/game/initialization";
    private const string AnonymousMethod = "POST";

    public InitializeUserMiddleware(
        RequestDelegate next,
        ILogger<InitializeUserMiddleware> logger,
        IMongoClient client)
    {
        _next = next;
        _logger = logger;
        _collection = client.GetDatabase(MongoConfig.DatabaseName).GetCollection<User>(nameof(User));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var rawUserId = context.Request.Headers["UserId"].FirstOrDefault();
        var correctId = ObjectId.TryParse(rawUserId, out var userId);
        if (context.Request.Path == AnonymousPath && context.Request.Method == AnonymousMethod)
        {
            User user;
            if (correctId &&
                await _collection.Find(x => x.Id == userId).FirstOrDefaultAsync(_ct) is { } existingUser)
            {
                user = existingUser;
            }
            else
            {
                user = new User();
                await _collection.InsertOneAsync(user, cancellationToken: _ct);
                context.Response.Cookies.Append("UserId", user.Id.ToString());
            }

            context.User = user;
        }
        else
        {
            if (correctId &&
                await _collection.Find(x => x.Id == userId).FirstOrDefaultAsync(_ct) is { } existingUser)
            {
                context.User = existingUser;
            }
            else
            {
                         
                _logger.LogError(
                    "Somebody trying to access method {Method} {Path} without userId in cookies",
                    context.Request.Method,
                    context.Request.Path);
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;   
            }
        }
        
        await _next(context);
    }
}