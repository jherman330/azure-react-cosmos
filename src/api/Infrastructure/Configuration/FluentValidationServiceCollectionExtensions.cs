using System.Text.Json;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Todo.Api.Application.Validation;

namespace Todo.Api.Infrastructure.Configuration;

/// <summary>
/// Registers FluentValidation with automatic validation before controller actions (AC-FOUNDATION-008.1, 008.2, 008.5).
/// </summary>
public static class FluentValidationServiceCollectionExtensions
{
    private const string ValidationSummaryMessage = "One or more validation errors occurred.";

    /// <summary>
    /// Adds MVC controllers, FluentValidation auto-validation, assembly scanning for validators,
    /// and HTTP 400 responses in the standard API envelope format (AC-FOUNDATION-008.3, 008.4).
    /// </summary>
    public static IServiceCollection AddFluentValidationPipeline(this IServiceCollection services)
    {
        services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = new Dictionary<string, List<string>>(StringComparer.Ordinal);
                    foreach (var (key, entry) in context.ModelState)
                    {
                        if (entry?.Errors is not { Count: > 0 })
                            continue;
                        var fieldKey = ToCamelCaseModelStateKey(string.IsNullOrEmpty(key) ? "_" : key);
                        if (!errors.TryGetValue(fieldKey, out var list))
                        {
                            list = new List<string>();
                            errors[fieldKey] = list;
                        }

                        foreach (var err in entry.Errors)
                        {
                            var msg = string.IsNullOrWhiteSpace(err.ErrorMessage)
                                ? "The value is invalid."
                                : err.ErrorMessage;
                            list.Add(msg);
                        }
                    }

                    var traceId = context.HttpContext.TraceIdentifier;
                    var envelope = new ApiValidationErrorEnvelope(
                        traceId,
                        ErrorCodes.ValidationFailed,
                        ValidationSummaryMessage,
                        errors.ToDictionary(k => k.Key, k => k.Value.ToArray()));

                    return new BadRequestObjectResult(envelope);
                };
            });

        services.AddFluentValidationAutoValidation(configuration =>
        {
            configuration.DisableDataAnnotationsValidation = true;
        });

        // Inbound request validators only (Application.Transport + TransportValidatorBase<T>); not domain entities.
        services.AddValidatorsFromAssemblyContaining<ValidationAssemblyMarker>();

        ValidatorOptions.Global.DisplayNameResolver = (_, member, _) => member?.Name;

        return services;
    }

    /// <summary>
    /// Converts ModelState keys (e.g. <c>Message</c>, <c>request.Name</c>) to camelCase segments for JSON APIs.
    /// </summary>
    private static string ToCamelCaseModelStateKey(string key)
    {
        var parts = key.Split('.');
        var converted = new string[parts.Length];
        for (var i = 0; i < parts.Length; i++)
        {
            var part = parts[i];
            var bracket = part.IndexOf('[', StringComparison.Ordinal);
            if (bracket >= 0)
            {
                var name = part[..bracket];
                var suffix = part[bracket..];
                converted[i] = JsonNamingPolicy.CamelCase.ConvertName(name) + suffix;
            }
            else
            {
                converted[i] = JsonNamingPolicy.CamelCase.ConvertName(part);
            }
        }

        return string.Join('.', converted);
    }
}
