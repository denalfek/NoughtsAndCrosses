using NoughtsAndCrosses.API;
using NoughtsAndCrosses.API.Controllers;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables(prefix: "NOUGHTS_AND_CROSSES_API_");
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
const string corsPolicyName = "myCorsPolicy";
builder.Services.AddCors(opts =>
{
    opts.AddPolicy(corsPolicyName, policyBuilder =>
    {
        // extend for production
        policyBuilder
            .WithOrigins("http://localhost:3001")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.BuildServices();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Controllers();
app.UseMiddleware<InitializeUserMiddleware>();
app.UseCors(corsPolicyName);
app.Run();
