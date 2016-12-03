using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AzureCAT.Extensions.Logging.AppInsights.Models
{
    public class MetricModel
    {
        public string CorrelationId { get; set; }
        public string Name { get; set; }
        public long Value { get; set; }
    }
}
