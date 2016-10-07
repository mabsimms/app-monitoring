using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace Microsoft.AzureCAT.Extensions.Logging.Sinks
{
    /// <summary>
    /// TODO - implement gzip compression for a blob chunk
    /// TODO - implement type demultiplexing (one type per blob) support 
    /// with broadcastblock and linkto with options
    /// </summary>
    public class BlobContainerSink<T> : IDisposable
    {
        protected readonly JsonSerializer _jsonSerializer;
        
        private readonly BufferBlock<T> _bufferBlock;
        private TransformBlock<T, byte[]> _transformBlock;
        private ActionBlock<byte[]> _publishBlock;

        private readonly MemoryStream _memoryBuffer;

        private readonly CloudBlobContainer _container;
        private readonly Func<string> _blobPathFunc;
        private readonly Func<CloudBlockBlob, Task> _blobWrittenFunc;
        private System.IDisposable[] _disposables;

        public BlobContainerSink(
            CloudBlobContainer blobContainer,
            Func<string> blobPathFunc,
            Func<CloudBlockBlob, Task> onBlobWrittenFunc,
            int bufferSize = 4 * 1024 * 1024)
        {            
            this._container = blobContainer;
            this._blobPathFunc = blobPathFunc;
            this._blobWrittenFunc = onBlobWrittenFunc;
            this._jsonSerializer = new JsonSerializer();

            var transmitBuffer = new byte[bufferSize];
            _memoryBuffer = new MemoryStream(transmitBuffer);


            _bufferBlock = new BufferBlock<T>(
                new DataflowBlockOptions()
                {
                    BoundedCapacity = 1024
                });

            _transformBlock = new TransformBlock<T, byte[]>(
                evt => Transform(evt),
                new ExecutionDataflowBlockOptions()
                {
                    BoundedCapacity = 1,
                    MaxDegreeOfParallelism = 1
                });

            _publishBlock = new ActionBlock<byte[]>(
                async evts => await Publish(evts),
                new ExecutionDataflowBlockOptions()
                {
                    BoundedCapacity = 16,
                    MaxDegreeOfParallelism = 1
                });

            _disposables = new IDisposable[]
            {
                _bufferBlock.LinkTo(_transformBlock),
                _transformBlock.LinkTo(_publishBlock)
            };


        }

        protected virtual byte[] Transform(T evt)
        {
            return Encoding.UTF8.GetBytes(
                JsonConvert.SerializeObject(evt)
            );
        }

        private async Task Publish(byte[] evts)
        {
            try
            {
                if (_memoryBuffer.Length - _memoryBuffer.Position > evts.Length)
                    _memoryBuffer.Write(evts, 0, evts.Length);
                else
                {
                    // Flush the buffer
                    // TODO - more advanced version and iterate through a pool of buffers
                    await WriteBuffer(_memoryBuffer, 0, _memoryBuffer.Position);

                    // Clear the buffer and write
                    _memoryBuffer.Position = 0;
                    _memoryBuffer.Write(evts, 0, evts.Length);
                }
            }
            catch (Exception ex)
            {
                // TODO - logging
                // CoreEventSource.Log.LogVerbose("Error in publishing to blob storage", ex.ToString());
            }
        }

        private async Task WriteBuffer(Stream buffer,
            int offset, long length)
        {
            var blobPath = _blobPathFunc();
            var blobReference = _container.GetBlockBlobReference(blobPath);
            _memoryBuffer.Position = 0;
            await blobReference
                .UploadFromStreamAsync(buffer, length)
                .ConfigureAwait(false);
            await _blobWrittenFunc(blobReference).ConfigureAwait(false);
        }

        public void Dispose()
        {
            _memoryBuffer?.Dispose();
        }     
    }
}
