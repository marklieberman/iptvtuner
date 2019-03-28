using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using uhttpsharp;
using uhttpsharp.Headers;

namespace IPTVTuner.Handlers
{
    class MyHandler
    {
        protected HttpResponse NotAvailable()
        {
            return new HttpResponse(HttpResponseCode.ServiceUnavailable, string.Empty, false);
        }

        protected HttpResponse NotFound()
        {
            return new HttpResponse(HttpResponseCode.NotFound, string.Empty, false);
        }

        protected IHttpResponse Found(string url)
        {
            return StringHttpResponse.Create(string.Empty, HttpResponseCode.Found, null, false, new ListHttpHeaders(new[] {
                new KeyValuePair<string, string>("Location", url)
            }));
        }

        protected HttpResponse JsonResponse(IHttpContext context, object value)
        {
            var serializer = new JsonSerializer();
            var memoryStream = new MemoryStream();
            var streamWriter = new StreamWriter(memoryStream);

            serializer.Serialize(streamWriter, value);

            streamWriter.Flush();
            memoryStream.Position = 0;

            return new HttpResponse(HttpResponseCode.Ok, "application/json; charset-utf-8", memoryStream, false);
        }

        protected HttpResponse JsonResponse(IHttpContext context, FileStream stream)
        {
            return new HttpResponse(HttpResponseCode.Ok, "application/json; charset-utf-8", stream, false);
        }

        protected HttpResponse XMLResponse(IHttpContext context, FileStream stream)
        {
            return new HttpResponse(HttpResponseCode.Ok, "application/xml; charset-utf-8", stream, false);
        }
    }
}
