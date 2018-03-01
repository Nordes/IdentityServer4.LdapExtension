using System;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Validation;
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

        //public LdapUserResourceOwnerPasswordValidator(ILdapAuthenticationService authenticationService, ISystemClock clock)
        public LdapUserResourceOwnerPasswordValidator(ILdapUserStore users, ISystemClock clock)
        {
            //_users = new LdapUserStore(authenticationService);
            _users = users;
            _clock = clock;
        }

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
