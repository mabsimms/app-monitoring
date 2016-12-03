using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AzureCAT.Extensions.Logging.AppInsights.Models;
using Microsoft.AzureCAT.Samples;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AzureCAT.Extensions.Logging
{
    public class AppInsightElasticSearchPublisher : ITelemetryProcessor
    {
        public static async Task<AppInsightElasticSearchPublisher> 
            CreateAsync(ITelemetryProcessor next, string elasticEndpoint)
        {
            var elasticWrapper = await ElasticSearchWrapper<ElasticTelemetryEvent>.CreateAsync(
                new Uri(elasticEndpoint),
                (tel) => tel.Timestamp,
                "aitel2-template",
                "aitel2-*",
                "aitel2");
            var publisher = await ElasticSearchBulkPublisher<ElasticTelemetryEvent>
                .CreateAsync(elasticWrapper, TimeSpan.FromSeconds(1));

            var sink = new AppInsightElasticSearchPublisher(next, publisher);
            return sink;
        }

        public void Process(ITelemetry item)
        {
            if (item is EventTelemetry)
            {
                var et = item as EventTelemetry;
                var tel = new ElasticTelemetryEvent()
                {
                    Metrics = et.Metrics,
                    Properties = et.Properties,
                    Timestamp = et.Timestamp,
                    Name = et.Name
                };
                _pipeline.Emit(tel);
            }
            _next.Process(item);
        }

        protected readonly ITelemetryProcessor _next;
        protected readonly ElasticSearchBulkPublisher<ElasticTelemetryEvent> _pipeline;

        public AppInsightElasticSearchPublisher(ITelemetryProcessor next, 
            ElasticSearchBulkPublisher<ElasticTelemetryEvent> pipeline)
        {
            _next = next;
            _pipeline = pipeline;
        }       
    }
}
