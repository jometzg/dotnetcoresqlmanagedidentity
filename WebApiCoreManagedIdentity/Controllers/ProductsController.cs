using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApiCoreManagedIdentity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IConfiguration Configuration;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(ILogger<ProductsController> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            Configuration = configuration;
        }

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
    }
}
