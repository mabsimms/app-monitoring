using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureCAT.Extensions.Logging.AppInsights.Provider
{
    /// <summary>
    /// [TODO] - finish implementing this using the serilog as a base
    /// https://github.com/serilog/serilog-extensions-logging/blob/dev/src/Serilog.Extensions.Logging/Extensions/Logging/SerilogLogger.cs
    /// </summary>
    public class AppInsightsLogger : ILogger
    {
        private static readonly TelemetryClient _telemetryClient;
        private readonly AppInsightsLoggerProvider _provider;

        static AppInsightsLogger()
        {
            _telemetryClient = new TelemetryClient();    
        }

        public AppInsightsLogger(AppInsightsLoggerProvider provider,
            string categoryName = null)
        {
            // TODO - how do we do category logging?
            this._provider = provider;
            
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!_telemetryClient.IsEnabled())
                return;

            var eventTelemetry = new EventTelemetry();
            eventTelemetry.Timestamp = DateTimeOffset.UtcNow;

            // TODO eventTelemetry.Sequence = 0;
            
            var structure = state as IEnumerable<KeyValuePair<string, object>>;
            if (structure != null)
            {
                foreach (var property in structure)
                {
                    if (property.Key.StartsWith("@"))
                    {
                        // Nested property - TODO, this isn't right
                        eventTelemetry.Properties.Add(property.Key.Substring(1),
                            property.Value.ToString());
                    }
                    else
                    {
                        // Standard property
                        eventTelemetry.Properties.Add(property.Key, 
                            AsLoggableValue(state, formatter).ToString());
                    }
                }   

                // 
                var stateType = state.GetType();
                var stateTypeInfo = stateType.GetTypeInfo();
                
            }

            // TODO - message template parsing

            
            _telemetryClient.TrackEvent(eventTelemetry);
        }

        static object AsLoggableValue<TState>(TState state, Func<TState, Exception, string> formatter)
        {
            object sobj = state;
            if (formatter != null)
                sobj = formatter(state, null);
            return sobj;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            // TODO
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return _provider.BeginScope(state);
        }
    }
}
