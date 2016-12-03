using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mojio.TelematicServer.Common.Logging.AppInsights.Provider
{
    public class ElasticTelemetryEvent
    {
        public IDictionary<string, double> Metrics { get; set; }
        public IDictionary<string, string> Properties { get; set;  }
        public DateTimeOffset Timestamp { get; set; }
        public string Name { get; set; }
         
    }
}
