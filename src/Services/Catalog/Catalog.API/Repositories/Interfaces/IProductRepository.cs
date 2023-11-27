using Catalog.API.Entities;
using Common.Shared.Dtos;

namespace Catalog.API.Repositories.Interfaces
{
    public interface IProductRepository
    {


        Task<ResponseDto<IEnumerable<Product>>> GetProductsAsync();
        Task<ResponseDto<Product>> GetProductAsync(string id);
        Task<ResponseDto<IEnumerable<Product>>> GetProductByNameAsync(string name);
        Task<ResponseDto<IEnumerable<Product>>> GetProductByCategoryAsync(string categoryName);

        Task CreateProductAsync(Product product);
        Task<ResponseDto<bool>> UpdateProductAsync(Product product);
        Task<ResponseDto<bool>> DeleteProductAsync(string id);
    }
}
