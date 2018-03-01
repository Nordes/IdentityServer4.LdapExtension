using System.Collections.Generic;
using System.Security.Claims;
using IdentityServer.LdapExtension.UserModel;

namespace IdentityServer.LdapExtension.UserStore
{
    public interface ILdapUserStore
    {
        IAppUser ValidateCredentials(string username, string password);
        IAppUser FindBySubjectId(string subjectId);
        IAppUser FindByUsername(string username);
        IAppUser FindByExternalProvider(string provider, string userId);
        IAppUser AutoProvisionUser(string provider, string userId, List<Claim> claims);
    }
}
