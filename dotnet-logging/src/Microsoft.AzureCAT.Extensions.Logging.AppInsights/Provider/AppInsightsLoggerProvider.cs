using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureCAT.Extensions.Logging.AppInsights.Provider
{
    public class AppInsightsLoggerProvider : ILoggerProvider
    {
        private readonly TelemetryClient _client;

        public AppInsightsLoggerProvider()
        {
            _client = new TelemetryClient();
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new AppInsightsLogger(this, categoryName);
        }

        public void Dispose()
        {
          
        }

        public IDisposable BeginScope<T>(T state)
        {
            return new AppInsightsScope(this, state);
        }

        readonly AsyncLocal<AppInsightsScope> _value = 
            new AsyncLocal<AppInsightsScope>();

        internal AppInsightsScope CurrentScope
        {
            get
            {
                return _value.Value;
            }
            set
            {
                _value.Value = value;
            }
        }
    }
}
