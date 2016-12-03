using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureCAT.Extensions.Logging.AppInsights.Provider
{
    public class AppInsightsLoggerProvider : ILoggerProvider
    {
        public const string OriginalFormatPropertyName = "{OriginalFormat}";

        private ImmutableDictionary<string, LogLevel> _levels;
        private LogLevel _defaultLevel;

        private readonly TelemetryClient _client;
        private readonly IConfiguration _cfg;

		public AppInsightsLoggerProvider(IConfiguration config)
        {
            _client = new TelemetryClient();

	        _client.InstrumentationKey = config.GetSection("ApplicationInsights").GetValue<string>("InstrumentationKey");

            // Load in the level map
            var change = config.GetReloadToken();
            change.RegisterChangeCallback(ConfigUpdated, null);
            _cfg = config;

            LoadConfiguration();
		}

        private void ConfigUpdated(object obj)
        {
            LoadConfiguration();
        }

        private void LoadConfiguration()
        {
            var levels = _cfg.GetSection("ApplicationInsights")?.GetSection("LogLevels");

            var dict = new Dictionary<string, LogLevel>();
            LogLevel defaultLevel = LogLevel.Information;

            if (levels != null)
            {
                foreach (var k in levels.GetChildren())
                {
                    LogLevel level;
                    if (Enum.TryParse(k.Value, true, out level))
                        dict.Add(k.Key, level);
                    else
                        dict.Add(k.Key, LogLevel.Warning);
                }

                if (dict.ContainsKey("Default"))
                {
                    defaultLevel = dict["Default"];
                    dict.Remove("Default");
                }
            }

            _levels = dict.ToImmutableDictionary();
            _defaultLevel = defaultLevel;
        }

        // TODO - fix this
        public bool IsEnabled(string categoryName, LogLevel level)
        {
            if (_levels.ContainsKey(categoryName))
                return level >= _levels[categoryName];
            else
                return level >= _defaultLevel;
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

        public AppInsightsScope CurrentScope
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

        public TelemetryClient Client
        {
            get { return _client; }
        }
    }
}
