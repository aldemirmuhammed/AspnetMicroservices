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
        private readonly ILogger<BasketRepository> _logger;
        public BasketRepository(IDistributedCache cache, ILogger<BasketRepository> logger)
        {
            _redisCache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger;
        }

        public async Task<ResponseDto<ShoppingCart>> GetBasket(string userName)
        {
            var basket = await _redisCache.GetStringAsync(userName);

            if (string.IsNullOrEmpty(basket))
            {
                _logger.LogError("Basket is null.");
                return ResponseDto<ShoppingCart>.Fail(404, "User basket not found.");
            }
            var response = JsonConvert.DeserializeObject<ShoppingCart>(basket);

            if (response == null)
            {
                _logger.LogError("Basket response could not deserialize.");
                return ResponseDto<ShoppingCart>.Fail(404, "User basket can not deserialize.");
            }

            _logger.LogInformation("Getting basket.");
            return ResponseDto<ShoppingCart>.Success(200, response);
        }

        public async Task<ResponseDto<ShoppingCart>> UpdateBasket(ShoppingCart basket)
        {
            await _redisCache.SetStringAsync(basket.UserName, JsonConvert.SerializeObject(basket));

            foreach (var item in basket.Items)
                _logger.LogInformation("Updated item. Updated basket item={@item}", item);

            return await GetBasket(basket.UserName);
        }

        public async Task DeleteBasket(string userName)
        {
            await _redisCache.RemoveAsync(userName);
            _logger.LogInformation("Deleted item.");
        }
    }
}
