using Nest;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AzureCAT.Samples
{
    public class ElasticSearchWrapper<T>
        where T : class
    {
        private readonly ElasticClient _elasticClient;
        private readonly Func<T, DateTimeOffset> _getDateFunc;

        public const string TemplateName = "azurestorage-template";
        public const string IndexPattern = "azurestorage-*";
        public const string IndexName = "azurestorage";

        private readonly string _templateName;
        private readonly string _indexPattern;
        private readonly string _indexName;

        public static async Task<ElasticSearchWrapper<T>> CreateAsync(
            Uri elasticSearchUri,
            Func<T, DateTimeOffset> getDateFunc,
            string templateName = TemplateName,
            string indexPattern = IndexPattern,
            string indexName = IndexName)
        {
            if (getDateFunc == null) throw new ArgumentNullException("getDateFunc");

            var ew = new ElasticSearchWrapper<T>(elasticSearchUri,
                getDateFunc, templateName, indexPattern, indexName);
            await ew.Initialize();
            return ew;
        }

        // [masimms] TODO - add logging
        protected ElasticSearchWrapper(Uri elasticSearchUri,
            Func<T, DateTimeOffset> getDateFunc,
            string templateName = TemplateName,
            string indexPattern = IndexPattern,
            string indexName = IndexName)
        {
            var settings = new ConnectionSettings(uri: elasticSearchUri);
            _elasticClient = new ElasticClient(settings);

            _templateName = templateName;
            _indexPattern = indexPattern;
            _indexName = indexName;
            _getDateFunc = getDateFunc;
        }

        public async Task Initialize()
        {
            // Check for the index mapping            
            var templateName = new Names(new string[] { _templateName });
            var template = await _elasticClient.GetIndexTemplateAsync(
                new GetIndexTemplateRequest(templateName));
            if (template != null && template.CallDetails.HttpStatusCode == 200)
                return;

            var response = await _elasticClient.PutIndexTemplateAsync(_templateName, t => t
                .Template(_indexName)
                .Settings(s => s
                    .NumberOfShards(1)
                )
               .Mappings(mds => mds
                    .Map<T>(s => s.AutoMap())
                )
            );

            // TODO - check response
        }

        public async Task Publish(IEnumerable<T> events)
        {
            try
            {
                var result = await _elasticClient.BulkAsync(b =>
                    b.IndexMany(events, (x, y) => x.Index(new IndexName()
                    {
                        Name = GetIndexName(_getDateFunc(y)),
                        Type = typeof(T)
                    })
                ));
                var items = result.Items;
            }
            catch (Exception ex)
            {

            }
        }

        private string GetIndexName(DateTimeOffset timestamp)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}-{1:yyyy.MM.dd}",
                    _indexName, timestamp);
        }
    }
}
