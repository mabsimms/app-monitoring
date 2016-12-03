using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AzureCAT.Extensions.Logging.Sinks;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using Serilog.Events;

namespace Microsoft.AzureCAT.Extensions.Logging.Serilog
{
    public class SerilogGraphiteSink : 
        GraphitePublisherBase<LogEvent>, 
        ILogEventSink
    {
        public SerilogGraphiteSink(string hostName) : 
            base(hostName)
        { }

        public SerilogGraphiteSink(string hostName, int port, 
            TimeSpan maxFlushTime, int maxWindowEventCount = 100) 
                : base(hostName, port, maxFlushTime, maxWindowEventCount)
        { }

        protected override IList<string> GetContent(IEnumerable<LogEvent> events)
        {
            var contentList = new List<string>();

            var metricEvents = events
               .Where(e => e.Properties.ContainsKey("elapsed"))
               .GroupBy(GetName)
               .Select(e => new MetricEvent()
               {
                   MetricName = e.Key,
                   MetricUnit = "ms",
                   Average = e.Average(t => GetValue(t)),
                   Count = e.Count(),
                   Min = e.Min(t => GetValue(t)),
                   Max = e.Max(t => GetValue(t)),
                   Timestamp = e.Min(t => t.Timestamp)

                    // TODO - percentiles
                });

            foreach (var me in metricEvents)
            {                                
                var metricName = me.MetricName
                    .ToLower()
                        .Replace(' ', '_')
                        .Replace(':', '.')
                        .Replace('/', '.')
                        .Replace("\"", "")
                        .TrimEnd('.')
                        .TrimEnd('\n')
                    ;

                contentList.Add($"{metricName}.avg {me.Average} {me.Timestamp.ToUnixTimeSeconds()}");
                contentList.Add($"{metricName}.min {me.Min} {me.Timestamp.ToUnixTimeSeconds()}");
                contentList.Add($"{metricName}.max {me.Max} {me.Timestamp.ToUnixTimeSeconds()}");
                contentList.Add($"{metricName}.count {me.Count} {me.Timestamp.ToUnixTimeSeconds()}");

                //contentList.Add($"{metricName}.stddev {me.StandardDeviation} {me.Timestamp.ToUnixTimeSeconds()}");

                //if (me.Properties.ContainsKey("P50"))
                //    contentList.Add($"{metricName}.p50 {me.Properties["P50"]} {me.Timestamp.ToUnixTimeSeconds()}");
                //if (me.Properties.ContainsKey("P90"))
                //    contentList.Add($"{metricName}.p90 {me.Properties["P90"]} {me.Timestamp.ToUnixTimeSeconds()}");
                //if (me.Properties.ContainsKey("P99"))
                //    contentList.Add($"{metricName}.p99 {me.Properties["P99"]} {me.Timestamp.ToUnixTimeSeconds()}");
              
            }
            return contentList;

        }

        public void Emit(LogEvent logEvent)
        {
            this.Process(logEvent);
        }


        protected string GetName(LogEvent evt)
        {
            if (evt.Properties.ContainsKey("categoryName") &&
                evt.Properties.ContainsKey("operationType"))
            {
                StringBuilder opName = new StringBuilder();
                if (evt.Properties.ContainsKey("service"))
                {
                    opName.Append(evt.Properties["service"].ToString());
                    opName.Append('.');
                }


                if (evt.Properties.ContainsKey("categoryName"))
                {
                    opName.Append(evt.Properties["categoryName"].ToString());
                    opName.Append('.');
                }

                opName.Append(evt.Properties["operationType"].ToString());
                opName.Append('.');

                if (evt.Properties.ContainsKey("environment"))
                {
                    opName.Append(evt.Properties["environment"].ToString());
                    opName.Append('.');
                }

                // todo
                if (evt.Properties.ContainsKey("Instance"))
                {
                    opName.Append(evt.Properties["Instance"].ToString());
                    opName.Append('.');
                }

                return opName.ToString()
                    .ToLower()
                    .Replace(':', '.')
                    .Replace('/', '.')
                    .Replace("\"", "")
                    .TrimEnd('.')
                    ;
            }
            else
                return "unknown";
        }

        protected decimal GetValue(LogEvent evt)
        {
            if (evt.Properties.ContainsKey("elapsed"))
                return (decimal)TimeSpan.Parse(evt.Properties["elapsed"].ToString()).TotalMilliseconds;
            return 0.0M;
        }


    }
}
