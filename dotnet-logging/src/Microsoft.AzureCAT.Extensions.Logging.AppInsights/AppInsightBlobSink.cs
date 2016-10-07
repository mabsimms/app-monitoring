using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AzureCAT.Extensions.Logging.Sinks;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Microsoft.AzureCAT.Extensions.Logging.AppInsights
{
    public class AppInsightBlobSink : BlobContainerSink<ITelemetry>
    {
        private readonly ITelemetryProcessor _next;

        public AppInsightBlobSink(ITelemetryProcessor next,
            CloudBlobContainer blobContainer, 
            Func<string> blobPathFunc, 
            Func<CloudBlockBlob, Task> onBlobWrittenFunc, int bufferSize = 4194304) 
                : base(blobContainer, blobPathFunc, onBlobWrittenFunc, bufferSize)
        {
            this._next = next;
        }
    }
}
