using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer.LdapExtension.UserModel;

namespace IdentityServer.LdapExtension
{
    /// <summary>
    /// This interface is used to save the user data from external provider. By default we use an in memory provider.
    /// </summary>
    /// <remarks>We should be injecting in the LdapUserStore this provider and by default use the inMemory one.</remarks>
    interface IExternalDataProvider<TUser>
        where TUser: IAppUser
    {
        TUser FindBySubjectId(string subjectId);
        TUser FindByUserName(string username);
        TUser FindByExternalProvider(string provider, string userId);
        bool AddUser(string provider, TUser user);
    }
}
