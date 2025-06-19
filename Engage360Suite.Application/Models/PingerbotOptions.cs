using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engage360Suite.Application.Models
{
    public class PingerbotOptions
    {
        public string InstanceId { get; set; } = default!;
        public string AccessToken { get; set; } = default!;
        public string GroupId { get; set; } = default!;
    }
}
