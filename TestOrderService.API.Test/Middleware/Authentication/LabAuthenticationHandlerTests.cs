using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Encodings.Web;
using TestOrderService.API.Middleware.Authentication;
namespace Patient_TestOrder_Service.API.Test.Middleware.Authentication
{
    [TestFixture]
    public class LabAuthenticationHandlerTests
    {

        [SetUp]
        public void Setup()
        {
            _context = new DefaultHttpContext();

            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>()))
                .Returns(Mock.Of<ILogger>());

            _optionsMock = new Mock<IOptionsMonitor<LabAuthenticationSchemeOptions>>();
            _optionsMock.Setup(x => x.Get(It.IsAny<string>()))
                .Returns(new LabAuthenticationSchemeOptions
                {
                    IssuerSigningKey = SigningKey,
                    ValidIssuer = Issuer,
                    ValidAudience = Audience
                });

            _handler = new LabAuthenticationHandler(
                _optionsMock.Object,
                _loggerFactoryMock.Object,
                UrlEncoder.Default);

            _handler.InitializeAsync(
                new AuthenticationScheme("LabScheme", null, typeof(LabAuthenticationHandler)),
                _context);
        }
        private Mock<ILoggerFactory> _loggerFactoryMock;
        private Mock<IOptionsMonitor<LabAuthenticationSchemeOptions>> _optionsMock;
        private LabAuthenticationHandler _handler;
        private DefaultHttpContext _context;

        private const string SigningKey = "THIS_IS_A_TEST_SIGNING_KEY_123456789";
        private const string Issuer = "TestIssuer";
        private const string Audience = "TestAudience";

        // --------------------------------------------------------------
        // 1. Path (IsPassedPath = true)
        // --------------------------------------------------------------
        [Test]
        public async Task HandleAuthenticateAsync_Should_Return_NoResult_When_Path_Is_Bypassed()
        {
            _context.Request.Path = "/home";

            var result = await _handler.AuthenticateAsync();

            if (!result.None)
                Assert.Fail("Expected NoResult when path is bypassed.");

            if (!_context.Items.ContainsKey("AuthResultStatus"))
                Assert.Fail("AuthResultStatus missing.");
        }

        // --------------------------------------------------------------
        // 2. Not [Authorize]
        // --------------------------------------------------------------
        [Test]
        public async Task HandleAuthenticateAsync_Should_Return_NoResult_When_No_Authorize()
        {
            _context.Request.Path = "/api/test";
            _context.SetEndpoint(new Endpoint(_ => Task.CompletedTask, new EndpointMetadataCollection(), "Test"));

            var result = await _handler.AuthenticateAsync();

            if (!result.None)
                Assert.Fail("Expected NoResult when no authorize attribute.");
        }

        // --------------------------------------------------------------
        // 3. Authorization header
        // --------------------------------------------------------------
        [Test]
        public async Task HandleAuthenticateAsync_Should_Fail_When_Authorization_Header_Missing()
        {
            _context.Request.Path = "/api/test";
            _context.SetEndpoint(new Endpoint(_ => Task.CompletedTask,
                new EndpointMetadataCollection(new AuthorizeAttribute()), ""));

            var result = await _handler.AuthenticateAsync();

            if (result.Succeeded)
                Assert.Fail("Expected Fail when Authorization header is missing.");

            if (_context.Items["AuthResultMessage"] as string != "Authorization header is missing.")
                Assert.Fail("Incorrect AuthResultMessage.");
        }

        // --------------------------------------------------------------
        // Generate valid JWT
        // --------------------------------------------------------------
        private static string GenerateValidToken()
        {
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SigningKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                Issuer,
                Audience,
                expires: DateTime.UtcNow.AddMinutes(5),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // --------------------------------------------------------------
        // 4. Token valid
        // --------------------------------------------------------------
        [Test]
        public async Task HandleAuthenticateAsync_Should_Succeed_When_Token_Is_Valid()
        {
            _context.Request.Path = "/api/test";
            _context.SetEndpoint(new Endpoint(_ => Task.CompletedTask,
                new EndpointMetadataCollection(new AuthorizeAttribute()), ""));

            var token = GenerateValidToken();

            _context.Request.Headers.Authorization = "Bearer " + token;

            var result = await _handler.AuthenticateAsync();

            if (!result.Succeeded)
                Assert.Fail("Expected Success for valid token.");

            if (result.Principal == null)
                Assert.Fail("Principal must not be null.");
        }

        // --------------------------------------------------------------
        // 5. Token invalid
        // --------------------------------------------------------------
        [Test]
        public async Task HandleAuthenticateAsync_Should_Fail_When_Token_Invalid()
        {
            _context.Request.Path = "/api/test";
            _context.SetEndpoint(new Endpoint(_ => Task.CompletedTask,
                new EndpointMetadataCollection(new AuthorizeAttribute()), ""));

            _context.Request.Headers.Authorization = "Bearer INVALID_TOKEN";

            var result = await _handler.AuthenticateAsync();

            if (result.Succeeded)
                Assert.Fail("Expected token verification failure.");
        }

        // --------------------------------------------------------------
        // 6. Validate throws exception
        // --------------------------------------------------------------
        [Test]
        public async Task Validate_Should_Fail_When_Exception_Thrown()
        {
            // Provide token with invalid signing key length → will throw
            _optionsMock.Setup(x => x.Get(It.IsAny<string>()))
                .Returns(new LabAuthenticationSchemeOptions
                {
                    IssuerSigningKey = "SHORT_KEY",
                    ValidIssuer = Issuer,
                    ValidAudience = Audience
                });

            _handler = new LabAuthenticationHandler(
                _optionsMock.Object,
                _loggerFactoryMock.Object,
                UrlEncoder.Default);

            await _handler.InitializeAsync(
                new AuthenticationScheme("LabScheme", null, typeof(LabAuthenticationHandler)),
                _context);

            _context.Request.Path = "/api/test";
            _context.SetEndpoint(new Endpoint(_ => Task.CompletedTask,
                new EndpointMetadataCollection(new AuthorizeAttribute()), ""));

            var token = GenerateValidToken();

            _context.Request.Headers.Authorization = "Bearer " + token;

            var result = await _handler.AuthenticateAsync();

            if (result.Succeeded)
                Assert.Fail("Should fail because signing key is invalid.");
        }
    }
}
