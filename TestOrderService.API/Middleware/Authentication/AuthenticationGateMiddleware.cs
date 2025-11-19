using UnauthorizedAccessException=TestOrderService.Application.Exceptions.UnauthorizedAccessException;
namespace TestOrderService.API.Middleware.Authentication
{
    /// <summary>
    ///     Authentication Gate Middleware implement.
    /// </summary>
    public class AuthenticationGateMiddleware(RequestDelegate next, ILogger<AuthenticationGateMiddleware> logger)
    {

        /// <summary>
        ///     The logger
        /// </summary>
        private readonly ILogger<AuthenticationGateMiddleware> _logger = logger;
        /// <summary>
        ///     The next
        /// </summary>
        private readonly RequestDelegate _next = next;

        /// <summary>
        ///     Invokes the asynchronous.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="Application.Exceptions.UnauthorizedAccessException"></exception>
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
