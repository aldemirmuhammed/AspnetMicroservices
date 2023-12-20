using Microsoft.Extensions.Logging;
using OpenTelemetry.Shared;
using Ordering.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ordering.Infrastructure.Persistence
{
    public class OrderContextSeed
    {
        public static async Task SeedAsync(OrderContext orderContext, ILogger<OrderContextSeed> logger)
        {
            if (!orderContext.Orders.Any())
            {
                var item = GetPreconfiguredOrders();
                orderContext.Orders.AddRange(item);
                await orderContext.SaveChangesAsync();
                logger.LogInformation("Seed database associated with context {DbContextName}", typeof(OrderContext).Name);
               
                using var activity = ActivitySourceProvider.Source.StartActivity();
                activity?.AddTag("Order seed successfully inserted.", item);
                activity?.AddEvent(new("Order seed successfully inserted."));
            }
        }

        private static IEnumerable<Order> GetPreconfiguredOrders()
        {
            return new List<Order>
            {
                new Order() {
                    UserName = "swn",
                    FirstName = "Muhammed",
                    LastName = "ALDEMİR",
                    EmailAddress = "aldemirrmuhammed2009@gmail.com",
                    AddressLine = "Tepebasi",
                    Country = "Turkey",
                    TotalPrice = 350 ,

                    CardName="",
                     CardNumber="",
                     CreatedBy="",
                     CreatedDate = DateTime.Now,
                     CVV="",
                     Expiration="",
                     ZipCode="",
                     State = ""
                }
            };
        }
    }
}
