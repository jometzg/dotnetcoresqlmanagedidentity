# .NET Core Azure SQL Managed Identity
A sample application demonstrating how to use managed identity with Azure SQL and app services.

There are a number of blogs and documentation pages which discuss the steps on how to get an ASP.NET Core application to authenticate using a managed identity, but none of these has a fully worked sample with all of the source code. So that is what this sample attempts to do.

It builds on the Azure documentation here
[Secure Azure SQL Database connection from App Service using a managed identity](https://docs.microsoft.com/en-us/azure/app-service/app-service-web-tutorial-connect-msi?tabs=windowsclient%2Cdotnetcore#prerequisites)

And other blogs which provide some of the missing pieces. This one here [Connecting with Azure SQL Database using Azure Active Directory and Managed Identity in .NET Core](https://coderjony.com/blogs/connecting-with-azure-sql-database-using-azure-active-directory-and-managed-identity-in-net-core/) is especially useful in showing how to debug using an identity and how to then set the application to pull the managed identity once deployed to an Azure app service.

## So, what problem is this trying to solve?
It may be useful to step back and re-iterate what managed identity is and why it is useful for connecting to and Azure SQL database.

### Managed Identity
This is an identity associated with an Azure service. This capability is usually switched off, but may be enabled. When enabled, for the basic "system" managed identity, and Azure Active Directory application is created and then associated with the Azure service. For web applications hosted in app services, this provides a system identity which may be then used to allow that app service instance to communicate with other Azure AD enabled services. This is essentially a server-to-server relationship that would previously have to be done by registering an Azure AD application and putting its client id and secret in the application settings.

The two main advantages of a managed identity are:
1. There is no client secret to manage and therefore no need for this to be potentially rotated at a later time
2. You do not need to implement code in your application to acquire the identity to use this (or at least this is much streamlined)

[What are managed identities for Azure resources?](https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/overview)

### App Services Managed identity
This may be enabled in the app service instance, which then returns a client ID, which may be used later. When enabled, it creates an Azure AD application which has the same name of the web app. As the web app has to have a unique FQDN, then this means that the name will also be unique in the Azure AD directory.


### Azure SQL Database Active Directory authentication



## The sample app
This sample is a simple web API that returns the contents of the products table from the Azure SQL database sample "NorthWind" database.
