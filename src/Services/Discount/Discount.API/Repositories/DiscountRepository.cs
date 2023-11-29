using Common.Shared.Dtos;
using Dapper;
using Discount.API.Entities;
using Discount.API.Repositories.Interfaces;
using Npgsql;
using OpenTelemetry.Shared;

namespace Discount.API.Repositories
{

    public class DiscountRepository : IDiscountRepository
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DiscountRepository> _logger;

        public DiscountRepository(IConfiguration configuration, ILogger<DiscountRepository> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;
        }

        public async Task<ResponseDto<Coupon>> GetDiscount(string productName)
        {
            using var connection = new NpgsqlConnection(_configuration.GetValue<string>("DatabaseSettings:ConnectionString"));

            var coupon = await connection.QueryFirstOrDefaultAsync<Coupon>
                ("SELECT * FROM Coupon WHERE ProductName = @ProductName", new { ProductName = productName });

            if (coupon == null)
                return ResponseDto<Coupon>.Fail(200, "No Discount");

            #region Logger

            _logger.LogInformation($"Getting discount. Discount count : {coupon}");

            #endregion

            return ResponseDto<Coupon>.Success(200, coupon);
        }

        public async Task<ResponseDto<bool>> CreateDiscount(Coupon coupon)
        {
            using var connection = new NpgsqlConnection(_configuration.GetValue<string>("DatabaseSettings:ConnectionString"));

            var affected =
                await connection.ExecuteAsync
                    ("INSERT INTO Coupon (ProductName, Description, Amount) VALUES (@ProductName, @Description, @Amount)",
                            new { ProductName = coupon.ProductName, Description = coupon.Description, Amount = coupon.Amount });

            if (affected == 0)
            {
                #region Logger

                _logger.LogError($"Discount could not created.");

                #endregion
                return ResponseDto<bool>.Fail(404, "Coupon could not created");
            }

            #region Logger

            _logger.LogInformation($"Created discount. Discount count : {coupon}");

            #endregion

            #region Observability

            using var activity = ActivitySourceProvider.Source.StartActivity();
            activity?.AddEvent(new($"Discount created successfully => {(affected == 0 ? false : true)}"));
            activity?.AddTag("Discount created successfully => ", affected == 0 ? false : true);

            #endregion

            return ResponseDto<bool>.Success(200, true);
        }

        public async Task<ResponseDto<bool>> UpdateDiscount(Coupon coupon)
        {
            using var connection = new NpgsqlConnection(_configuration.GetValue<string>("DatabaseSettings:ConnectionString"));

            var affected = await connection.ExecuteAsync
                    ("UPDATE Coupon SET ProductName=@ProductName, Description = @Description, Amount = @Amount WHERE Id = @Id",
                            new { ProductName = coupon.ProductName, Description = coupon.Description, Amount = coupon.Amount, Id = coupon.Id });

            if (affected == 0)
            {
                #region Logger

                _logger.LogError($"Discount could not updated.");

                #endregion
                return ResponseDto<bool>.Fail(404, "Coupon could not updated");
            }
            #region Logger

            _logger.LogInformation($"Updated discount. Discount count : {coupon}");

            #endregion

            #region Observability

            using var activity = ActivitySourceProvider.Source.StartActivity();
            activity?.AddEvent(new($"Discount updated successfully => {(affected == 0 ? false : true)}"));
            activity?.AddTag("Discount updated successfully => ", affected == 0 ? false : true);

            #endregion

            return ResponseDto<bool>.Success(200, true);
        }

        public async Task<ResponseDto<bool>> DeleteDiscount(string productName)
        {
            using var connection = new NpgsqlConnection(_configuration.GetValue<string>("DatabaseSettings:ConnectionString"));

            var affected = await connection.ExecuteAsync("DELETE FROM Coupon WHERE ProductName = @ProductName",
                new { ProductName = productName });

            if (affected == 0)
            {
                #region Logger

                _logger.LogError($"Discount could not deleted.");

                #endregion

                return ResponseDto<bool>.Fail(404, "Coupon could not deleted");
            }
            #region Logger

            _logger.LogInformation($"Deleted discount. Discount count : {affected}");

            #endregion

            #region Observability

            using var activity = ActivitySourceProvider.Source.StartActivity();
            activity?.AddEvent(new($"Discount deleted successfully => {(affected == 0 ? false : true)}"));
            activity?.AddTag("Discount deleted successfully => ", affected == 0 ? false : true);

            #endregion

            return ResponseDto<bool>.Success(200, true);
        }
    }
}
