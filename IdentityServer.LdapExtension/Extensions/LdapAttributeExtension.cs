using Novell.Directory.Ldap;
using System.Collections.Generic;

namespace IdentityServer.LdapExtension.Extensions
{
    internal static class LdapAttributeExtension
    {
        /// <summary>
        /// The previous behavior of Novell was to return null if the key was not in the collection. However, this have been changed
        /// and it now throws a key not found exception instead. 
        /// 
        /// This method repeat the previous behavior.
        /// </summary>
        /// <param name="ldapEntry">LdapEntry that we extend</param>
        /// <param name="attribute">The key attribute we are looking for.</param>
        /// <returns>Returns the LdapAttribute or NULL when not found.</returns>
        public static LdapAttribute GetNullableAttribute(this LdapEntry ldapEntry, string attribute)
        {
            try
            {
                var ldapAttr = ldapEntry.GetAttribute(attribute);

                return ldapAttr;
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }
    }
}
