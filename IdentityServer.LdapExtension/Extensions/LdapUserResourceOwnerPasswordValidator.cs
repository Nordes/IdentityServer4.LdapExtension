using System;
using System.Threading.Tasks;
using Duende.IdentityServer.Validation;
using IdentityModel;
using IdentityServer.LdapExtension.UserModel;
using IdentityServer.LdapExtension.UserStore;
using Microsoft.AspNetCore.Authentication;

namespace IdentityServer.LdapExtension.Extensions
{
    public class LdapUserResourceOwnerPasswordValidator<TUser>: IResourceOwnerPasswordValidator
        where TUser: IAppUser, new()
    {
        private readonly ILdapUserStore _users;
        private readonly ISystemClock _clock;

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapUserResourceOwnerPasswordValidator{TUser}"/> class.
        /// </summary>
        /// <param name="users">The users.</param>
        /// <param name="clock">The clock.</param>
        public LdapUserResourceOwnerPasswordValidator(ILdapUserStore users, ISystemClock clock)
        {
            _users = users;
            _clock = clock;
        }

        /// <summary>
        /// Validates the resource owner password credential
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>Returns nothing, but update the context.</returns>
        /// <exception cref="ArgumentException">Subject ID not set - SubjectId</exception>
        public Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            var user = _users.ValidateCredentials(context.UserName, context.Password);
            if (user != null)
            {
                context.Result = new GrantValidationResult(
                    user.SubjectId ?? throw new ArgumentException("Subject ID not set",
                    nameof(user.SubjectId)),
                    OidcConstants.AuthenticationMethods.Password,
                    _clock.UtcNow.UtcDateTime,
                    user.Claims);
            }

            return Task.CompletedTask;
        }
    }
}
