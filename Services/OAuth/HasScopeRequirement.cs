using Microsoft.AspNetCore.Authorization;
using System;

namespace GeneralAPI.Services.OAuth
{
    /// <summary>
    /// Class HasScopeRequirement.
    /// Implements the <see cref="IAuthorizationRequirement" />
    /// </summary>
    /// <seealso cref="IAuthorizationRequirement" />
    public class HasScopeRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HasScopeRequirement"/> class.
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <param name="issuer">The issuer.</param>
        public HasScopeRequirement(string scope, string issuer)
        {
            Scope = scope ?? throw new ArgumentNullException(nameof(scope));
            Issuer = issuer ?? throw new ArgumentNullException(nameof(issuer));
        }

        /// <summary>
        /// Gets the issuer.
        /// </summary>
        /// <value>The issuer.</value>
        public string Issuer { get; }

        /// <summary>
        /// Gets the scope.
        /// </summary>
        /// <value>The scope.</value>
        public string Scope { get; }
    }
}