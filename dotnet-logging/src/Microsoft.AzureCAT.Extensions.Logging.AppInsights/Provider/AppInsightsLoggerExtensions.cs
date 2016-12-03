using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Diagnostics;
using System.Net;
using Microsoft.AzureCAT.Extensions.Logging.AppInsights.Models; 
using Mojio.TelematicServer.Common.Logging.AppInsights;

namespace Microsoft.AzureCAT.Extensions.Logging.AppInsights.Provider
{
	public static class AppInsightsLoggerExtensions
	{

		private const string CountFormatString = "Key={Key}, Count={Value}";
		private const string ExceptionFormatString = 
				"ExceptionOccurrenceId={ExceptionOccurrenceId}, ExceptionType={ExceptionType}, " +
				"ExceptionMessage={ExceptionMessage}, ExceptionStackTrace={ExceptionStackTrace}, " +
				"ExceptionDepth={ExceptionDepth}";

		private const string FlattenedExceptionFormatString =
			"AggregateException details for {ExceptionOccurrenceId}: exception {ExceptionDepth}, " +
			"ExceptionType={ExceptionType},ExceptionMessage={ExceptionMessage}, " +
			"ExceptionStackTrace={ExceptionStackTrace}";
		private const string HttpFormatString = 
				"Headers={Headers}, ResponseCode={ResponseCode}, Cookies={Cookies}, " +
				"URL={URL}, Method={Method}, DurationInMs={DurationInMs}";
		private const string CookieHeader = "Set-Cookie";

		public static ILoggerFactory AddAppInsights(
			this ILoggerFactory factory,
			IConfiguration cfg,
			IEnumerable<ITelemetryInitializer> telemetryInitializers = null,
			ILogger logger = null,
			bool dispose = false)
		{
			if (factory == null)
				throw new ArgumentNullException(nameof(factory));

			// TODO - pass through the logger
			factory.AddProvider(new AppInsightsLoggerProvider(cfg));

			return factory;
		}

		public static IDisposable BeginTimedOperation(this ILogger logger, string operationId)
		{
			return new TimedOperation(logger, operationId);
		}
          
         public static async Task<HttpResponseMessage> HttpRequestWithLogging(
			this ILogger logger, 
			HttpClient client, 
			HttpMethod method, 
			string requestUri, 
			StringContent body = null)
		{
			var requestMessage = new HttpRequestMessage(method, requestUri);
			if (!method.Equals(HttpMethod.Get))
			{
				if (body == null)
					body = new StringContent(String.Empty);
				requestMessage.Content = body;
			}
			var sw = Stopwatch.StartNew();
			var response = await client.SendAsync(requestMessage);
			sw.Stop();
			var eid = new EventId(0, LogTypes.HttpLog);

			var headers = client.DefaultRequestHeaders;
			var statusCode = (int) response.StatusCode;
			IEnumerable<string> cookies = null;
			response.Headers.TryGetValues(CookieHeader, out cookies);
			var url = String.Join("/", client.BaseAddress, requestUri);
			var elapsedMs = sw.ElapsedMilliseconds;

			logger.LogInformation(eid, HttpFormatString, headers, statusCode, cookies, url, method, elapsedMs);

			return response;
		}
         
	}
}
