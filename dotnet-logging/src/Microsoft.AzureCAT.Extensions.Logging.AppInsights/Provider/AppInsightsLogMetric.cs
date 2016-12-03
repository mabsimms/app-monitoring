using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AzureCAT.Extensions.Logging;
using Microsoft.AzureCAT.Extensions.Logging.AppInsights.Models;
using Microsoft.AzureCAT.Extensions.Logging.AppInsights.Provider;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.AzureCAT.Extensions.Logging.AppInsights.Provider
{
	/// <summary>
	/// Extension methods to assist with logging TelemetryMetrics
	/// </summary>
	public static class LogMetricExtentions
	{
		static readonly EventId EventId = new EventId(0, null);

		public static void LogMetric(this ILogger logger, string metricName, long metricValue)
		{
			if (logger == null)
			{
				throw new ArgumentNullException(nameof(logger));
			}

			string correlationId = "";

			MetricModel logModel = new MetricModel
			{
				CorrelationId = correlationId,
				Name = metricName,
				Value = metricValue
			};						

			logger.Log(LogLevel.Information, EventId, logModel, null, (schema, exception) => null);
		}
	}
}
