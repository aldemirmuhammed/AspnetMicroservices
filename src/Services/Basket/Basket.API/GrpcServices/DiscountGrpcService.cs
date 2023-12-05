
using Discount.Grpc.Protos;

namespace Basket.API.GrpcServices
{
    public class DiscountGrpcService
    {
        private readonly DiscountProtoService.DiscountProtoServiceClient _discountProtoService;
        private readonly ILogger<DiscountGrpcService> _logger;
        public DiscountGrpcService(DiscountProtoService.DiscountProtoServiceClient discountProtoService, ILogger<DiscountGrpcService> logger)
        {
            _discountProtoService = discountProtoService ?? throw new ArgumentNullException(nameof(discountProtoService));
            _logger = logger;
        }

        public async Task<CouponModel> GetDiscount(string productName)
        {
            var discountRequest = new GetDiscountRequest { ProductName = productName };
            _logger.LogInformation("Getting discount coupon model.");
            return await _discountProtoService.GetDiscountAsync(discountRequest);
        }
    }
}
