using System;

namespace Microsoft.AzureCAT.Extensions.Logging.Serilog
{
    public class MetricEvent
    {
        public string MetricName { get; set; }
        public string MetricUnit { get; set; }

        public DateTimeOffset Timestamp { get; set; }

        public decimal Average { get; set; }
        public decimal Min { get; set; }
        public decimal Max { get; set; }
        public int Count { get; set; }
    }
}
