using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Shared.Dtos
{
    public record StockCheckAndPaymentProcessRequestDto
    {
        public string OrderCode { get; set; } = null!;

        public List<OrderItemDto> OrderItems { get; set; } = null!;

    }
}
