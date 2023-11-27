using Basket.API.Entities;
using Basket.API.Repositories.Interfaces;
using Common.Shared.Dtos;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
namespace Basket.API.Repositories
{
    public class BasketRepository : IBasketRepository
    {

        private readonly IDistributedCache _redisCache;

        public BasketRepository(IDistributedCache cache)
        {
            _redisCache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task<ResponseDto<ShoppingCart>> GetBasket(string userName)
        {
            var basket = await _redisCache.GetStringAsync(userName);

            if (string.IsNullOrEmpty(basket))
                return ResponseDto<ShoppingCart>.Fail(404, "User basket not found.");

            var response = JsonConvert.DeserializeObject<ShoppingCart>(basket);

            if (response == null)
                return ResponseDto<ShoppingCart>.Fail(404, "User basket can not deserialize.");

            return ResponseDto<ShoppingCart>.Success(200, response);
        }

        public async Task<ResponseDto<ShoppingCart>> UpdateBasket(ShoppingCart basket)
        {
            await _redisCache.SetStringAsync(basket.UserName, JsonConvert.SerializeObject(basket));

            return await GetBasket(basket.UserName);
        }

        public async Task DeleteBasket(string userName)
        {
            await _redisCache.RemoveAsync(userName);
        }
    }
}
