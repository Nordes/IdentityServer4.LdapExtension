# IdentityServer4.LdapExtension
IdentityServer4 Ldap Extension (OpenLdap or ActiveDirectory)

## Installation
From the package manager prompt

```csharp
Install-Package IdentityServer.LdapExtension
```

### Configuration on your IdentityServer4 server
Use the `AddLdapUsers` in order to integrate your Ldap users. Don't forget that you need to fill your configuration in your `appsettings.json`.

```csharp
// ... Code ...

services.AddIdentityServer()
    .AddDeveloperSigningCredential()
    //.AddSigningCredential(...)
    .AddInMemoryIdentityResources(Config.GetIdentityResources())
    .AddInMemoryApiResources(Config.GetApiResources())
    .AddInMemoryClients(Config.GetClients())
    .AddLdapUsers<OpenLdapAppUser>(Configuration.GetSection("ldap"), UserStore.InMemory);

// ... Code ...
```

The `appsettings.json` could have something similar to:

```json
{
  "ldap": {
    "url": "localhost",
    "port": 389,
    "ssl": false, // Not implemented
    "bindDn": "cn=ldap-ro,dc=contoso,dc=com",
    "bindCredentials": "P@ss1W0Rd!",
    //// Active directory
    //"searchBase": "cn=users,dc=contoso,dc=com",
    //"searchFilter": "(&(objectClass=user)(objectClass=person)(sAMAccountName={0}))",
    
    //// OpenLdap where the uid is the userId
    "searchBase": "ou=users,DC=contoso,dc=com",
    "searchFilter": "(&(objectClass=posixAccount)(objectClass=person)(uid={0}))",
    "redis": "localhost:32771,ssl=false", // Need a redis server ConnectionString if Redis store 
  }
}
```

Other changes are required in the default IdentityServer4 templates. You can see an implementation in the `sample` folder.

## You don't have a LDAP for your tests?
Not a problem, I wrote an article in order to setup the entire OpenLdap server with Docker. That way you can play with existing users or create your own users. The page is https://nordes.github.io/#/Articles/howto-openldap-with-contoso-users.

## How does it work?
You can open the `Sample` project and see how it runs. Of course, it use a direct reference to the original project and is not using the Nuget package.

Basically, you can chose to use Redis or InMemory to store the user who are connecting/connected. It doesn't replace the database for IdentityServer, but it only act as to keep data somewhere instead of flooding the LDAP server and keep also user connecting with different OpenID provider connected. We don't necessarly want to add them in LDAP. You can create your own user implementation and your own user storage implementation.

Also, the schema differ between `Active Directory` and `OpenLdap`, so there is 2 object type with different attributes specification in order to map the user.

## New features on the road
Since I don't have much time, the only feature that will come along for now are going to be tests and maybe different implementation for the samples in order to have all under MIT.

## Licenses
- MIT (For the Nuget LdapExtension)
- IdentityServer4 Sample - Apache 2 (due to original code a bit updated)