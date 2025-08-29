using Dapper;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using StoreManagement.ProductFunction.Models;
using StoreManagement.ProductFunction.SqlHelper;
using System.Data;
using System.Net;
using System.Text.Json;

namespace StoreManagement.ProductFunction
{
    public class ProductsFunction
    {
        private readonly ILogger<ProductsFunction> _logger;
        private readonly SqlConnectionFactory _connectionFactory;

        public ProductsFunction(ILogger<ProductsFunction> logger, SqlConnectionFactory connectionFactory)
        {
            _logger = logger;
            _connectionFactory = connectionFactory;
        }

        [Function("CreateProduct")]
        public async Task<HttpResponseData> CreateProduct(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "products")] HttpRequestData req)
        {
            try
            {
                if (!req.Headers.TryGetValues("X-Company-Id", out var headerValues)
                    || !int.TryParse(headerValues.FirstOrDefault(), out var companyId))
                {
                    var badReq = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badReq.WriteStringAsync("X-Company-Id header is required and must be a valid integer.");
                    return badReq;
                }

                var body = await JsonSerializer.DeserializeAsync<CreateProductRequest>(req.Body);

                if (body is null)
                {
                    var badReq = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badReq.WriteStringAsync("Invalid request body.");
                    return badReq;
                }

                using var connection = _connectionFactory.CreateConnection();
                
                var storeExists = await connection.ExecuteScalarAsync<int>(
                    "SELECT COUNT(1) FROM Store WHERE StoreId=@StoreId AND CompanyId=@CompanyId",
                    new { StoreId = body.StoreId, CompanyId = companyId }
                );

                if (storeExists == 0)
                {
                    var forbidden = req.CreateResponse(HttpStatusCode.NotFound);
                    await forbidden.WriteStringAsync("Store not found.");
                    return forbidden;
                }

                var parameters = new DynamicParameters();
                parameters.Add("@StoreId", body.StoreId);
                parameters.Add("@Name", body.Name);
                parameters.Add("@Description", body.Description);
                parameters.Add("@Price", body.Price);

                await connection.ExecuteAsync("sp_InsertProduct", parameters, commandType: CommandType.StoredProcedure);

                var response = req.CreateResponse(HttpStatusCode.Created);
                await response.WriteStringAsync("Product created successfully.");
                return response;

            }
            catch (Exception ex)
            {
                var error = req.CreateResponse(HttpStatusCode.InternalServerError);
                await error.WriteStringAsync($"Error: {ex.Message}");
                return error;
            }
        }

        [Function("GetProductsByStore")]
        public async Task<HttpResponseData> GetProductsByStore(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "stores/{storeId:int}/products")] HttpRequestData req,
            int storeId)
        {
            try
            {
                if (!req.Headers.TryGetValues("X-Company-Id", out var headerValues)
                    || !int.TryParse(headerValues.FirstOrDefault(), out var companyId))
                {
                    var badReq = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badReq.WriteStringAsync("X-Company-Id header is required and must be a valid integer.");
                    return badReq;
                }

                using var connection = _connectionFactory.CreateConnection();

                var storeExists = await connection.ExecuteScalarAsync<int>(
                    "SELECT COUNT(1) FROM Store WHERE StoreId=@StoreId AND CompanyId=@CompanyId",
                    new { StoreId = storeId, CompanyId = companyId });

                if (storeExists == 0)
                {
                    var forbidden = req.CreateResponse(HttpStatusCode.NotFound);
                    await forbidden.WriteStringAsync("Store not found.");
                    return forbidden;
                }

                var json = await connection.ExecuteScalarAsync<string>(
                    "SELECT dbo.fn_GetProductsJson(@StoreId)",
                    new { StoreId = storeId });

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json");

                await response.WriteStringAsync(string.IsNullOrEmpty(json) ? "[]" : json);
                return response;
            }
            catch (Exception ex)
            {
                var error = req.CreateResponse(HttpStatusCode.InternalServerError);
                await error.WriteStringAsync($"Error: {ex.Message}");
                return error;
            }
        }

        [Function("GetProductById")]
        public async Task<HttpResponseData> GetProductById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products/{productId:int}")] HttpRequestData req,
            int productId)
        {
            try
            {
                if (!req.Headers.TryGetValues("X-Company-Id", out var headerValues)
                    || !int.TryParse(headerValues.FirstOrDefault(), out var companyId))
                {
                    var badReq = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badReq.WriteStringAsync("X-Company-Id header is required and must be a valid integer.");
                    return badReq;
                }

                using var connection = _connectionFactory.CreateConnection();

                var product = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    @"SELECT p.ProductId, p.StoreId, p.Name, p.Description, p.Price, p.CreatedAt, p.UpdatedAt
                        FROM Product p
                        INNER JOIN Store s ON p.StoreId = s.StoreId
                        WHERE p.ProductId = @ProductId AND s.CompanyId = @CompanyId",
                    new { ProductId = productId, CompanyId = companyId });

                if (product == null)
                {
                    var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFound.WriteStringAsync("Product not found.");
                    return notFound;
                }

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json");

                var json = System.Text.Json.JsonSerializer.Serialize(product);
                await response.WriteStringAsync((string)json);
                return response;
            }
            catch (Exception ex)
            {
                var error = req.CreateResponse(HttpStatusCode.InternalServerError);
                await error.WriteStringAsync($"Error: {ex.Message}");
                return error;
            }
        }

        [Function("UpdateProduct")]
        public async Task<HttpResponseData> UpdateProduct(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "products/{productId:int}")] HttpRequestData req,
            int productId)
        {
            try
            {
                if (!req.Headers.TryGetValues("X-Company-Id", out var headerValues)
                    || !int.TryParse(headerValues.FirstOrDefault(), out var companyId))
                {
                    var badReq = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badReq.WriteStringAsync("X-Company-Id header is required and must be a valid integer.");
                    return badReq;
                }

                var body = await JsonSerializer.DeserializeAsync<CreateProductRequest>(
                    req.Body,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (body == null)
                {
                    var badReq = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badReq.WriteStringAsync("Invalid request body.");
                    return badReq;
                }

                using var connection = _connectionFactory.CreateConnection();

                var exists = await connection.ExecuteScalarAsync<int>(
                    @"SELECT COUNT(1)
                        FROM Product p
                        INNER JOIN Store s ON p.StoreId = s.StoreId
                        WHERE p.ProductId = @ProductId AND s.CompanyId = @CompanyId",
                    new { ProductId = productId, CompanyId = companyId });

                if (exists == 0)
                {
                    var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFound.WriteStringAsync("Product not found.");
                    return notFound;
                }

                await connection.ExecuteAsync(
                    @"UPDATE Product
                        SET Name = @Name,
                            Description = @Description,
                            Price = @Price,
                            UpdatedAt = GETDATE()
                        WHERE ProductId = @ProductId",
                    new { body.Name, body.Description, body.Price, ProductId = productId });

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteStringAsync("Product updated successfully.");
                return response;
            }
            catch (Exception ex)
            {
                var error = req.CreateResponse(HttpStatusCode.InternalServerError);
                await error.WriteStringAsync($"Error: {ex.Message}");
                return error;
            }
        }

        [Function("DeleteProduct")]
        public async Task<HttpResponseData> DeleteProduct(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "products/{productId:int}")] HttpRequestData req,
            int productId)
        {
            try
            {
                if (!req.Headers.TryGetValues("X-Company-Id", out var headerValues)
                    || !int.TryParse(headerValues.FirstOrDefault(), out var companyId))
                {
                    var badReq = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badReq.WriteStringAsync("X-Company-Id header is required and must be a valid integer.");
                    return badReq;
                }

                using var connection = _connectionFactory.CreateConnection();

                var exists = await connection.ExecuteScalarAsync<int>(
                    @"SELECT COUNT(1)
                        FROM Product p
                        INNER JOIN Store s ON p.StoreId = s.StoreId
                        WHERE p.ProductId = @ProductId AND s.CompanyId = @CompanyId",
                    new { ProductId = productId, CompanyId = companyId });

                if (exists == 0)
                {
                    var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFound.WriteStringAsync("Product not found.");
                    return notFound;
                }

                await connection.ExecuteAsync(
                    @"DELETE FROM Product WHERE ProductId = @ProductId",
                    new { ProductId = productId });

                var response = req.CreateResponse(HttpStatusCode.NoContent);
                return response;
            }
            catch (Exception ex)
            {
                var error = req.CreateResponse(HttpStatusCode.InternalServerError);
                await error.WriteStringAsync($"Error: {ex.Message}");
                return error;
            }
        }
    }
}
