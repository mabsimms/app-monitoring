using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;
using Microsoft.AzureCAT.Extensions.Logging.AppInsights.Microsoft.ApplicationInsights.Extensibility.Implementation.Tracing;

namespace Microsoft.AzureCAT.Extensions.Logging.Sinks
{
    // TODO - subclass or encapsulate the buffering publisher
    // TODO - optimize the tcp socket management to maintain sockets
    public abstract class GraphitePublisherBase<T> : System.IDisposable
    {        
        protected readonly string _host;
        protected readonly int _port;
        protected readonly BatchBlock<T> _batchBlock;        
        protected readonly ActionBlock<IEnumerable<T>> _actionBlock;
        protected readonly CancellationTokenSource _tokenSource;
        private System.Threading.Timer _windowTimer;
        private System.IDisposable[] _disposables;

        // TODO - how to enrich the metric name with the source identifier
        public GraphitePublisherBase(string hostName) : 
            this(hostName, 2003, System.TimeSpan.FromSeconds(1), 100)
        { }

        public GraphitePublisherBase(
            string hostName,
            int port,
            System.TimeSpan maxFlushTime,
            int maxWindowEventCount = 100)
        {            
            this._host = hostName;
            this._port = port;
            this._tokenSource = new CancellationTokenSource();

            this._batchBlock = new BatchBlock<T>(maxWindowEventCount,
                new GroupingDataflowBlockOptions()
                {
                    BoundedCapacity = maxWindowEventCount,
                    Greedy = true,
                    CancellationToken = _tokenSource.Token
                });

            this._actionBlock = new ActionBlock<IEnumerable<T>>(
                 async (e) => await Publish(e),
                 new ExecutionDataflowBlockOptions()
                 {
                     MaxDegreeOfParallelism = 1,
                     BoundedCapacity = 2,
                     CancellationToken = _tokenSource.Token
                 });

            var disp = new List<System.IDisposable>();
            disp.Add(_batchBlock.LinkTo(_actionBlock));
            _disposables = disp.ToArray();

            this._windowTimer = new Timer(FlushBuffer, null,
                maxFlushTime, maxFlushTime);
        }

        protected void FlushBuffer(object state)
        {
            _batchBlock?.TriggerBatch();
        }

        public virtual void Process(T item)
        {
            this._batchBlock.Post(item);            
        }

        protected async Task Publish(IEnumerable<T> events)
        {
            if (events == null)
                return;
            try
            {
                var eventsList = events.ToArray();
                if (eventsList.Length == 0)
                    return;

                var content = GetContent(eventsList);
                await PublishEvents(content);
            }
            catch (System.Exception ex)
            {           
                CoreEventSource.Log.LogVerbose("GraphitePublisher publish failed: ", ex.ToString());
            }
        }

        protected abstract IList<string> GetContent(IEnumerable<T> events);

        protected async Task PublishEvents(IList<string> events)
        {
            if (events == null || events.Count == 0)
                return;

            using (var tcpClient = new TcpClient())
            {
                await tcpClient.ConnectAsync(_host, _port);
                using (var stream = tcpClient.GetStream())
                using (var sw = new StreamWriter(stream))
                {
                    foreach (var e in events)
                    {
                        await sw.WriteLineAsync(e);                       
                    }
                }
            }            
        }
         
        public void Dispose()
        {
            _tokenSource.Cancel();

            if (_disposables != null)
            {
                foreach (var d in _disposables)
                    d.Dispose();
                _disposables = null;
            }

            if (_windowTimer != null)
            {
                _windowTimer.Dispose();
                _windowTimer = null;
            }
        }
    }
}
