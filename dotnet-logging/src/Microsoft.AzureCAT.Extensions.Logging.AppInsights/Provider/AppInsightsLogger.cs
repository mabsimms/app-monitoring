using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Data.Edm.Library.Values;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureCAT.Extensions.Logging.AppInsights.Provider
{
    /// <summary>
    /// [TODO] - finish implementing this using the serilog as a base
    /// https://github.com/serilog/serilog-extensions-logging/blob/dev/src/Serilog.Extensions.Logging/Extensions/Logging/SerilogLogger.cs
    /// </summary>
    public class AppInsightsLogger : ILogger
    {
        internal const string MessageKey = "Message";

        private readonly TelemetryClient _telemetryClient;
        private readonly AppInsightsLoggerProvider _provider;
        private readonly string _categoryName;

        
        public AppInsightsLogger(AppInsightsLoggerProvider provider,
            string categoryName = null)
        {
            // TODO - how do we do category logging?
            this._provider = provider;
            this._telemetryClient = provider.Client;
            this._categoryName = categoryName ?? "Default";
        }

        // TODO - implement category based logging from configuration
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, 
            Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!_provider.IsEnabled(_categoryName, logLevel))
                return;

            var eventTelemetry = new EventTelemetry();
            eventTelemetry.Timestamp = DateTimeOffset.UtcNow;

            // TODO eventTelemetry.Sequence = 0;
            string messageTemplate = null;

            var structure = state as IEnumerable<KeyValuePair<string, object>>;
            if (structure != null)
            {
                foreach (var property in structure)
                {
                    // Plain "printf" style log message (no embedded structure)
                    if (property.Key == AppInsightsLoggerProvider.OriginalFormatPropertyName 
                        && property.Value is string)
                    {
                        //eventTelemetry.Properties.Add(MessageKey, (string)property.Value);
                        messageTemplate = (string) property.Value;
                    }
                    else if (property.Key.StartsWith("@"))
                    {
                        // Nested property - TODO, this isn't right
                        eventTelemetry.Properties.Add(property.Key.Substring(1),
                            property.Value.ToString());
                    }
                    else
                    {
                        // Standard property
                        eventTelemetry.Properties.Add(property.Key,
                            property.Value?.ToString());
                    }
                }   

                // 
                var stateType = state.GetType();
                var stateTypeInfo = stateType.GetTypeInfo();
                
                // TODO - message template stuff
                if (messageTemplate == null && !stateTypeInfo.IsGenericType)
                {
                    messageTemplate = "{" + stateType.Name + ":l}";
                    // Bind the state property
                }
            }

            // TODO - message template parsing
            if (messageTemplate == null && state != null)
            {
                // Bind in the property
            }

            // Map in the event id
            if (eventId.Id != 0)
                eventTelemetry.Properties.Add("Id", eventId.Id.ToString());
            if (eventId.Name != null)
                eventTelemetry.Properties.Add("Name", eventId.Name);
            
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
