using IdentityServer.LdapExtension.UserModel;

namespace IdentityServer.LdapExtension
{
    /// <summary>
    /// Maybe not mandatory, to see.
    /// </summary>
    public interface ILdapService<out TUser>
        where TUser: IAppUser, new()
    {
        TUser Login(string username, string password);
        TUser FindUser(string username);
    }
}
