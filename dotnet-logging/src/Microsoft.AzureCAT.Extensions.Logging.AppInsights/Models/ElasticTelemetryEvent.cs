using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AzureCAT.Extensions.Logging.AppInsights.Models
{
    public class ElasticTelemetryEvent
    {
        public IDictionary<string, double> Metrics { get; set; }
        public IDictionary<string, string> Properties { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string Name { get; set; }
    }
}
