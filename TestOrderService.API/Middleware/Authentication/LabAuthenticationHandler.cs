using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
namespace TestOrderService.API.Middleware.Authentication
{
    /// <summary>
    ///     Lab Authentication Handler custom.
    /// </summary>
    /// <seealso
    ///     cref="Microsoft.AspNetCore.Authentication.AuthenticationHandler&lt;TestOrderService.API.Middleware.Authentication.LabAuthenticationSchemeOptions&gt;" />
    public class LabAuthenticationHandler(
        IOptionsMonitor<LabAuthenticationSchemeOptions> options,
        ILoggerFactory loggerFactory,
        UrlEncoder encoder) : AuthenticationHandler<LabAuthenticationSchemeOptions>(options, loggerFactory, encoder)
    {

        private const string AUTH_STATUS = "AuthResultStatus";
        /// <summary>
        ///     The logger
        /// </summary>
        private readonly ILogger<LabAuthenticationHandler> _logger = loggerFactory.CreateLogger<LabAuthenticationHandler>();

        /// <summary>
        ///     Allows derived types to handle authentication.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:Microsoft.AspNetCore.Authentication.AuthenticateResult" />.
        /// </returns>
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            _logger.LogInformation("Handling authentication...");

            var path = Context.Request.Path.Value?.ToLower();
            if (IsPassPath(path))
            {
                _logger.LogInformation("No authentication with passing path.");
                Context.Items[AUTH_STATUS] = "NoResult";
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            // Authentication when have [Authorize].
            var endpoints = Context.GetEndpoint();
            var requiresAuth = endpoints?.Metadata.GetMetadata<IAuthorizeData>() != null;
            if (!requiresAuth)
            {
                Context.Items[AUTH_STATUS] = "NoResult";
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var authorizationHeaders = Context.Request.Headers.Authorization;
            if (authorizationHeaders.Count == 0)
            {
                Context.Items[AUTH_STATUS] = "Fail";
                Context.Items["AuthResultMessage"] = "Authorization header is missing.";
                return Task.FromResult(AuthenticateResult.Fail("Authorization header is missing."));
            }

            var tokenValue = authorizationHeaders[0]?.Replace("Bearer ", "");
            if (!string.IsNullOrEmpty(tokenValue) && VerifyToken(tokenValue, out var claimsPrincipal))
            {
                var ticket = new AuthenticationTicket(claimsPrincipal!, Scheme.Name);
                _logger.LogInformation("Authentication succeeded.");

                return Task.FromResult(AuthenticateResult.Success(ticket));
            }

            Context.Items[AUTH_STATUS] = "Fail";
            Context.Items["AuthResultMessage"] = "Verify token unsuccessfully.";
            return Task.FromResult(AuthenticateResult.Fail("Verify token unsuccessfully."));
        }

        /// <summary>
        ///     Verifies the token.
        /// </summary>
        /// <param name="tokenValue">The token value.</param>
        /// <param name="claimsPrincipal">The claims principal.</param>
        /// <returns></returns>
        private bool VerifyToken(string tokenValue, out ClaimsPrincipal? claimsPrincipal)
        {
            var handler = new JwtSecurityTokenHandler();
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Options.IssuerSigningKey)),
                ValidateIssuer = true,
                ValidIssuer = Options.ValidIssuer,
                ValidateAudience = true,
                ValidAudience = Options.ValidAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                claimsPrincipal = handler.ValidateToken(tokenValue, tokenValidationParameters, out var securityToken);
                _logger.LogInformation("Token validation succeeded.");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                claimsPrincipal = null;

                return false;
            }
        }

        /// <summary>
        ///     Determines whether [is pass path] [the specified path].
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        ///     <c>true</c> if [is pass path] [the specified path]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsPassPath(string? path)
        {
            return !path!.StartsWith("/api");
        }
    }
}
