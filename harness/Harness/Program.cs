using Harness.Extensions;
using Harness.Models;
using Harness.Producers;
using Harness.Producers.Configs;
using Microsoft.AspNetCore.Mvc;

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

app.MapGet("kafka/host",
    (
        ProducerServiceConfig config) => Results.Ok(new { host = config.Host })
    );

app.MapPost("/kafka/produce", 
    async (
        [FromServices]
        IKafkaProducerService<TestMessage> producer, 
        [FromBody]
        TestMessage message) =>
{
    await producer.ProduceAsync(message);
    return Results.Ok();
});

app.Run();
