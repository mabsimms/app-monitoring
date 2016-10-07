using Serilog.Core;
using Serilog.Events;

namespace Microsoft.AzureCAT.Extensions.Logging.Serilog.Serilog
{
    /// <summary>
    /// Pull in the correlation identifier from the logical call context
    /// </summary>
    public class SerilogContextEnricher : ILogEventEnricher
    {
        public const string CorrelationIdStateName = "CorrelationId";

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            // TODO - rewrite this to use AsyncLocal<T> or the ambient http context

            //var data = CallContext.LogicalGetData(CorrelationIdStateName);
            //if (data == null)
            //    return;
            //var guidStr = data.ToString();
            //Guid id;

            //if (Guid.TryParse(guidStr, out id))
            //{
            //    var prop = new LogEventProperty(CorrelationIdStateName, new ScalarValue(id));
            //    logEvent.AddPropertyIfAbsent(prop);
            //}
        }
    }
}