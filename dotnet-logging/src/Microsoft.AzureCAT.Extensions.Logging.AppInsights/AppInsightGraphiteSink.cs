using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AzureCAT.Extensions.Logging.Sinks;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureCAT.Extensions.Logging.AppInsights
{
    public class AppInsightGraphiteSink : GraphitePublisherBase<ITelemetry>, 
        ITelemetryProcessor
    {
        protected readonly ITelemetryProcessor _next;

        public AppInsightGraphiteSink(ITelemetryProcessor next,
            ILogger logger,
            string hostName)
            : base(logger, hostName)
        {
            this._next = next;
        }

        public AppInsightGraphiteSink(ITelemetryProcessor next,
            ILogger logger,
            string hostName,
            int port,
            System.TimeSpan maxFlushTime,
            int maxWindowEventCount = 100)
             : base(logger, hostName, port, maxFlushTime, maxWindowEventCount)
        {
            this._next = next;
        }

        public override void Process(ITelemetry item)
        {
            if (item is MetricTelemetry)
            {                
                base.Process(item);
            }
            this._next.Process(item);
        }

        protected override IList<string> GetContent(IEnumerable<ITelemetry> events)
        {
            var contentList = new List<string>();
            foreach (var e in events)
            {
                if (e is MetricTelemetry)
                {
                    var me = e as MetricTelemetry;
                    var metricName = me.Name
                    .ToLower()
                        .Replace(' ', '_')
                        .Replace(':', '.')
                        .Replace('/', '.')
                        .Replace("\"", "")
                        .TrimStart('.')
                        .TrimEnd('.')
                        .TrimEnd('\n')
                    ;
                    contentList.Add($"{metricName}.avg {me.Value} {me.Timestamp.ToUnixTimeSeconds()}");
                    contentList.Add($"{metricName}.min {me.Min} {me.Timestamp.ToUnixTimeSeconds()}");
                    contentList.Add($"{metricName}.max {me.Max} {me.Timestamp.ToUnixTimeSeconds()}");
                    contentList.Add($"{metricName}.count {me.Count} {me.Timestamp.ToUnixTimeSeconds()}");
                    contentList.Add($"{metricName}.stddev {me.StandardDeviation} {me.Timestamp.ToUnixTimeSeconds()}");

                    if (me.Properties.ContainsKey("P50"))
                        contentList.Add($"{metricName}.p50 {me.Properties["P50"]} {me.Timestamp.ToUnixTimeSeconds()}");
                    if (me.Properties.ContainsKey("P90"))
                        contentList.Add($"{metricName}.p90 {me.Properties["P90"]} {me.Timestamp.ToUnixTimeSeconds()}");
                    if (me.Properties.ContainsKey("P99"))
                        contentList.Add($"{metricName}.p99 {me.Properties["P99"]} {me.Timestamp.ToUnixTimeSeconds()}");
                }
            }
            return contentList;

        }
    }
}
