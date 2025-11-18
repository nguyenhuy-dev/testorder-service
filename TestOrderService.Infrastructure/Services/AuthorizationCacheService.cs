using Microsoft.Extensions.Caching.Memory;
using TestOrderService.Application.Interfaces;
namespace TestOrderService.Infrastructure.Services
{
    /// <summary>
    ///     Impelment authorization cache service.
    /// </summary>
    /// <seealso cref="TestOrderService.Application.Interfaces.IAuthorizationCacheService" />
    public class AuthorizationCacheService(IMemoryCache memoryCache) : IAuthorizationCacheService
    {
        /// <summary>
        ///     The memory cache
        /// </summary>
        private readonly IMemoryCache _memoryCache = memoryCache;

        /// <summary>
        ///     Sets the policy.
        /// </summary>
        /// <param name="policyName">Name of the policy.</param>
        /// <param name="policy">The policy.</param>
        /// <param name="timeSpan">The time span.</param>
        public void SetPolicy(string policyName, object policy, TimeSpan? timeSpan = null)
        {
            timeSpan ??= TimeSpan.FromHours(5);
            _memoryCache.Set(policyName, policy, timeSpan.Value);
        }

        /// <summary>
        ///     Tries the get policy.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="policyName">Name of the policy.</param>
        /// <param name="policy">The policy.</param>
        /// <returns></returns>
        public bool TryGetPolicy<T>(string policyName, out T? policy)
        {
            if (_memoryCache.TryGetValue(policyName, out var cachedPolicy) && cachedPolicy is T casted)
            {
                policy = casted;
                return true;
            }

            policy = default;
            return false;
        }
    }
}
