using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XComStreamApp
{
    // Simple middleware that prevents chunked encoding in responses to XCOM 2, to simplify the HTTP implementation over there
    public class DeChunkerMiddleware
    {
        private readonly RequestDelegate _next;

        public DeChunkerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var originalBodyStream = context.Response.Body;

            using (var responseBody = new MemoryStream())
            {
                context.Response.Body = responseBody;

                await _next(context);
                context.Response.Headers.ContentLength = context.Response.Body.Length;

                context.Response.Body.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }
    }
}
