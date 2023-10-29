using log4net;
using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Assessment.API.Middleware
{
    public class RequestResponseLogging
    {
        private readonly RequestDelegate _next;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(RequestResponseLogging));

        public RequestResponseLogging(RequestDelegate next)
        {
            _next = next;

        }

        public Task InvokeAsync(HttpContext context)
        {
            var watch = new Stopwatch();
            watch.Start();
            context.Response.OnStarting(() =>
            {
                watch.Stop();
                _logger.Info(context.Request.Scheme + "://" + context.Request.Host + context.Request.Path + " Time :-" + watch.ElapsedMilliseconds.ToString() + " StatusCode :-" + context.Response.StatusCode.ToString());
                return Task.CompletedTask;
            });
            return this._next(context);
        }
    }
}

