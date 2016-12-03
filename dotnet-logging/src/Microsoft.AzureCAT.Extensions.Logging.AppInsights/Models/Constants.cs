using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AzureCAT.Extensions.Logging.AppInsights
{

    public static class TelemetryProps
    {
        public const string TrueString = "true";
        public const string CategoryName = "CategoryName";
        public const string Level = "Level";
        public const string DataSourceSchema = "DataSourceSchema";
        public const string FormattedMessage = "FormattedMessage";
        public const string UnknownFormat = "???";
        public const string Id = "Id";
        public const string Name = "Name";
        public const string CorrelationId = "CorrelationId";
        public const string SecondaryCorrelationId = "SecondaryCorrelationId";
        public const string Environment = "Environment";
        public const string MachineRole = "MachineRole";
        public const string MachineName = "MachineName";
        public const string MessageTemplate = "MessageTemplate";
        public const string CustomPropertyPrefix = "";
        public const string Blob = "Blob";
        public const string Happiness = "Happiness";
        public const string HappinessExplanation = "HappinessExplanation";
        public const string RobotName = "RobotName";
        public const string DurationInMs = "DurationInMs";
        public const string MetricAggregation = "MetricAggregation";
    }

    public enum DataSources
    {
        Interactions,
        TimedOperation,
        Log,
        EventTraces,
        Count,
        HTTP,
        PerfCounters,
        Exception
    }

    public static class LogTypes
    {
        public const string HttpLog = "HttpOpenSchema";
        public const string Exceptions = "ExceptionsOpenSchema";
        public const string Count = "CountOpenSchema";
        public const string Interactions = "InteractionsOpenSchema";
        public const string TimedOperation = "TimedOperationOpenSchema";
        public const string Log = "LogOpenSchema";
    }

    public static class MetricProps
    {
        public const string Avg = "Avg";
        public const string Min = "Min";
        public const string Max = "Max";
        public const string Count = "Count";
        public const string StdDev = "StdDev";
        public const string P50 = "P50";
        public const string P90 = "P90";
        public const string P95 = "P95";
        public const string P99 = "P99";
    }
}
