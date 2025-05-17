var builder = DistributedApplication.CreateBuilder(args);


builder.AddProject<Projects.Order_Api>("Order-Api");

builder.Build().Run();
