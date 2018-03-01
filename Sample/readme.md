# Sample.IdentityServer.Ldap
Based on the Quickstart #3 from the official repository of IdentityServer 4.

## Details
This project shows how to use the Ldap extension.

# Alteration from the original QuickStart
- Add the LdapExtension library (Project reference, not a Nuget reference)
- Add an `AppSettings.json` to add the configuration for LDAP. (Link to a Docker LDAP) 
- Use LDAP cache within the memory (Possibility to change to redis if you have also that Docker image)
- Remove the default users found within the `IdentityServer/Config.cs`
- Update the `IdentityServer/Quickstart/AccountController` [todo]
  - Change the `TestUserStore _users` by an official user store [todo... complete]
- Update the views? (Maybe not)

# License
Since the Quickstart is under the Apache 2.0, I'll put the license that fit with it for this folder and those under. However, the IdentityServer part was modified by myself [Nordes Menard-Lamarre].