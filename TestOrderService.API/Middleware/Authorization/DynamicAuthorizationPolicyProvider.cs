using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using TestOrderService.Application.Exceptions;
using TestOrderService.Application.Interfaces;
using TestOrderService.Application.Interfaces.gRPC;
namespace TestOrderService.API.Middleware.Authorization
{
    public class DynamicAuthorizationPolicyProvider(
        IOptions<AuthorizationOptions> options,
        IServiceScopeFactory scopeFactory) : DefaultAuthorizationPolicyProvider(options)
    {
        private readonly AuthorizationOptions _authorizationOptions = options.Value;

        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

        public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            // Whether skip authorization.
            using var scope = _scopeFactory.CreateScope();

            var httpContextAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
            if (httpContextAccessor.HttpContext?.Items["SkipAuthorization"] is true)
                return null;

            // Check registered policy.
            if (_authorizationOptions.GetPolicy(policyName) is { } existingPolicy)
                return existingPolicy;

            // Check cached policy.
            var cache = scope.ServiceProvider.GetRequiredService<IAuthorizationCacheService>();
            if (cache.TryGetPolicy(policyName, out AuthorizationPolicy? cachedPolicy) && cachedPolicy != null)
                return cachedPolicy;

            if (string.IsNullOrEmpty(policyName))
                throw new ForbiddenAccessException();

            var privilegeGrpc = scope.ServiceProvider.GetRequiredService<IPrivilegeGrpcClient>();
            var privilege = (await privilegeGrpc.GetPrivilegesAsync())
                .FirstOrDefault(p => p.PrivilegeName == policyName);
            if (privilege == null)
                throw new ForbiddenAccessException();

            var roleCodes = privilege.Roles.Select(r => r.RoleCode);
            AuthorizationPolicy policy;
            try
            {
                policy = new AuthorizationPolicyBuilder()
                    .RequireAssertion(context =>
                        {
                            var hasRole = roleCodes.Any(rc => context.User.IsInRole(rc));
                            if (!hasRole)
                                throw new ForbiddenAccessException();
                            return hasRole;
                        }
                    ).Build();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Unable to build authorization policy.", ex);
            }

            // Cache the policy for future requests.
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var timeSpanMinutes = configuration.GetSection("Jwt").GetValue("AuthorizationPolicyCacheDurationInMinutes", 5);
            cache.SetPolicy(policyName, policy, TimeSpan.FromMinutes(timeSpanMinutes));

            var logger = scope.ServiceProvider.GetRequiredService<ILogger<DynamicAuthorizationPolicyProvider>>();
            logger.LogInformation("Authorization succeeded.");

            return policy;
        }
    }
}
