# IdentityServer4.LdapExtension
IdentityServer4 Ldap Extension (OpenLdap or ActiveDirectory)

# Installation
From the package manager prompt


```csharp
Install-Package IdentityServer.LdapExtension
```

# How does it work?
You can open the `Sample` project and see how it runs. Of course, it use a direct reference to the original project and is not using the Nuget package.

Basically, you can chose to use Redis or InMemory to store the user who are connecting/connected. It doesn't replace the database for IdentityServer, but it only act as to keep data somewhere instead of flooding the LDAP server and keep also user connecting with different OpenID provider connected. We don't necessarly want to add them in LDAP. You can create your own user implementation and your own user storage implementation.

Also, the schema differ between `Active Directory` and `OpenLdap`, so there is 2 object type with different attributes specification in order to map the user.

# New features on the road
Since I don't have much time, the only feature that will come along for now are going to be tests and maybe different implementation for the samples in order to have all under MIT.

# Licenses
- MIT (For the LdapExtension)
- IdentityServer4 Sample - Apache 2 (due to original code a bit updated)