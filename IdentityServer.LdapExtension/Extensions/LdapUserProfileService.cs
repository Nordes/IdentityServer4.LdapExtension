using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer.LdapExtension.UserModel;
using IdentityServer.LdapExtension.UserStore;
using Microsoft.Extensions.Logging;

namespace IdentityServer.LdapExtension.Extensions
{
    public class LdapUserProfileService<TUser>: IProfileService
        where TUser : IAppUser, new()
    {
        /// <summary>
        /// The logger
        /// </summary>
        protected readonly ILogger Logger;

        /// <summary>
        /// The users
        /// </summary>
        protected readonly ILdapUserStore Users;

        //public LdapUserProfileService(ILdapAuthenticationService authenticationService, ILogger<LdapUserProfileService<TUser>> logger)
        public LdapUserProfileService(ILdapUserStore users, ILogger<LdapUserProfileService<TUser>> logger)
        {
            Users = users;
            Logger = logger;
        }

        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            context.LogProfileRequest(Logger);
            //Logger.LogDebug("Get profile called for subject {subject} from client {client} with claim types {claimTypes} via {caller}",
            //    context.Subject.GetSubjectId(),
            //    context.Client.ClientName ?? context.Client.ClientId,
            //    context.RequestedClaimTypes,
            //    context.Caller);

            if (context.RequestedClaimTypes.Any())
            {
                var user = Users.FindBySubjectId(context.Subject.GetSubjectId());
                context.AddRequestedClaims(user.Claims);
            }

            context.LogIssuedClaims(Logger);

            return Task.CompletedTask;
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            Logger.LogDebug("IsActive called from: {caller}", context.Caller);

            var user = Users.FindBySubjectId(context.Subject.GetSubjectId());
            context.IsActive = user?.IsActive == true;

            return Task.CompletedTask;
        }
    }
}
