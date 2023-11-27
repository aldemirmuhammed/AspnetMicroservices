using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Shared.Dtos
{
    public record PaymentCreateRequestDto
    {
        public string OrderCode { get; set; } = null!;
        public decimal TotalPrice{ get; set; }
    }
}
