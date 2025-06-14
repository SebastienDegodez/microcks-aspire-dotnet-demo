using Aspire.Hosting.Microcks;

var builder = DistributedApplication.CreateBuilder(args);

var microcks = builder.AddMicrocks("microcks")
    .WithMainArtifacts(
        "resources/third-parties/apipastries-openapi.yaml"
    );


builder.AddProject<Projects.Order_Api>("Order-Api");


builder.Build().Run();

