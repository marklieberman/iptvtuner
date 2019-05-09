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
            var headers = new ListHttpHeaders(new[] {
                new KeyValuePair<string, string>("Location", url)
            });

            return StringHttpResponse.Create(string.Empty, HttpResponseCode.Found, null, false, headers);
        }

        protected HttpResponse JsonResponse(IHttpContext context, object value)
        {
            var headers = new ListHttpHeaders(new[] {
                new KeyValuePair<string, string>("Cache-Control", "no-store")
            });

            var serializer = new JsonSerializer();
            var stream = new MemoryStream();
            var streamWriter = new StreamWriter(stream);

            serializer.Serialize(streamWriter, value);

            streamWriter.Flush();
            stream.Position = 0;

            return new HttpResponse(HttpResponseCode.Ok, "application/json; charset-utf-8", stream, false, headers);
        }

        protected HttpResponse JsonResponse(IHttpContext context, FileStream stream)
        {
            var headers = new ListHttpHeaders(new[] {
                new KeyValuePair<string, string>("Cache-Control", "no-store")
            });

            return new HttpResponse(HttpResponseCode.Ok, "application/json; charset-utf-8", stream, false, headers);
        }

        protected HttpResponse XMLResponse(IHttpContext context, FileStream stream)
        {
            var headers = new ListHttpHeaders(new[] {
                new KeyValuePair<string, string>("Cache-Control", "no-store")
            });

            return new HttpResponse(HttpResponseCode.Ok, "application/xml; charset-utf-8", stream, false, headers);
        }
    }
}
