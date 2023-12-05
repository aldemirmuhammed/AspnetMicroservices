using Common.Shared.Dtos;
using Discount.Grpc.Entities;

namespace Discount.Grpc.Repositories.Interfaces
{
    public interface IDiscountRepository
    {
        Task<ResponseDto<Coupon>> GetDiscount(string productName);

        Task<ResponseDto<bool>> CreateDiscount(Coupon coupon);
        Task<ResponseDto<bool>> UpdateDiscount(Coupon coupon);
        Task<ResponseDto<bool>> DeleteDiscount(string productName);
    }
}
