using Microsoft.AzureCAT.Extensions.Logging.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.AzureCAT.Samples
{
    public class ElasticSearchBulkPublisher<T> where T : class   
    {
        private readonly ElasticSearchWrapper<T> _publisher;

        public async static Task<ElasticSearchBulkPublisher<T>> CreateAsync(
            ElasticSearchWrapper<T> publisher,
            TimeSpan windowSize,       
            int maxItemsInBatch = 1024,
            int maxPendingBatches = 128,
            int maxDop = 4)
        {
            var pub = new ElasticSearchBulkPublisher<T>(publisher,
                windowSize, maxItemsInBatch, maxDop);
            await pub.InitializeAsync();
            return pub;
        }

        protected readonly BatchingPublisher<T> _batcher;
    
        protected ElasticSearchBulkPublisher(ElasticSearchWrapper<T> publisher,
            TimeSpan windowSize,
            int maxItemsInBatch,
            int maxDop)
        {
            this._batcher = new BatchingPublisher<T>(maxItemsInBatch, windowSize, publisher.Publish);
            this._publisher = publisher;
        }

        protected virtual Task InitializeAsync()
        {
            return Task.FromResult(0);
        }

        public void Emit(T logEvent)
        {
            _batcher.Send(logEvent); 
        }

        public async Task PublishEvents(
            IEnumerable<T> events)
        {
            await _publisher.Publish(events);
        }
    }
}
