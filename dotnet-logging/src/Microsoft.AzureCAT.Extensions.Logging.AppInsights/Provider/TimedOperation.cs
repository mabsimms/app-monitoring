using System;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Globalization;
using Microsoft.AzureCAT.Extensions.Logging.AppInsights.Models; 
using Mojio.TelematicServer.Common.Logging.AppInsights;

namespace Microsoft.AzureCAT.Extensions.Logging.AppInsights.Provider
{
	internal class TimedOperation : IDisposable
	{
		private readonly string _operationId;
		private readonly ILogger _logger;
		private readonly DateTime _startTime;
		private const string FormatString = "Operation {OperationId} finished executing in {DurationInMs} ms, StartTime: {StartTime}, EndTime {EndTime}";

		public TimedOperation(ILogger logger, string operationId)
		{
			_logger = logger;
			_operationId = operationId;
			_startTime = DateTime.UtcNow;
		}

		#region IDisposable Support
		private bool _disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposedValue)
			{
				var endTime = DateTime.UtcNow;
				EventId eid = new EventId(0, LogTypes.TimedOperation);
				var startTimeStr = _startTime.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
				var endTimeStr = endTime.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
				TimeSpan span = endTime - _startTime;
				int durationInMs = (int)span.TotalMilliseconds;
				_logger.LogInformation(eid, FormatString, _operationId, durationInMs, startTimeStr, endTimeStr);
				_disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}
		#endregion
	}
}
