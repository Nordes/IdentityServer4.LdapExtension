using System.Linq;
using System.Threading.Tasks;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using IdentityServer.LdapExtension.UserModel;
using IdentityServer.LdapExtension.UserStore;
using Microsoft.Extensions.Logging;

namespace IdentityServer.LdapExtension.Extensions
{
    public class LdapUserProfileService<TUser>: IProfileService
        where TUser : IAppUser, new()
    {
        protected readonly ILogger Logger;
        protected readonly ILdapUserStore Users;

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapUserProfileService{TUser}"/> class.
        /// </summary>
        /// <param name="users">The users.</param>
        /// <param name="logger">The logger.</param>
        public LdapUserProfileService(ILdapUserStore users, ILogger<LdapUserProfileService<TUser>> logger)
        {
            Users = users;
            Logger = logger;
        }

        /// <summary>
        /// This method is called whenever claims about the user are requested (e.g. during token creation or via the userinfo endpoint)
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>Returns nothing, but update the current claims to the context.</returns>
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

        /// <summary>
        /// This method gets called whenever identity server needs to determine if the user is valid or active (e.g. if the user's account has been deactivated since they logged in).
        /// (e.g. during token issuance or validation).
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public Task IsActiveAsync(IsActiveContext context)
        {
            Logger.LogDebug("IsActive called from: {caller}", context.Caller);

            var user = Users.FindBySubjectId(context.Subject.GetSubjectId());
            context.IsActive = user?.IsActive == true;

            return Task.CompletedTask;
        }
    }
}
