using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TransferPlatform.Api.Authentication;
public class ApiKeyAuthAttribute : Attribute, IAsyncActionFilter
{
    private const string ApiKeyHeader = "X-API-KEY";
    private const string ApiSecretHeader = "X-API-SECRET";

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var configuration = context.HttpContext
            .RequestServices
            .GetRequiredService<IConfiguration>();

        var expectedApiKey = configuration["Auth:ApiKey"];
        var expectedApiSecret = configuration["Auth:ApiSecret"];

        var request = context.HttpContext.Request;

        if (!request.Headers.TryGetValue(ApiKeyHeader, out var apiKey) ||
            !request.Headers.TryGetValue(ApiSecretHeader, out var apiSecret))
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                message = "Missing API credentials"
            });

            return;
        }

        if (apiKey != expectedApiKey ||
            apiSecret != expectedApiSecret)
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                message = "Invalid API credentials"
            });

            return;
        }

        await next();
    }
}