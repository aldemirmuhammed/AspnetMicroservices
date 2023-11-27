using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Shared.Dtos
{
    public record PaymentCreateResponsetDto
    {
        public string Description { get; set; } = null!;
      
    }
}
