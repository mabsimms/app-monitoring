using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.Extensions.Configuration;
using Microsoft.ApplicationInsights.DataContracts;

namespace Microsoft.AzureCAT.Extensions.Logging.AppInsights.Enrichers
{
    public class ContextEnricher : ITelemetryInitializer
    {
        private readonly IConfigurationSection _enrichSection;
        private readonly IList<KeyValuePair<string, string>> _values;

        public ContextEnricher(IConfiguration cfg)
        {
            var section = cfg.GetSection("ApplicationInsights")?.GetSection("Enrich");
            if (section == null || section.GetChildren().Count() == 0)
                _values = new List<KeyValuePair<string, string>>();
            else
            {
                var val = new List<KeyValuePair<string, string>>();
                foreach (var kv in section.GetChildren())
                {
                    val.Add(new KeyValuePair<string, string>(kv.Key, kv.Value));
                }
                _values = val;
            }
        }

        public void Initialize(ITelemetry telemetry)
        {
            var evt = telemetry as ISupportProperties;
            if (evt != null)
            {
                foreach (var p in _values)
                    evt.Properties.Add(p.Key, p.Value);
            }
        }
    }
}