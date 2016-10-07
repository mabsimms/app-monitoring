using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MathNet.Numerics.Statistics;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AzureCAT.Extensions.Logging.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureCAT.Extensions.Logging.AppInsights
{
    public class AppInsightAggregator : ITelemetryProcessor
    {
        private readonly ITelemetryProcessor _next;
        private readonly SlidingWindowBase<ITelemetry, ITelemetry> _aggregator;
         
        public AppInsightAggregator(ITelemetryProcessor next, IConfiguration config)
        {
            this._next = next;

            this._aggregator = new SlidingWindowBase<ITelemetry, ITelemetry>(
                config: config, 
                logger: null, 
                filterFunc: Filter,
                nameFunc: GetName,
                transformFunc: Transform,
                publishFunc: (e) => {
                    foreach (var x in e) _next.Process(x);
                    return Task.FromResult(0);
                });
        }

        protected bool Filter(ITelemetry evt)
        {
            // TODO - incorporate the rest of the non-metric telemetry types,
            // e.g. asp.net RequestTelemetry
            if (evt is MetricTelemetry) return false;
            if (evt is RequestTelemetry) return false;

            return true;
        }

        protected string GetName(ITelemetry evt)
        {            
            if (evt is MetricTelemetry)
                return (evt as MetricTelemetry).Name;
            if (evt is RequestTelemetry)
                return (evt as RequestTelemetry).Name;
            return String.Empty;
        }

        protected IEnumerable<ITelemetry> Transform(IEnumerable<ITelemetry> evts)
        {
            var metrics = evts
                  .OfType<MetricTelemetry>()
                  // TODO - ensure that this works with the standard hooks from ai
                  .GroupBy(e => new { e.Name })
                  .Select(e => new MetricTelemetryCollection()
                  {
                      Event = new MetricTelemetry()
                      {
                          Name = e.Key.Name,
                          Value = e.Average(t => t.Value),
                          Timestamp = e.First().Timestamp,
                          Min = e.Min(t => t.Value),
                          Max = e.Max(t => t.Value),
                          Count = e.Count(),
                          StandardDeviation = e.Select(t => t.Value).StandardDeviation()
                      },
                      // Use the merge method to pull the percentiles into the
                      // proprties dictionary                     
                      Properties = new Dictionary<string, string>() {
                            { "P50", e.Select(t=> t.Value).Percentile(50).ToString() },
                            { "P90", e.Select(t=> t.Value).Percentile(90).ToString() },
                            { "P99", e.Select(t=> t.Value).Percentile(99).ToString() }
                      }
                  })
                  .Select(e => e.Merge())
              ;

            var requests = evts
                .OfType<RequestTelemetry>()
                .GroupBy(e => new {Name = e.Url.AbsolutePath.ToString()})
                .Select(e => new MetricTelemetryCollection()
                {
                    Event = new MetricTelemetry()
                    {
                        Name = e.Key.Name,
                        Value = e.Average(t => t.Duration.TotalMilliseconds),
                        Timestamp = e.First().Timestamp,
                        Min = e.Min(t => t.Duration.TotalMilliseconds),
                        Max = e.Max(t => t.Duration.TotalMilliseconds),
                        Count = e.Count(),
                        StandardDeviation = e.Select(t => t.Duration.TotalMilliseconds).StandardDeviation()
                    },
                    // Use the merge method to pull the percentiles into the
                    // proprties dictionary                     
                    Properties = new Dictionary<string, string>()
                    {
                        {"P50", e.Select(t => t.Duration.TotalMilliseconds).Percentile(50).ToString()},
                        {"P90", e.Select(t => t.Duration.TotalMilliseconds).Percentile(90).ToString()},
                        {"P99", e.Select(t => t.Duration.TotalMilliseconds).Percentile(99).ToString()}
                    }
                })
                .Select(e => e.Merge());

            return metrics
                .Concat(requests);                
        }


        public void Process(ITelemetry item)
        {
            this._aggregator.Process(item);
            this._next.Process(item);
        }
    }


    public class MetricTelemetryCollection
    {
        public MetricTelemetry Event { get; set; }
        public IDictionary<string, string> Properties { get; set; }

        public MetricTelemetry Merge()
        {
            foreach (var nr in Properties)
                Event.Properties.Add(nr.Key, nr.Value);
            return Event;
        }
    }

}
