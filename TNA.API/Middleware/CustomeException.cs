using TNA.API.Helper;
using TNA.API.Model;
//using Courses.API.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace Courses.API.Middleware
{
    public class HttpStatusCodeException : Exception
    {
        public int StatusCode { get; set; }
        public string ContentType { get; set; } = @"text/plain";

        public HttpStatusCodeException(int statusCode)
        {
            this.StatusCode = statusCode;
        }

        public HttpStatusCodeException(int statusCode, string message) : base(message)
        {
            this.StatusCode = statusCode;
        }

        public HttpStatusCodeException(int statusCode, Exception inner) : this(statusCode, inner.ToString()) { }

        public HttpStatusCodeException(int statusCode, JObject errorObject) : this(statusCode, errorObject.ToString())
        {
            this.ContentType = @"application/json";
        }
    }
    public class DeveloperExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<DeveloperExceptionMiddleware> _logger;

        public DeveloperExceptionMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = loggerFactory?.CreateLogger<DeveloperExceptionMiddleware>() ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                //Log Exception Here
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }
        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var result = JsonConvert.SerializeObject(new { Message = exception });
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 500;
            return context.Response.WriteAsync(result);
        }
    }


    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class HttpStatusCodeExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseDeveloperExceptionMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<DeveloperExceptionMiddleware>();
        }
    }
    public class ProductionExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ProductionExceptionMiddleware> _logger;
        private readonly IConfiguration _configuration;
        public ProductionExceptionMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = loggerFactory?.CreateLogger<ProductionExceptionMiddleware>() ?? throw new ArgumentNullException(nameof(loggerFactory));
            _configuration = configuration;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                //Log Exception Here
                await _next(context);
            }
            catch (Exception ex)
            {
                await AddExceptionToDb(context, ex);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var result = JsonConvert.SerializeObject(new
            {
                Message = "Internal Server Error",
                Description = "Internal Server Error"
            });
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 500;
            return context.Response.WriteAsync(result);
        }
        private async Task AddExceptionToDb(HttpContext context, Exception exception)
        {
            using (var db = GetDbContext(context))
            {
                using (var cmd = db.Database.GetDbConnection().CreateCommand())
                {
                    ExceptionLog exceptionLog = new ExceptionLog();
                    exceptionLog.CreatedDate = DateTime.UtcNow;
                    exceptionLog.Message = exception.Message;
                    exceptionLog.Source = exception.Source;
                    exceptionLog.StackTrace = exception.StackTrace;
                    cmd.CommandText = "LogException";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@Source", SqlDbType.VarChar) { Value = exceptionLog.Source });
                    cmd.Parameters.Add(new SqlParameter("@Message", SqlDbType.VarChar) { Value = exceptionLog.Message });
                    cmd.Parameters.Add(new SqlParameter("@StackTrace", SqlDbType.VarChar) { Value = exceptionLog.StackTrace });
                    cmd.Parameters.Add(new SqlParameter("@CreatedDate", SqlDbType.VarChar) { Value = exceptionLog.CreatedDate });
                    await db.Database.OpenConnectionAsync();
                    DbDataReader reader = await cmd.ExecuteReaderAsync();
                    await db.Database.OpenConnectionAsync();
                }
            }
        }
        private CourseContext GetDbContext(HttpContext context)
        {
            string ConnectionString = null;
            if (context != null)
            {
                var httpContext = context;
                string encryptedConnectionString = httpContext.User.FindFirst("address") == null ? null : httpContext.User.FindFirst("address").Value;
                if (!string.IsNullOrEmpty(encryptedConnectionString))
                    ConnectionString = Security.Decrypt(encryptedConnectionString);
            }
            else
            {

                ConnectionString = (new ConnStringEncDec()).GetDefaultConnectionString();
            }
            var optionsBuilder = new DbContextOptionsBuilder<CourseContext>();
            optionsBuilder.UseSqlServer(ConnectionString, option => option.UseRowNumberForPaging())
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            return new CourseContext(optionsBuilder.Options);
        }
    }


    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class ProductionExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseProductionExceptionMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ProductionExceptionMiddleware>();
        }
    }
}
