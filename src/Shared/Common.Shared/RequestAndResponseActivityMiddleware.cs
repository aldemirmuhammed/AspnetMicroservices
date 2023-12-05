using Microsoft.AspNetCore.Http;
using Microsoft.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Shared
{
    public class RequestAndResponseActivityMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
        public RequestAndResponseActivityMiddleware(RequestDelegate next)
        {
            _next = next;
            _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await AddRequestBodyContentToActivityTags(context);
            await AddResponseBodyContentToActivityTags(context);
        }


        private async Task AddRequestBodyContentToActivityTags(HttpContext context)
        {
            if (context.Request.ContentType == "application/grpc")
                return;
            context.Request.EnableBuffering();
            var requestBodyStreamReader = new StreamReader(context.Request.Body);
            var requestBodyContent = await requestBodyStreamReader.ReadToEndAsync();
            Activity.Current?.SetTag("http.request.body", requestBodyContent);
            context.Request.Body.Position = 0;
        }


        private async Task AddResponseBodyContentToActivityTags(HttpContext context)
        {
            if (context.Response.ContentType == "application/grpc")
                return;
            var originalResponse = context.Response.Body;

            await using var responseBodyMemoryStream = _recyclableMemoryStreamManager.GetStream();

            context.Response.Body = responseBodyMemoryStream;
            await _next(context);

            responseBodyMemoryStream.Position = 0;
            var responseBodyStreamReader = new StreamReader(responseBodyMemoryStream);

            var responseBodyContent = await responseBodyStreamReader.ReadToEndAsync();

            Activity.Current?.SetTag("http.response.body", responseBodyContent);
            responseBodyMemoryStream.Position = 0;
            await responseBodyMemoryStream.CopyToAsync(originalResponse);
        }


    }
}
