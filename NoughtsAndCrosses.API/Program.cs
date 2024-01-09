using MongoDB.Bson;
using MongoDB.Driver;
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
    "game",
    async (PlayerSide side, HttpContext context, IMongoClient mongoClient) =>
    {
        if (context.Request.Cookies["user_id"] is { } rawUserId
            && ObjectId.TryParse(rawUserId, out var userId))
        {
            
        }
        

        var db = mongoClient
            .GetDatabase(MongoConfig.DatabaseName);
        var playerCollection = db.GetCollection<Player>(nameof(Player));
        var player = new Player();
        await playerCollection.InsertOneAsync(player);
        
        var game = new Game(new []{ player });
        var gameCollection = db.GetCollection<Game>(nameof(Game));
        await gameCollection.InsertOneAsync(game);
    });

app.Run();
