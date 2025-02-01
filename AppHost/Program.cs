using AppHost.Initializers;
using Microsoft.Extensions.DependencyInjection;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

    var kafka = builder.AddKafka("kafka")
        // .WithEnvironment("KAFKA_AUTO_CREATE_TOPICS_ENABLE", "true")
        .WithEndpoint(port:9092, targetPort:9092, name:"kafka")
        .WithKafkaUI();

builder.Services.AddHostedService<KafkaTopicInitializer>();

var app = builder.AddProject<Harness>("retryableconsumer")
    .WithReference(kafka)
        .WaitFor(kafka)
        .WithArgs(kafka.Resource.Name);



builder.Build().Run();