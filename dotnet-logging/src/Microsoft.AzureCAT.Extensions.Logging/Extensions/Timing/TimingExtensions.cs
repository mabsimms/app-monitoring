using System;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace Microsoft.AzureCAT.Extensions.Logging.Timing
{
    public static class TimingExtensions
    {

        public static void LogTiming(this ILogger logger,
           string categoryName, string operationType,
           TimeSpan elapsed, string status = null)
        {
            if (String.IsNullOrEmpty(status))
            {
                logger.LogInformation(
                    "{eventType}; Completed operation {categoryName}:{operationType} in {elapsed}",
                    "ActionTiming", categoryName, operationType, elapsed);
            }
            else
            {
                logger.LogInformation(
                    "{eventType}; Completed operation {categoryName}:{operationType} in {elapsed} with status {status}",
                    "ActionTiming", categoryName, operationType, elapsed, status);
            }
        }

        public static void LogTiming(this ILogger logger,
            string operationType, TimeSpan elapsed, string status = null)
        {
            if (String.IsNullOrEmpty(status))
            {
                logger.LogInformation(
                    "{eventType}; Completed operation {categoryName}:{operationType} in {elapsed}",
                    "ActionTiming", "default", operationType, elapsed);
            }
            else
            {
                logger.LogInformation(
                   "{eventType}; Completed operation {categoryName}:{operationType} in {elapsed} with status {status}",
                   "ActionTiming", "default", operationType, elapsed, status);
            }
        }

        public static void LogTiming(this ILogger logger,
            string operationType, TimeSpan elapsed,
            Guid operationId, string status = null)
        {
            if (String.IsNullOrEmpty(status))
            {
                logger.LogInformation(
                    "{eventType}; Completed operation {operationType} in {elapsed} (id {operationId})",
                    "ActionTiming", operationType, elapsed, operationId);
            }
            else
            {
                logger.LogInformation(
                    "{eventType}; Completed operation {operationType} in {elapsed} (id {operationId}) with status {status}",
                    "ActionTiming", operationType, elapsed, operationId, status);
            }
        }

        public static void LogTiming(this ILogger logger,
          string categoryName, string operationType, TimeSpan elapsed,
          Guid operationId, string status = null)
        {
            if (String.IsNullOrEmpty(status))
            {
                logger.LogInformation(
                    "{eventType}; Completed operation {categoryName}:{operationType} in {elapsed} (id {operationId})",
                    "ActionTiming", categoryName, operationType, elapsed, operationId);
            }
            else
            {
                logger.LogInformation(
                   "{eventType}; Completed operation {categoryName}:{operationType} in {elapsed} (id {operationId}) with status {status}",
                   "ActionTiming", categoryName, operationType, elapsed, operationId, status);
            }
        }
    }
}
