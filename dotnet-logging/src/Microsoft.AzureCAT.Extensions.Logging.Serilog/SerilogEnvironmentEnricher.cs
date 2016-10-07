using System.Reflection;
using Serilog.Core;
using Serilog.Events;

namespace Microsoft.AzureCAT.Extensions.Logging.Serilog.Serilog
{
    class SerilogEnvironmentEnricher : ILogEventEnricher
    {
        private const string EnvironmentKey = "environment";
        private const string ServiceKey = "service";
        private const string RegionKey = "region";
        private const string VersionKey = "version";

        private readonly string _environmentName;
        private readonly string _serviceName;
        private readonly string _regionName;
        private readonly string _versionName;

        public SerilogEnvironmentEnricher()
        {
            _environmentName = "not-set";
            _serviceName = "not-set";
            _regionName = "not-set";
            _versionName = Assembly.GetEntryAssembly().GetName().Version.ToString();
        }

        public SerilogEnvironmentEnricher(string environmentName,
            string region, string serviceName, string version = null)
        {
            _environmentName = environmentName;
            _serviceName = serviceName;
            _regionName = region;
            _versionName = version;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var envProp = new LogEventProperty(EnvironmentKey, 
                new ScalarValue(_environmentName));
            logEvent.AddPropertyIfAbsent(envProp);

            var svcProp = new LogEventProperty(ServiceKey, new ScalarValue(_serviceName));
            logEvent.AddPropertyIfAbsent(svcProp);

            var regProp = new LogEventProperty(RegionKey, new ScalarValue(_regionName));
            logEvent.AddPropertyIfAbsent(regProp);

            var verProp = new LogEventProperty(VersionKey, new ScalarValue(_versionName));
            logEvent.AddPropertyIfAbsent(verProp);
        }
    }
}