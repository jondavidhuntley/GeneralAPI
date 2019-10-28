using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;

namespace GeneralAPI.Services.OAuth
{
    /// <summary>
    /// Class HasScopeHandler.
    /// </summary>
    public class HasScopeHandler : AuthorizationHandler<HasScopeRequirement>
    {
        /// <summary>
        /// Makes a decision if authorization is allowed based on a specific requirement.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The requirement to evaluate.</param>
        /// <returns>Task.</returns>
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            HasScopeRequirement requirement)
        {
            // If user does not have the scope claim, get out of here
            if (!context.User.HasClaim(c => c.Type == "scope" && c.Issuer == requirement.Issuer))
            {
                return Task.CompletedTask;
            }

            // Split the scopes string into an array
            var scopes = context.User.FindFirst(c => c.Type == "scope" && c.Issuer == requirement.Issuer).Value.Split(' ');
            // Succeed if the scope array contains the required scope
            if (scopes.Any(s => s == requirement.Scope))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}