using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engage360Suite.Application.Models
{
    public class LeadDto
    {
        public required string Name { get; set; } = string.Empty;
        public required string PhoneNumber { get; set; } = string.Empty;
    }
}
