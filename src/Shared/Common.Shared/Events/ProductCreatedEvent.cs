using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Shared.Events
{
    public class ProductCreatedEvent
    {
        public string ProductCode { get; set; } = null!;
    }
}
