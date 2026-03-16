// =============================================================================
// Clean Architecture Layer Structure
// =============================================================================
// This solution follows Clean Architecture. Dependencies flow inward:
//   API (Controllers, DTOs) → Application (Services, Interfaces) → Domain (Entities, Interfaces)
//   Infrastructure (Repositories, Configuration) → Domain
//
// Layer directories:
//   Api/         — HTTP entry point; controllers and DTOs (see Api/README.md)
//   Application/ — Use cases, services, application interfaces (see Application/README.md)
//   Domain/      — Entities and domain interfaces; no outward dependencies (see Domain/README.md)
//   Infrastructure/ — Repositories, configuration, external services (see Infrastructure/README.md)
//
// This file is the Composition Root: dependencies are wired here, middleware is
// configured, and the application is bootstrapped. Register Infrastructure and
// Application services below as those layers are implemented.
// =============================================================================

using Azure.Identity;
using Microsoft.Azure.Cosmos;

var credential = new DefaultAzureCredential();
var builder = WebApplication.CreateBuilder(args);

// TODO: Register Infrastructure services — see Infrastructure/Configuration
// TODO: Register Application services

// Cosmos client: used when AZURE_COSMOS_* is set (e.g. by infra). Add your own repositories and wire them here.
builder.Services.AddSingleton(_ => new CosmosClient(builder.Configuration["AZURE_COSMOS_ENDPOINT"], credential, new CosmosClientOptions()
{
    SerializerOptions = new CosmosSerializationOptions
    {
        PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
    }
}));
builder.Services.AddCors();
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

app.UseCors(policy =>
{
    policy.AllowAnyOrigin();
    policy.AllowAnyHeader();
    policy.AllowAnyMethod();
});

// Swagger UI
app.UseSwaggerUI(options => {
    options.SwaggerEndpoint("./openapi.yaml", "v1");
    options.RoutePrefix = "";
});

app.UseStaticFiles(new StaticFileOptions{
    // Serve openapi.yaml file
    ServeUnknownFileTypes = true,
});

app.MapGet("/", () => Results.Ok("OK"));
app.MapGet("/health", () => Results.Ok());
app.Run();
