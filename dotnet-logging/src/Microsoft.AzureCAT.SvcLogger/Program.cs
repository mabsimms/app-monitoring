using Microsoft.AzureCAT.Extensions.Logging.AppInsights;
using Microsoft.AzureCAT.Extensions.Logging.AppInsights.Provider;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AzureCAT.SvcLogger
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var directProps = new Dictionary<string, string>()
            {
                { "ApplicationInsights:Enrich:instance", Dns.GetHostName() },
                { "ApplicationInsights:Enrich:role", "svc" },
                { "ApplicationInsights:Enrich:environment", "test" },
                { "ApplicationInsights:Enrich:appversion", "1.0" },
            };

            var builder = new ConfigurationBuilder()
               .AddInMemoryCollection(directProps)
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
               ;

            var cfg = builder.Build();

            AppInsightLoggingManager.Initialize(cfg)
                .GetAwaiter().GetResult();
            var loggerFactory = new LoggerFactory()
                .AddAppInsights(cfg);

            var log = loggerFactory.CreateLogger("test");

            int i = 0;
            while (true)
            {
                i++;
                log.LogInformation("{testing} this is a test", i);
                Thread.Sleep(10);
            }
        }
    }
}
