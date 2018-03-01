namespace IdentityServer.LdapExtension.DirectoryServices
{
    /// <summary>
    /// This class only exists in order to wrap the original LdapConnection and allow Mocking.
    /// </summary>
    internal class LdapConnection: Novell.Directory.Ldap.LdapConnection, ILdapConnection
    {
    }
}
