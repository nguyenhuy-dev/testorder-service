using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Text;
using System.Text.Json;
using TestOrderService.API.Middleware;
using TestOrderService.Application.Exceptions;
using ApplicationException=TestOrderService.Application.Exceptions.ApplicationException;
using UnauthorizedAccessException=TestOrderService.Application.Exceptions.UnauthorizedAccessException;

namespace TestOrderService.API.Test.Middleware
{
    [TestFixture]
    public class GlobalExceptionHandlerMiddlewareTests
    {

        [SetUp]
        public void SetUp()
        {
            _logger = Substitute.For<ILogger<GlobalExceptionHandlerMiddleware>>();
        }
        private ILogger<GlobalExceptionHandlerMiddleware> _logger = null!;
        private static readonly string[] expectation = { "e1", "e2" };

        private static async Task<(string body, int status)> InvokeAsync(
            RequestDelegate next,
            ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            var middleware = new GlobalExceptionHandlerMiddleware(next, logger);
            var context = new DefaultHttpContext();
            var ms = new MemoryStream();
            context.Response.Body = ms;

            await middleware.InvokeAsync(context);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(context.Response.Body, Encoding.UTF8);
            return (await reader.ReadToEndAsync(), context.Response.StatusCode);
        }

        [Test]
        public async Task InvokeAsync_NoException_ShouldReturn200_EmptyBody()
        {
            RequestDelegate next = _ => Task.CompletedTask;

            var (body, status) = await InvokeAsync(next, _logger);

            status.Should().Be(200);
            body.Should().BeEmpty();
        }

        [Test]
        public async Task GenericException_ShouldReturn500_AndLogError()
        {
            RequestDelegate next = _ => throw new Exception("X");

            var (body, status) = await InvokeAsync(next, _logger);

            status.Should().Be(500);
            var json = JsonDocument.Parse(body);
            json.RootElement.GetProperty("message").GetString().Should().Be("X");
            json.RootElement.GetProperty("title").GetString().Should().Be("Server Error");

            _logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains("Unhandled exception")),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>());
        }

        [TestCase(typeof(NotFoundException), 404, "Resource not found.")]
        [TestCase(typeof(BadRequestException), 400, "Having bad request.")]
        [TestCase(typeof(UnauthorizedAccessException), 401, "Unauthorized access attempt.")]
        [TestCase(typeof(ForbiddenAccessException), 403, "Forbidden access attempt.")]
        public async Task SpecificException_ShouldReturnCorrectStatusAndLogWarning(Type exType, int expectedStatus, string logMessage)
        {
            var ex = (Exception)Activator.CreateInstance(exType, "msg")!;
            RequestDelegate next = _ => throw ex;

            var (_, status) = await InvokeAsync(next, _logger);

            status.Should().Be(expectedStatus);

            _logger.Received(1).Log(
                LogLevel.Warning,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                ex,
                Arg.Any<Func<object, Exception?, string>>());
        }

        [Test]
        public async Task BusinessRuleException_ShouldReturn400_AndLogWarning()
        {
            var ex = new BusinessRuleException("ruleName", "broken");
            RequestDelegate next = _ => throw ex;

            var (_, status) = await InvokeAsync(next, _logger);

            status.Should().Be(400);

            _logger.Received(1).Log(
                LogLevel.Warning,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                ex,
                Arg.Any<Func<object, Exception?, string>>());
        }

        [Test]
        public async Task ValidationException_ShouldReturn400_WithErrors_AndLogWarning()
        {
            var dict = new Dictionary<string, string[]>
            {
                { "A", expectation },
                { "B", new[] { "eX" } }
            };
            var vex = new ValidationException(dict);
            RequestDelegate next = _ => throw vex;

            var (body, status) = await InvokeAsync(next, _logger);

            status.Should().Be(400);

            var json = JsonDocument.Parse(body);
            var errors = json.RootElement.GetProperty("errors").EnumerateArray().ToList();
            errors.Count.Should().Be(2);

            var fieldA = errors.First(x => x.GetProperty("field").GetString() == "A");
            fieldA.GetProperty("messages").EnumerateArray()
                .Select(e => e.GetString())
                .Should().BeEquivalentTo(expectation);

            _logger.Received(1).Log(
                LogLevel.Warning,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                vex,
                Arg.Any<Func<object, Exception?, string>>());
        }

        [Test]
        public void ApplicationException_Should_SetTitleAndMessage()
        {
            var ex = new TestApplicationException("T", "M");
            ex.Title.Should().Be("T");
            ex.Message.Should().Be("M");
        }
    }

// Helper sealed class
    internal sealed class TestApplicationException : ApplicationException
    {
        public TestApplicationException(string title, string message) : base(title, message) { }
    }
}
