using Confluent.Kafka;
using Harness.Consumers.Handlers;
using RetryableKafkaConsumer.Contracts.Configs;
using RetryableKafkaConsumer.Setup;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

//builder.Services.RegisterTestHostedService();

// void ConfigureKafkaConsumer()
// {
//     // builder.Services.AddSingleton<TestHandler>();
//     //
//     // var config = builder.Configuration
//     //     .GetRequiredSection("RetryableKafkaConsumer")
//     //     .Get<RetryableConsumerConfig>()!;
//
//     builder.Services.RegisterTestHostedService();
// }

builder.Services.AddSingleton<TestHandler>();

var config = builder.Configuration
    .GetRequiredSection("RetryableKafkaConsumer")
    .Get<RetryableConsumerConfig>()!;

builder.Services.RegisterRetryableConsumer<Ignore, string, TestHandler>(config);

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
    app.MapOpenApi();
// }

// app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}