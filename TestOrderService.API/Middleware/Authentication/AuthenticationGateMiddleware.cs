using UnauthorizedAccessException=TestOrderService.Application.Exceptions.UnauthorizedAccessException;
namespace TestOrderService.API.Middleware.Authentication
{
    public class AuthenticationGateMiddleware(RequestDelegate next, ILogger<AuthenticationGateMiddleware> logger)
    {

        private readonly ILogger<AuthenticationGateMiddleware> _logger = logger;
        private readonly RequestDelegate _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            var status = context.Items["AuthResultStatus"] as string ?? "";

            switch (status)
            {
                case "NoResult":
                    _logger.LogInformation("Authentication returned NoResult. Skipping authorization.");
                    context.Items["SkipAuthorization"] = true;
                    break;
                case "Fail":
                    var failureMessage = context.Items["AuthResultMessage"] as string ?? "";
                    throw new UnauthorizedAccessException(failureMessage);
            }

            await _next(context);
        }
    }
}
