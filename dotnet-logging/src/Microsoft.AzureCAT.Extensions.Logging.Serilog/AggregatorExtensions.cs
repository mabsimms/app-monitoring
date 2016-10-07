using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Configuration;

namespace Microsoft.AzureCAT.Extensions.Logging.Serilog
{
    public static class LoggerSinkConfigurationExtensions
    {
        public static LoggerConfiguration GraphiteStats(
             this LoggerSinkConfiguration loggerConfiguration,
             IConfiguration config)
        {
            var server = config.GetValue<string>("serilog:graphite:server", "127.0.0.1");
            var sink = new SerilogGraphiteSink(null, server);
            return loggerConfiguration.Sink(sink);
        }
    }
}
