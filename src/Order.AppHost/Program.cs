using Aspire.Hosting.Microcks;

var builder = DistributedApplication.CreateBuilder(args);

var microcks = builder.AddMicrocks("microcks")
    .WithMainArtifacts(
        "resources/third-parties/apipastries-openapi.yaml"
    )
    .WithEnvironment("OTEL_JAVAAGENT_ENABLED", "true")
    .WithOtlpExporter();


var orderapi = builder.AddProject<Projects.Order_Api>("Order-Api")
    .WithHttpEndpoint(targetPort: 8001, name: "http-order-api")
    .WithReference(microcks);

microcks.WithReference(orderapi);

builder.Build().Run();

