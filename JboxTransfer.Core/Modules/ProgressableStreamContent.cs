using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Core.Modules
{
    public class ProgressableStreamContent : HttpContent
    {
        public delegate void OnProgressDelegate(long uploaded);
        private const int defaultBufferSize = 4096;

        private Stream content;
        private int bufferSize;
        private bool contentConsumed;
        private OnProgressDelegate onProgress;

        public ProgressableStreamContent(Stream content, OnProgressDelegate onProgress) : this(content, defaultBufferSize, onProgress) { }

        public ProgressableStreamContent(Stream content, int bufferSize, OnProgressDelegate onProgress)
        {
            if (content == null)
            {
                throw new ArgumentNullException("content");
            }
            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException("bufferSize");
            }

            this.content = content;
            this.bufferSize = bufferSize;
            this.onProgress = onProgress;
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            Contract.Assert(stream != null);

            PrepareContent();

            return Task.Run(() =>
            {
                var buffer = new Byte[this.bufferSize];
                var size = content.Length;
                var uploaded = 0;

                onProgress(uploaded);

                while (true)
                {
                    var length = content.Read(buffer, 0, buffer.Length);
                    if (length <= 0) break;
                    stream.Write(buffer, 0, length);
                    uploaded += length;
                    onProgress(uploaded);
                }
            });
        }

        protected override bool TryComputeLength(out long length)
        {
            length = content.Length;
            return true;
        }

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        content.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}


        private void PrepareContent()
        {
            if (contentConsumed)
            {
                // If the content needs to be written to a target stream a 2nd time, then the stream must support
                // seeking (e.g. a FileStream), otherwise the stream can't be copied a second time to a target 
                // stream (e.g. a NetworkStream).
                if (content.CanSeek)
                {
                    content.Position = 0;
                }
                else
                {
                    throw new InvalidOperationException("SR.net_http_content_stream_already_read");
                }
            }

            contentConsumed = true;
        }
    }
}
