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
Azure SQL database can work in SQL authentication mode or Active Directory (AD) Authentication mode. You can also allow both at the same time. When enabling Azure AD authentication, you need to set an AD account as adminstrator of server.

SQL Server Management Studio (SSMS) can authenticate against Azure SQL in Active Directory mode and this is a good way of testing that this works. This can also be a simple route to adding the managed identity user to the database and giving them permission to access resources in the database.

When a web application needs to authenticate against an Azure SQL database, an actual user (user principal) should not be used as even if you know that user's password, the authentication process for users expected these to be entered on Azure AD hosted web pages and there may also be a requirement for multi-factor authentication that cannot be met with a user flow. So a managed identity must use a separate authentication flow - more akin to a device flow.


## The sample app
This sample is a simple web API that returns the contents of the products table from the Azure SQL database sample "NorthWind" database. This is exposed as a GET request on the path /products.

In code terms, the GET method, creates a connection to the SQL database, opens the connection and executes a simple *select Name, ProductNumber from SalesLT.Product* from the Product table in the sample database.

```
// GET: api/<ProductsController>
        [HttpGet]
        public IEnumerable<Product> Get()
        {
            List<Product> products = new List<Product>();
            try
            {
                using (SqlConnection connection = new SqlConnection(Configuration.GetConnectionString("northwind")))
                {
                    connection.AccessToken = new AzureSqlAuthTokenService().GetToken(Configuration["connectionStringForToken"]);
                    connection.Open();
         
                    String sql = "select Name, ProductNumber from SalesLT.Product";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                products.Add(new Product()
                                {
                                    Name = reader.GetString(0),
                                    ProductNumber = reader.GetString(1)
                                });
                            }
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                _logger.LogError(e.ToString());
            }
            return products;
        }
```
This is almost identical to a conventional application, except for how the application code authenticates to the database. The following line illustrates the change:
```
connection.AccessToken = new AzureSqlAuthTokenService().GetToken(Configuration["connectionStringForToken"]);
```
This is calls down into a simple class which gets a SQL access token using the *current identity*
```
 public class AzureSqlAuthTokenService
    {
        public string GetToken(string connectionString)
        {
            AzureServiceTokenProvider provider = new AzureServiceTokenProvider();
            var token = provider.GetAccessTokenAsync("https://database.windows.net/").Result;
            return token;
        }
    }
```

As can be seen, the application code pulls in a couple of values from configuration. These are important.

```
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*",
  "connectionStringForToken": "RunAs=Developer; DeveloperTool=VisualStudio",
  "ConnectionStrings": {
    "northwind": "Server=tcp:<your-servername>.database.windows.net,1433;Initial Catalog=<your-database>;Persist Security Info=False;"
  }
}

```
Both of these need a little explaining:
1. the database connection string defines the server and database names, but there is no setting for user name, password or how to authenticate.
2. The setting "connectionStringForToken" will have different values in the development environment compared to when deployed in the app service. As can be seen above, these settings are the correct ones for local debugging.

## Local Debugging
One of the challenges with using managed identities and SQL authentication is how for this to both work in the target environment - the target app service, and how to debug locally. In the previous section, it show the value of a setting to be *RunAs=Developer; DeveloperTool=VisualStudio* - this indicates to the code that in development mode, use Visual Studio to authenticate.

How does this work?

You can configure Visual Studio (in this case 2019) to authenticate against Azure AD when debugging. Below is the setting, which can be found in Visual Studio at *Tools -> Options -> Azure Service Authentication*.

![Azure Service Authentication](images/visual-stuidio-settings.png "Visual Studio Settings")

