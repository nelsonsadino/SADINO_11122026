using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Tally.Api.Swagger;

// Describes the X-API-Key header in the OpenAPI document, so Swagger UI shows an Authorize button.
public sealed class ApiKeySecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    private const string SchemeName = "ApiKey";

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[SchemeName] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Header,
            Name = "X-API-Key",
        };

        document.Security ??= new List<OpenApiSecurityRequirement>();
        document.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(SchemeName, document)] = new List<string>(),
        });

        return Task.CompletedTask;
    }
}
