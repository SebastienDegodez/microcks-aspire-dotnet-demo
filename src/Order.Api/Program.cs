var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();

// This partial class is used to reference the assembly in tests and other projects.
public partial class Program { }