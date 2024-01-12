using MongoDB.Driver;
using NoughtsAndCrosses.Application;
using NoughtsAndCrosses.Application.Services;
using NoughtsAndCrosses.Application.Services.Interfaces;
using NoughtsAndCrosses.Infrastructure.Data.Configs;

namespace NoughtsAndCrosses.API;

public static class WebAppBuilder
{
    public static void BuildServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<MongoConfigurationOptions>(
            provider =>
            {
                if (provider.GetRequiredService<IConfiguration>()
                        .GetSection("MongoDb")
                        .Get<MongoConfigurationOptions>()
                    is { } config) { return config; }
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
        
        builder.Services.AddSingleton<GameConfiguration>(provider =>
        {
            if (provider.GetRequiredService<IConfiguration>().GetSection(nameof(GameConfiguration))
                    .Get<GameConfiguration>()
                is { } config) { return config; }
            Console.WriteLine("Game configuration is null");
            throw new Exception("Game configuration is null");
        });
        builder.Services.AddTransient<IBot, Bot>();
        builder.Services.AddScoped<IGameService, GameService>();
    }
}