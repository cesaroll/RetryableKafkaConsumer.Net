using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var kafka = builder.AddKafka("aspirekafka")
    //.WithEndpoint(port: 9092, targetPort: 9092, name: "kafka") // ConnectionStrings__aspirekafka=aspirekafka:9092
    .WithKafkaUI();
    
var app = builder.AddProject<Harness>("aspireconsumer")
    .WithReference(kafka)
        .WaitFor(kafka)
        .WithArgs(kafka.Resource.Name);

builder.Build().Run();