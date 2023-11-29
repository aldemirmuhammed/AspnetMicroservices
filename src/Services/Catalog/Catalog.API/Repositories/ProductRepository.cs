using Catalog.API.Data.Interfaces;
using Catalog.API.Entities;
using Catalog.API.Repositories.Interfaces;
using Common.Shared.Dtos;
using MongoDB.Driver;
using OpenTelemetry.Shared;
using System.Diagnostics;
using System.Net.Mail;
using System.Net;
using static MassTransit.ValidationResultExtensions;
using Catalog.API.RedisServices;
using Elasticsearch.Net;
using Catalog.API.Controllers;
using System.Xml.Linq;

namespace Catalog.API.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ICatalogContext _context;
        private readonly RedisService _redisService;
        private readonly ILogger<ProductRepository> _logger;

        public ProductRepository(ICatalogContext context, RedisService redisService, ILogger<ProductRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _redisService = redisService;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }

        public async Task<ResponseDto<IEnumerable<Product>>> GetProductsAsync()
        {
            //using var activity = ActivitySourceProvider.Source.StartActivity();

            var result = await _context.Products.Find(p => true).ToListAsync();
            if (result == null)
            {
                _logger.LogError("Products not found.");
                return ResponseDto<IEnumerable<Product>>.Fail(HttpStatusCode.NotFound.GetHashCode(), "Product not found");
            }
            return ResponseDto<IEnumerable<Product>>.Success(HttpStatusCode.OK.GetHashCode(), result);
        }

        public async Task<ResponseDto<Product>> GetProductAsync(string id)
        {
            var result = await _context.Products.Find(p => p.Id == id).FirstOrDefaultAsync();
            if (result == null)
            {
                _logger.LogError("Product with productId={@id}, not found.", id);
                return ResponseDto<Product>.Fail(HttpStatusCode.NotFound.GetHashCode(), "Product not found");
            }
            return ResponseDto<Product>.Success(HttpStatusCode.OK.GetHashCode(), result);
        }

        public async Task<ResponseDto<IEnumerable<Product>>> GetProductByNameAsync(string name)
        {
            FilterDefinition<Product> filter = Builders<Product>.Filter.ElemMatch(p => p.Name, name);

            var result = await _context.Products.Find(filter).ToListAsync();
            if (result == null)
            {
                _logger.LogError("Products with name={@name} not found.", name);
                return ResponseDto<IEnumerable<Product>>.Fail(HttpStatusCode.NotFound.GetHashCode(), "Product not found as by name");
            }

            return ResponseDto<IEnumerable<Product>>.Success(HttpStatusCode.OK.GetHashCode(), result);
        }

        public async Task<ResponseDto<IEnumerable<Product>>> GetProductByCategoryAsync(string categoryName)
        {
            FilterDefinition<Product> filter = Builders<Product>.Filter.Eq(p => p.Category, categoryName);

            var result = await _context.Products.Find(filter).ToListAsync();
            if (result == null)
            {
                _logger.LogError("Products with categoryName={@categoryName} not found.", categoryName);
                return ResponseDto<IEnumerable<Product>>.Fail(HttpStatusCode.NotFound.GetHashCode(), "Product not found as by category");
            }
            return ResponseDto<IEnumerable<Product>>.Success(HttpStatusCode.OK.GetHashCode(), result);
        }


        public async Task CreateProductAsync(Product product)
        {

            // redis örneği
            //await _redisService.GetDb(0).StringSetAsync("Id", product.Id);
            //var redisUserId = _redisService.GetDb(0).StringGetAsync("Id");

            await _context.Products.InsertOneAsync(product);

            _logger.LogInformation("Product successfully created. product={@product}", product);

            using var activity = ActivitySourceProvider.Source.StartActivity();
            activity?.AddTag("Product successfully created.", product);
            activity?.AddEvent(new("Product successfully created."));
        }

        public async Task<ResponseDto<bool>> UpdateProductAsync(Product product)
        {
            var updateResult = await _context.Products.ReplaceOneAsync(filter: g => g.Id == product.Id, replacement: product);
            _logger.LogInformation("Updated operations completed with modified count : updateResult.ModifiedCount={@updateResult.ModifiedCount}", updateResult.ModifiedCount);

            return ResponseDto<bool>.Success(HttpStatusCode.OK.GetHashCode(),
                updateResult.IsAcknowledged && updateResult.ModifiedCount > 0);
        }

        public async Task<ResponseDto<bool>> DeleteProductAsync(string id)
        {
            FilterDefinition<Product> filter = Builders<Product>.Filter.Eq(p => p.Id, id);

            DeleteResult deleteResult = await _context.Products.DeleteOneAsync(filter);
            
            _logger.LogInformation("Deleted operations completed with modified count : deleteResult={@deleteResult}", deleteResult);

            return ResponseDto<bool>.Success(HttpStatusCode.OK.GetHashCode(),
              deleteResult.IsAcknowledged && deleteResult.DeletedCount > 0);

        }
    }
}
