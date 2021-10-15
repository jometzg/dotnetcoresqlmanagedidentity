# .NET Core Azure SQL Managed Identity
A sample application demonstrating how to use managed identity with Azure SQL and app services.

There are a number of blogs and documentation pages which discuss the steps on how to get an ASP.NET Core application to authenticate using a managed identity, but none of these has a fully worked sample with all of the source code. So that is what this sample attempts to do.

It builds on the Azure documentation here
[Secure Azure SQL Database connection from App Service using a managed identity](https://docs.microsoft.com/en-us/azure/app-service/app-service-web-tutorial-connect-msi?tabs=windowsclient%2Cdotnetcore#prerequisites)

And other blogs which provide some of the missing pieces. This one here [Connecting with Azure SQL Database using Azure Active Directory and Managed Identity in .NET Core](https://coderjony.com/blogs/connecting-with-azure-sql-database-using-azure-active-directory-and-managed-identity-in-net-core/) is especially useful in showing how to debug using an identity and how to then set the application to pull the managed identity once deployed to an Azure app service.

## The sample app
This sample is a simple web API that returns the contents of the products table from the Azure SQL database sample "NorthWind" database.
