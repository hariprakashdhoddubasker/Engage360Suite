using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engage360Suite.Infrastructure.Configuration
{
    /// <summary>
    /// Strongly-typed configuration for <see cref="InMemoryLeadQueue"/>
    /// </summary>
    public sealed class LeadQueueOptions
    {
        /// <summary>Maximum in-memory buffer before producers block.</summary>
        public int Capacity { get; set; } = 10000;
    }
}
