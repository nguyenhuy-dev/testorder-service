using System.Text.Json;
using System.Text.Json.Serialization;
using TestOrderService.Application.Exceptions;
using ApplicationException=TestOrderService.Application.Exceptions.ApplicationException;
using UnauthorizedAccessException=TestOrderService.Application.Exceptions.UnauthorizedAccessException;
namespace TestOrderService.API.Middleware
{
    /// <summary>
    ///     Global exception custom.
    /// </summary>
    public class GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {

        /// <summary>
        ///     The s write options
        /// </summary>
        private static readonly JsonSerializerOptions s_writeOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        ///     The logger
        /// </summary>
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger = logger;

        /// <summary>
        ///     The next
        /// </summary>
        private readonly RequestDelegate _next = next;

        /// <summary>
        ///     Invokes the asynchronous.
        /// </summary>
        /// <param name="context">The context.</param>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        ///     Handles the exception asynchronous.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <param name="ex">The ex.</param>
        private async Task HandleExceptionAsync(HttpContext httpContext, Exception ex)
        {
            var statusCode = GetStatusCode(ex);

            var response = new ErrorResponse
            {
                StatusCode = statusCode,
                Message = ex.Message,
                Title = GetTitle(ex),
                Errors = GetErrors(ex)
            };

            WriteLog(ex, response.Errors);

            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = statusCode;

            var jsonResponse = JsonSerializer.Serialize(response, s_writeOptions);

            await httpContext.Response.WriteAsync(jsonResponse);
        }

        /// <summary>
        ///     Writes the log.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <param name="errorDetails">The error details.</param>
        private void WriteLog(Exception ex, List<ErrorDetail>? errorDetails)
        {
            switch (ex)
            {
                case ValidationException validationException:
                    _logger.LogWarning(validationException, "Validation error: {ValidationErrors}.", string.Join(", ", errorDetails!.Select(e => $"{e.Field}: {e.Messages}"))
                    );
                    break;

                case NotFoundException notFoundException:
                    _logger.LogWarning(notFoundException, "Resource not found.");
                    break;

                case UnauthorizedAccessException unauthorizedAccessException:
                    _logger.LogWarning(unauthorizedAccessException, "Unauthorized access attempt.");
                    break;

                case ForbiddenAccessException forbiddenAccessException:
                    _logger.LogWarning(forbiddenAccessException, "Forbidden access attempt.");
                    break;

                case BadRequestException badRequestException:
                    _logger.LogWarning(badRequestException, "Having bad request.");
                    break;

                case BusinessRuleException businessRuleException:
                    _logger.LogWarning(businessRuleException, "Business rule violation.");
                    break;

                default:
                    _logger.LogError(ex, "Unhandled exception: {ExceptionType} - {Message}.", ex.GetType().Name, ex.Message);
                    break;
            }
        }

        /// <summary>
        ///     Gets the errors.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <returns></returns>
        private static List<ErrorDetail>? GetErrors(Exception ex)
        {
            List<ErrorDetail>? errors = null;

            if (ex is ValidationException validationException)
                errors = validationException.ErrorsDictionary.Select(e => new ErrorDetail
                {
                    Field = e.Key,
                    Messages = e.Value
                }).ToList();

            return errors;
        }

        /// <summary>
        ///     Gets the title.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <returns></returns>
        private static string GetTitle(Exception ex)
        {
            return ex switch
            {
                ApplicationException applicationException => applicationException.Title,
                _ => "Server Error"
            };
        }

        /// <summary>
        ///     Gets the status code.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <returns></returns>
        private static int GetStatusCode(Exception ex)
        {
            return ex switch
            {
                ForbiddenAccessException => StatusCodes.Status403Forbidden,
                NotFoundException => StatusCodes.Status404NotFound,
                ValidationException => StatusCodes.Status400BadRequest,
                BadRequestException => StatusCodes.Status400BadRequest,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                BusinessRuleException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError

            };
        }
    }


    /// <summary>
    ///     Error response dto.
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        ///     Gets or sets the status code.
        /// </summary>
        /// <value>
        ///     The status code.
        /// </value>
        public int StatusCode { get; set; }

        /// <summary>
        ///     Gets or sets the message.
        /// </summary>
        /// <value>
        ///     The message.
        /// </value>
        public string? Message { get; set; }

        /// <summary>
        ///     Gets or sets the title.
        /// </summary>
        /// <value>
        ///     The title.
        /// </value>
        public string? Title { get; set; }

        /// <summary>
        ///     Gets or sets the errors.
        /// </summary>
        /// <value>
        ///     The errors.
        /// </value>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public List<ErrorDetail>? Errors { get; set; }
    }

    /// <summary>
    ///     Error detail dto.
    /// </summary>
    public class ErrorDetail
    {
        /// <summary>
        ///     Gets or sets the field.
        /// </summary>
        /// <value>
        ///     The field.
        /// </value>
        public string Field { get; set; } = default!;

        /// <summary>
        ///     Gets or sets the messages.
        /// </summary>
        /// <value>
        ///     The messages.
        /// </value>
        public string[] Messages { get; set; } = default!;
    }
}
