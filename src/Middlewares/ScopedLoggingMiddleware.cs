using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Scoped.logging.Serilog.Models;
using Serilog.Context;

namespace Scoped.logging.Serilog.Middlewares
{
    public class ScopedLoggingMiddleware
    {
        const string CORRELATION_ID_HEADER_NAME = "CorrelationId";        
        private readonly RequestDelegate next;
        private readonly ILogger<ScopedLoggingMiddleware> logger;

        public ScopedLoggingMiddleware(RequestDelegate next, ILogger<ScopedLoggingMiddleware> logger)
        {
            this.next = next ?? throw new System.ArgumentNullException(nameof(next));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Invoke(HttpContext context)
        {
            if (context == null) throw new System.ArgumentNullException(nameof(context));

            var correlationId = GetOrAddCorrelationHeader(context);

            try
            {
                var loggerState = new LogerState
                {
                    [CORRELATION_ID_HEADER_NAME] = correlationId
                    //Add any number of properties to be logged under a single scope
                };

                using (logger.BeginScope(loggerState))
                {
                    await next(context);
                }
            }
            //To make sure that we don't loose the scope in case of an unexpected error
            catch (Exception ex) when (LogOnUnexpectedError(ex))
            {
                return;
            }
        }

        private string GetOrAddCorrelationHeader(HttpContext context)
        {
            if (context == null) throw new System.ArgumentNullException(nameof(context));

            if(string.IsNullOrWhiteSpace(context.Request.Headers[CORRELATION_ID_HEADER_NAME]))
                context.Request.Headers[CORRELATION_ID_HEADER_NAME] = Guid.NewGuid().ToString();

            return context.Request.Headers[CORRELATION_ID_HEADER_NAME];
        }

        private bool LogOnUnexpectedError(Exception ex)
        {
            logger.LogError(ex, "An unexpected exception occured!");
            return true;
        }
    }
}