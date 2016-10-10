using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureCAT.Extensions.Logging.AppInsights.Provider
{
    public static class AppInsightsLoggerExtensions
    {
        public static ILoggerFactory AddAppInsights(
            this ILoggerFactory factory,
            IConfiguration cfg,
            ILogger logger = null,
            bool dispose = false)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            // TODO - pass through the logger
            factory.AddProvider(new AppInsightsLoggerProvider(cfg));
            return factory;
        }
    }
}
