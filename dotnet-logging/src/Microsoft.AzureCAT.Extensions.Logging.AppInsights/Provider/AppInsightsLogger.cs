using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts; 
using Microsoft.Extensions.Logging;
using Microsoft.AzureCAT.Extensions.Logging.AppInsights.Models;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Mojio.TelematicServer.Common.Logging.AppInsights;

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
		private const string ModelsNamespace = "Microsoft.AzureCAT.Extensions.Logging.AppInsights.Models.";
		private const string MessageTemplatePrefix = "{";
		private const string MessageTemplatePostfix = ":1}";
		private const string DefaultCategoryName = "Default";
		private readonly Dictionary<Type, HashSet<String>> SchemaProperties = new Dictionary<Type, HashSet<string>>();
		private const string LogTypesClass = "Constants+LogTypes";

		private const string ExceptionOccurrenceIdKey = "ExceptionOccurrenceId";
		private const string ExceptionTypeKey = "ExceptionType";
		private const string ExceptionMessageKey = "ExceptionMessage";
		private const string ExceptionStackTraceKey = "ExceptionStackTrace";
		private const string ExceptionDepthKey = "ExceptionDepth";

		private const string ExceptionFormatString =
				"ExceptionOccurrenceId={ExceptionOccurrenceId}, ExceptionType={ExceptionType}, " +
				"ExceptionMessage={ExceptionMessage}, ExceptionStackTrace={ExceptionStackTrace}, " +
				"ExceptionDepth={ExceptionDepth}";

		public AppInsightsLogger(AppInsightsLoggerProvider provider,
			string categoryName = null)
		{
			// TODO - how do we do category logging?
			this._provider = provider;
			this._telemetryClient = provider.Client;
			this._categoryName = categoryName ?? DefaultCategoryName;
			PopulateSchemaProperties();

		}

		private void PopulateSchemaProperties()
		{
			// Get the Log Schemas from the Constants class
			var logTypesFullName = ModelsNamespace + LogTypesClass;
			var logSchemas = Type.GetType(logTypesFullName);

			if (logSchemas == null)
				return;

			var schemaClassNames = new List<String>();

			// Pull the schema class names from the constants class
			foreach (var field in logSchemas.GetFields())
			{
				var className = (string) field.GetValue(null);
				schemaClassNames.Add(className);
			}

			foreach (var className in schemaClassNames)
			{
				var fullName = ModelsNamespace + className;
				var schemaType = Type.GetType(fullName);
				if (schemaType == null)
					continue;
				var propNames = new HashSet<string>();
				foreach (var property in schemaType.GetProperties())
				{
					propNames.Add(property.Name);
				}
				SchemaProperties.Add(schemaType, propNames);
			}
		}

		// TODO - implement category based logging from configuration
		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
			Exception exception, Func<TState, Exception, string> formatter)
		{
			if (!_provider.IsEnabled(_categoryName, logLevel))
				return;

			// Create EventTelemetry for Log state to put through AI pipeline
			ISupportProperties telemetryProperties = null;
			ITelemetry telemetryModel = null;

			if (state is MetricModel)
			{
				telemetryModel = new MetricTelemetry();
				telemetryProperties = (ISupportProperties)telemetryModel;
			}
			else
			{
				telemetryModel = new EventTelemetry(_categoryName);
				telemetryProperties = (ISupportProperties)telemetryModel;
			}

			telemetryModel.Timestamp = DateTimeOffset.UtcNow;
			telemetryProperties.Properties.Add(TelemetryProps.CategoryName, _categoryName);
			telemetryProperties.Properties.Add(TelemetryProps.Level, logLevel.ToString());

			//telemetryProperties.Properties.Add(TelemetryProps.DataSourceSchema, eventId.Name.GetSourceType().ToString());

			// TODO eventTelemetry.Sequence = 0;
			string messageTemplate = null;
			var className = LogTypes.Log;

			if (eventId.Name != null)
			{
				className = eventId.Name;
			}

			var fullName = ModelsNamespace + className;
			var schemaType = Type.GetType(fullName);

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

						if (exception != null)
						{
							messageTemplate = $"{messageTemplate} {ExceptionFormatString}";
						}
					}
					else if (property.Key.StartsWith("@"))
					{
						// Nested property - TODO, this isn't right
						telemetryProperties.Properties.Add(property.Key.Substring(1),
							property.Value.ToString());
					}
					// If the schemas dictionary contains this schema type, check if the property is a first level property of the schema
					else if (schemaType != null && SchemaProperties.ContainsKey(schemaType) &&  SchemaProperties[schemaType].Contains(property.Key) )
					{
						telemetryProperties.Properties.Add(property.Key, property.Value?.ToString());
					}
					else
					{
						// Standard property
						telemetryProperties.Properties.Add(TelemetryProps.CustomPropertyPrefix + property.Key,
							property.Value?.ToString());
					}
				}

				// If exception was passed add additional properties to the state and message template
				if (exception != null)
				{
					if(!telemetryProperties.Properties.ContainsKey(ExceptionOccurrenceIdKey))
						telemetryProperties.Properties.Add(ExceptionOccurrenceIdKey, null);

					if (!telemetryProperties.Properties.ContainsKey(ExceptionTypeKey))
						telemetryProperties.Properties.Add(ExceptionTypeKey, exception.GetType().Name);

					if (!telemetryProperties.Properties.ContainsKey(ExceptionMessageKey))
						telemetryProperties.Properties.Add(ExceptionMessageKey, exception.Message);

					if (!telemetryProperties.Properties.ContainsKey(ExceptionStackTraceKey))
						telemetryProperties.Properties.Add(ExceptionStackTraceKey, exception.StackTrace);

					if (!telemetryProperties.Properties.ContainsKey(ExceptionDepthKey))
						telemetryProperties.Properties.Add(ExceptionDepthKey, 1.ToString());
				}

				var stateType = state.GetType();
				var stateTypeInfo = stateType.GetTypeInfo();

				// TODO - message template stuff
				if (messageTemplate == null && !stateTypeInfo.IsGenericType)
				{
					messageTemplate = MessageTemplatePrefix + stateType.Name + MessageTemplatePostfix;
					// Bind the state property
				}

				string formattedState = formatter(state, null);
				telemetryProperties.Properties.Add(TelemetryProps.FormattedMessage, formattedState);
			}		 
			else if (state is MetricModel)
			{
				var metric = state as MetricModel;
				var metricModel = telemetryModel as MetricTelemetry;
				metricModel.Name = metric.Name;
				metricModel.Value = metric.Value;
				telemetryProperties.Properties.Add(TelemetryProps.CorrelationId, metric.CorrelationId);
				telemetryProperties.Properties.Add(TelemetryProps.MetricAggregation, "true");
			}
			else
			{
				telemetryProperties.Properties.Add(TelemetryProps.FormattedMessage, TelemetryProps.UnknownFormat);
			}
			
			// TODO - message template parsing
			if (messageTemplate == null && state != null)
			{
				// Bind in the property
			}

			// Map in the event id
			if (eventId.Id != 0)
				telemetryProperties.Properties.Add(TelemetryProps.Id, eventId.Id.ToString());
			if (eventId.Name != null)
				telemetryProperties.Properties.Add(TelemetryProps.Name, eventId.Name);
			// add the message template so we can include it later (if populated by previous steps)
			if (messageTemplate != null)
			{ 
				telemetryProperties.Properties.Add(TelemetryProps.MessageTemplate, messageTemplate);
			}

			// Push these ITelemetry types into the publishing pipeline
			if (telemetryModel is EventTelemetry)
			{
                _telemetryClient.TrackEvent(telemetryModel as EventTelemetry);

    //            // Special case due to AI Client SDK limit of 8KB max per Property, bypass the AI pipeline
    //            if (state is InteractionsOpenSchema)
				//{
				//	// Enrich EventTelemetry since we won't get this form AI pipeline
				//	foreach (var init in TelemetryConfiguration.Active.TelemetryInitializers)
				//		init.Initialize(telemetryModel);
				//	// Send directly to AppInsightBlobSink
				//	AppInsightLoggingManager.PostEntryToProcessors(telemetryModel);
				//}
				//else
				//{
					
				//}
			}
			else if (telemetryModel is MetricTelemetry)
			{
				_telemetryClient.TrackMetric(telemetryModel as MetricTelemetry);
			}
			else
			{
				throw new ArgumentException($"AppInsightsLogger.Log Invalid State object type");
			}
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
