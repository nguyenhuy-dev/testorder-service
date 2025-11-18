using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using System.Security.Claims;
using TestOrderService.API.Middleware.Authorization;
using TestOrderService.Application.DTOs.gRPCs.GetPrivileges;
using TestOrderService.Application.Exceptions;
using TestOrderService.Application.Interfaces;
using TestOrderService.Application.Interfaces.gRPC;
namespace Patient_TestOrder_Service.API.Test.Middleware.Authorization
{
    [TestFixture]
    public class DynamicAuthorizationPolicyProviderTests
    {

        [SetUp]
        public void Setup()
        {
            var inMemory = new Dictionary<string, string?>
            {
                { "Jwt:AuthorizationPolicyCacheDurationInMinutes", "5" }
            };
            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemory)
                .Build();

            _scopeFactory = Substitute.For<IServiceScopeFactory>();
            _scope = Substitute.For<IServiceScope>();
            _provider = Substitute.For<IServiceProvider>();
            _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            _cache = Substitute.For<IAuthorizationCacheService>();
            _grpc = Substitute.For<IPrivilegeGrpcClient>();
            _logger = Substitute.For<ILogger<DynamicAuthorizationPolicyProvider>>();

            _scopeFactory.CreateScope().Returns(_scope);
            _scope.ServiceProvider.Returns(_provider);

            _provider.GetService(typeof(IHttpContextAccessor)).Returns(_httpContextAccessor);
            _provider.GetService(typeof(IAuthorizationCacheService)).Returns(_cache);
            _provider.GetService(typeof(IPrivilegeGrpcClient)).Returns(_grpc);
            _provider.GetService(typeof(IConfiguration)).Returns(_config);
            _provider.GetService(typeof(ILogger<DynamicAuthorizationPolicyProvider>)).Returns(_logger);

            _authOptions = new AuthorizationOptions();
            _sut = new DynamicAuthorizationPolicyProvider(
                Options.Create(_authOptions),
                _scopeFactory
            );
        }

        [TearDown]
        public void TearDown()
        {
            _scope.Dispose();
        }
        private IServiceScopeFactory _scopeFactory;
        private IServiceScope _scope;
        private IServiceProvider _provider;
        private IHttpContextAccessor _httpContextAccessor;
        private IAuthorizationCacheService _cache;
        private IPrivilegeGrpcClient _grpc;
        private IConfiguration _config;
        private ILogger<DynamicAuthorizationPolicyProvider> _logger;

        private AuthorizationOptions _authOptions;
        private DynamicAuthorizationPolicyProvider _sut;

        // =====================================================================
        // CASE 1: SkipAuthorization → return null
        // =====================================================================
        [Test]
        public async Task ReturnNull_When_SkipAuthorization_Is_True()
        {
            var http = new DefaultHttpContext();
            http.Items["SkipAuthorization"] = true;
            _httpContextAccessor.HttpContext.Returns(http);

            var result = await _sut.GetPolicyAsync("A");

            result.Should().BeNull();
        }

        // =====================================================================
        // CASE 2: Policy exists in AuthorizationOptions
        // =====================================================================
        [Test]
        public async Task ReturnExistingPolicy_When_Registered_In_AuthorizationOptions()
        {
            var http = new DefaultHttpContext();
            _httpContextAccessor.HttpContext.Returns(http);

            var existing = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
            _authOptions.AddPolicy("P1", existing);

            var result = await _sut.GetPolicyAsync("P1");

            result.Should().BeSameAs(existing);
        }

        // =====================================================================
        // CASE 3: Cache hit
        // =====================================================================
        [Test]
        public async Task ReturnCachedPolicy_When_CacheHit()
        {
            var http = new DefaultHttpContext();
            _httpContextAccessor.HttpContext.Returns(http);

            var cached = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();

            _cache.TryGetPolicy("P2", out Arg.Any<AuthorizationPolicy>()!)
                .Returns(ci =>
                {
                    ci[1] = cached;
                    return true;
                });

            var result = await _sut.GetPolicyAsync("P2");

            result.Should().BeSameAs(cached);
        }

        // =====================================================================
        // CASE 4: Empty policy name → ForbiddenAccessException
        // =====================================================================
        [Test]
        public void ThrowForbidden_When_PolicyName_Empty()
        {
            var http = new DefaultHttpContext();
            _httpContextAccessor.HttpContext.Returns(http);

            Func<Task> act = async () => await _sut.GetPolicyAsync("");

            act.Should().ThrowAsync<ForbiddenAccessException>();
        }

        // =====================================================================
        // CASE 5: Privilege not found → throw ForbiddenAccessException
        // =====================================================================
        [Test]
        public void ThrowForbidden_When_Privilege_NotFound()
        {
            var http = new DefaultHttpContext();
            _httpContextAccessor.HttpContext.Returns(http);

            _grpc.GetPrivilegesAsync().Returns(new List<PrivilegeDto>());

            Func<Task> act = async () => await _sut.GetPolicyAsync("Missing");

            act.Should().ThrowAsync<ForbiddenAccessException>();
        }

        // =====================================================================
        // CASE 6: User does not have role → failed assertion → Forbidden
        // =====================================================================
        [Test]
        public async Task ThrowForbidden_When_User_Does_Not_Have_Role()
        {
            var http = new DefaultHttpContext();
            _httpContextAccessor.HttpContext.Returns(http);

            _grpc.GetPrivilegesAsync().Returns(new List<PrivilegeDto>
            {
                new PrivilegeDto
                {
                    PrivilegeName = "Test",
                    Roles = new List<RoleDto> { new RoleDto { RoleCode = "ADMIN" } }
                }
            });

            var policy = await _sut.GetPolicyAsync("Test");

            var user = new ClaimsPrincipal(new ClaimsIdentity()); // no roles

            var assertion = () =>
            {
                var context = new AuthorizationHandlerContext(policy!.Requirements, user, null);
                var req = policy.Requirements.OfType<AssertionRequirement>().First();

                // gọi delegate trong RequireAssertion
                req.Handler(context);
            };

            assertion.Should().Throw<ForbiddenAccessException>();
        }

        // =====================================================================
        // CASE 7: Build policy fails → throw InvalidOperationException
        // =====================================================================
        [Test]
        public void ThrowInvalidOperation_When_Policy_Build_Fails()
        {
            var http = new DefaultHttpContext();
            _httpContextAccessor.HttpContext.Returns(http);

            _grpc.GetPrivilegesAsync().Returns(new List<PrivilegeDto>
            {
                new PrivilegeDto
                {
                    PrivilegeName = "Bad",
                    Roles = null! // gây NullReference trong builder
                }
            });

            Func<Task> act = async () => await _sut.GetPolicyAsync("Bad");

            act.Should().ThrowAsync<InvalidOperationException>();
        }

        // =====================================================================
        // CASE 8: Valid → return policy + set cache
        // =====================================================================
        [Test]
        public async Task CreatePolicy_And_Cache()
        {
            var http = new DefaultHttpContext();
            _httpContextAccessor.HttpContext.Returns(http);

            _grpc.GetPrivilegesAsync().Returns(new List<PrivilegeDto>
                {
                    new PrivilegeDto
                    {
                        PrivilegeName = "Valid",
                        Roles = new List<RoleDto>
                        {
                            new RoleDto { RoleCode = "ADMIN" }
                        }
                    }
                }
            );

            var result = await _sut.GetPolicyAsync("Valid");

            result.Should().NotBeNull();

            _cache.Received().SetPolicy(
                "Valid",
                Arg.Any<AuthorizationPolicy>(),
                TimeSpan.FromMinutes(5));
        }
    }
}
