namespace IdentityServer.LdapExtension.DirectoryServices
{
    /// <summary>
    /// This interface only exists in order to wrap the original LdapConnection and allow Mocking.
    /// </summary>
    internal interface ILdapConnection: Novell.Directory.Ldap.ILdapConnection
    {
      
    }
}
