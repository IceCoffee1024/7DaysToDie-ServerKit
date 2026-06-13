using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace SdtdServerKit.WebApi
{
    public class FileStreamResult : IHttpActionResult
    {
        private readonly Stream _fileStream;
        private readonly string _contentType;

        public FileStreamResult(Stream fileStream, string contentType)
        {
            if (fileStream == null)
                throw new ArgumentNullException(nameof(fileStream));

            if (contentType == null)
                throw new ArgumentNullException(nameof(contentType));

            _fileStream = fileStream;
            _contentType = contentType;
        }

        /// <inheritdoc/>
        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StreamContent(_fileStream);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(_contentType);
            return Task.FromResult(response);
        }
    }
}