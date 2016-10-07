using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AzureCAT.Extensions.Logging.AppInsights.Provider
{
    public class AppInsightsScope : IDisposable
    {
        private readonly AppInsightsLoggerProvider _provider;
        private readonly object _state;
        private readonly IDisposable _aiContext;

        public AppInsightsScope(AppInsightsLoggerProvider provider, object state)
        {
            _provider = provider;
            _state = state;
            

        }

        public AppInsightsScope Parent { get; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
