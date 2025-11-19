using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using TestOrderService.Application.Exceptions;
using TestOrderService.Application.Interfaces;
using TestOrderService.Application.Interfaces.gRPC;
namespace TestOrderService.API.Middleware.Authorization
{
    /// <summary>
    ///     Dynamic Authorization Policy Provider custom.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Authorization.DefaultAuthorizationPolicyProvider" />
    public class DynamicAuthorizationPolicyProvider(
        IOptions<AuthorizationOptions> options,
        IServiceScopeFactory scopeFactory) : DefaultAuthorizationPolicyProvider(options)
    {
        /// <summary>
        ///     The authorization options
        /// </summary>
        private readonly AuthorizationOptions _authorizationOptions = options.Value;

        /// <summary>
        ///     The scope factory
        /// </summary>
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

        /// <summary>
        ///     Gets a <see cref="T:Microsoft.AspNetCore.Authorization.AuthorizationPolicy" /> from the given
        ///     <paramref name="policyName" />
        /// </summary>
        /// <param name="policyName">The policy name to retrieve.</param>
        /// <returns>
        ///     The named <see cref="T:Microsoft.AspNetCore.Authorization.AuthorizationPolicy" />.
        /// </returns>
        /// <exception cref="TestOrderService.Application.Exceptions.ForbiddenAccessException"></exception>
        /// <exception cref="System.InvalidOperationException">Unable to build authorization policy.</exception>
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
