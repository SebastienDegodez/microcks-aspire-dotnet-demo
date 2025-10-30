using Aspire.Hosting.Microcks;

var builder = DistributedApplication.CreateBuilder(args);

var microcks = builder.AddMicrocks("microcks")
    .WithMainArtifacts(
        "resources/third-parties/apipastries-openapi.yaml",
        "resources/order-service-openapi.yaml"
    )
    .WithSecondaryArtifacts(
        "resources/order-service-postman-collection.json"
    )
    .WithEnvironment("OTEL_JAVAAGENT_ENABLED", "true")
    .WithOtlpExporter();


var orderapi = builder.AddProject<Projects.Order_Service>("Order-Api")
    .WithHttpEndpoint(name: "http-order-api")
    .WithEnvironment("PastryApi:BaseUrl", () =>
    { // Callback to get the URL once Microcks is started
        var pastryBaseUrl = microcks.Resource.GetRestMockEndpoint("API+Pastries", "0.0.1");

        return pastryBaseUrl.ToString();
    })
    .WithReference(microcks);

microcks.WithReference(orderapi);

builder.Build().Run();

