using Harness.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.RegisterConsumers(builder.Configuration);

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapOpenApi();
// app.UseHttpsRedirection();

app.MapGet("/ping", () => "Pong");

app.Run();
