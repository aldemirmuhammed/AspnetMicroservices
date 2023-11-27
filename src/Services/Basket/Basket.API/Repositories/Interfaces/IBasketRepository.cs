using Basket.API.Entities;
using Common.Shared.Dtos;

namespace Basket.API.Repositories.Interfaces
{
    public interface IBasketRepository
    {

        Task<ResponseDto<ShoppingCart>> GetBasket(string userName);
        Task<ResponseDto<ShoppingCart>> UpdateBasket(ShoppingCart basket);
        Task DeleteBasket(string userName);
    }
}
