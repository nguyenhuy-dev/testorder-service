using Microsoft.AspNetCore.Authentication;
namespace TestOrderService.API.Middleware.Authentication
{
    /// <summary>
    ///     Options for lab authentication.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions" />
    public class LabAuthenticationSchemeOptions : AuthenticationSchemeOptions
    {
        /// <summary>
        ///     Gets or sets the issuer signing key.
        /// </summary>
        /// <value>
        ///     The issuer signing key.
        /// </value>
        public string IssuerSigningKey { get; set; } = default!;

        /// <summary>
        ///     Gets or sets the valid issuer.
        /// </summary>
        /// <value>
        ///     The valid issuer.
        /// </value>
        public string ValidIssuer { get; set; } = default!;

        /// <summary>
        ///     Gets or sets the valid audience.
        /// </summary>
        /// <value>
        ///     The valid audience.
        /// </value>
        public string ValidAudience { get; set; } = default!;
    }
}
