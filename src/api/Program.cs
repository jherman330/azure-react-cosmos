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

using Todo.Api.Domain.Entities;
using Todo.Api.Infrastructure.Configuration;

var builder = WebApplication.CreateBuilder(args);

// TODO: Register Application services

// Cosmos DB client (session consistency, RU monitoring) and repository pattern — see Domain/Repositories/IRepository.cs
builder.Services.AddCosmosDbClient(builder.Configuration);
// AC-FOUNDATION-002.7: at least one repository registered and injectable when Cosmos is configured
if (!string.IsNullOrEmpty(builder.Configuration["AZURE_COSMOS_ENDPOINT"]))
{
    var db = builder.Configuration["AZURE_COSMOS_DATABASE_NAME"] ?? "App";
    var container = builder.Configuration["AZURE_COSMOS_CONTAINER_NAME"] ?? "Items";
    builder.Services.AddCosmosDbRepository<Item>(db, container, partitionKeyPath: "/id");
}
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
