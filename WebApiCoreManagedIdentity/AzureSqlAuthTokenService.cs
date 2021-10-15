using Microsoft.Azure.Services.AppAuthentication;

namespace WebApiCoreManagedIdentity
{
    public class AzureSqlAuthTokenService
    {
        public string GetToken(string connectionString)
        {
            AzureServiceTokenProvider provider = new AzureServiceTokenProvider();
            var token = provider.GetAccessTokenAsync("https://database.windows.net/").Result;
            return token;
        }
    }
}
