using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TestOrderService.API.Middleware.Authentication;
using UnauthorizedAccessException=TestOrderService.Application.Exceptions.UnauthorizedAccessException;
namespace TestOrderService.API.Test.Middleware.Authentication
{
    public class AuthenticationGateMiddlewareTests
    {
        private ILogger<AuthenticationGateMiddleware> _logger;
        private RequestDelegate _next;

        [SetUp]
        public void Setup()
        {
            _next = Substitute.For<RequestDelegate>();
            _logger = Substitute.For<ILogger<AuthenticationGateMiddleware>>();
        }

        private AuthenticationGateMiddleware CreateMiddleware()
        {
            return new AuthenticationGateMiddleware(_next, _logger);
        }

        // ------------------------------------------
        // 1. Case: NoResult
        // ------------------------------------------
        [Test]
        public async Task InvokeAsync_Should_Set_SkipAuthorization_When_NoResult()
        {
            // Arrange
            var middleware = CreateMiddleware();
            var context = new DefaultHttpContext();
            context.Items["AuthResultStatus"] = "NoResult";

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Items.ContainsKey("SkipAuthorization").Should().BeTrue();
            context.Items["SkipAuthorization"].Should().Be(true);

            await _next.Received(1).Invoke(context);

            _logger.Received().Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                null,
                Arg.Any<Func<object, Exception?, string>>()
            );
        }

        // ------------------------------------------
        // 2. Case: Fail → throw exception
        // ------------------------------------------
        [Test]
        public void InvokeAsync_Should_Throw_When_Fail()
        {
            // Arrange
            var middleware = CreateMiddleware();
            var context = new DefaultHttpContext();
            context.Items["AuthResultStatus"] = "Fail";
            context.Items["AuthResultMessage"] = "Invalid token";

            // Act
            var act = () => middleware.InvokeAsync(context);

            // Assert
            act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Invalid token");

            // _next MUST NOT be invoked
            _ = _next.DidNotReceive();
        }

        // ------------------------------------------
        // 3. Case: Empty or missing → skip switch cases → call _next
        // ------------------------------------------
        [Test]
        public async Task InvokeAsync_Should_Continue_When_Empty_Status()
        {
            // Arrange
            var middleware = CreateMiddleware();
            var context = new DefaultHttpContext();
            // NO AuthResultStatus in Items → default path

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            await _next.Received(1).Invoke(context);

            // No SkipAuthorization added
            context.Items.ContainsKey("SkipAuthorization").Should().BeFalse();
        }

        // ------------------------------------------
        // 4. Case: Unknown value → treat like default
        // ------------------------------------------
        [Test]
        public async Task InvokeAsync_Should_Continue_When_Unknown_Status()
        {
            // Arrange
            var middleware = CreateMiddleware();
            var context = new DefaultHttpContext();
            context.Items["AuthResultStatus"] = "SomethingElse";

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            await _next.Received(1).Invoke(context);
            context.Items.ContainsKey("SkipAuthorization").Should().BeFalse();
        }
    }
}
