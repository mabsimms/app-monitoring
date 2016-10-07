using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;

namespace Microsoft.AzureCAT.Extensions.Logging.AppInsights
{
    public class AppInsightLoggingManager
    {
        public static async Task Initialize(IConfiguration config)
        {
            // Use the custom in-memory pipeline publishing channel
            TelemetryConfiguration.Active.TelemetryChannel =
                new InMemoryPublishingChannel(500, TimeSpan.FromSeconds(30));

            // Set up a custom app insights pipeline
            var aiClientBuilder = TelemetryConfiguration.Active
                .TelemetryProcessorChainBuilder;

            // Publish raw events directly to blob storage
            //await PublishToBlobStorage(config);

            // Set up an in-memory aggregator
            var aggConfigSection = config.GetSection("ApplicationInsights")
                .GetSection("SlidingWindow");
            aiClientBuilder.Use((next) => new AppInsightAggregator(
                next: next,
                config: aggConfigSection));

            // Set up graphite configuration
            aiClientBuilder.Use((next) => new AppInsightGraphiteSink(next, null,
                "localhost"));

            // Update the ai client configuration
            aiClientBuilder.Build();

            var tc = new TelemetryClient();
            tc.TrackMetric("fred", 1.0);
        }

        private async static Task PublishToBlobStorage(IConfiguration config)
        {
            var blobConfigSection = config.GetSection("ApplicationInsights")
               .GetSection("BlobPublisher");
            var storageAccountString = blobConfigSection.GetValue<string>("BlobAccount");
            var containerName = blobConfigSection.GetValue<string>("ContainerName");

            var storageAccount = CloudStorageAccount.Parse(storageAccountString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(containerName);

            var createdContainer = await blobContainer.CreateIfNotExistsAsync();
            if (createdContainer)
            {
                // TODO
            }

            // TODO - blob publisher
            //aiClientBuilder.Use((next) => new AppInsightBlobSink(
            //    next: next,
            //    blobContainer: blobContainer,
            //    // TODO - put the real naming function in here
            //    blobPathFunc: () => String.Format("todo.json"),
            //    // TODO - adjust the callback to infer schema name from the type
            //    // TODO - adjust the blob writer to handle interleaved schema
            //    onBlobWrittenFunc: async (blob) => await OpenSchemaCallback.PostCallback(
            //        blob: blob,
            //        endpoint: new Uri(""),
            //        schemaName: "TODO",
            //        iKey: Configuration.GetSection("ApplicationInsights")
            //            .GetValue<string>("InstrumentationKey")),
            //    bufferSize: 4 * 1024));
        }
    }
}
