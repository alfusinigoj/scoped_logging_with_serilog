using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Scoped.logging.Serilog.Models;
using Serilog.Context;

namespace Scoped.logging.Serilog.Middlewares
{
    public class ScopedSerilogSpecificLoggingMiddleware
    {
        const string CORRELATION_ID_HEADER_NAME = "CorrelationID";
        private readonly RequestDelegate next;
        private readonly ILogger<ScopedSerilogSpecificLoggingMiddleware> logger;

        public ScopedSerilogSpecificLoggingMiddleware(RequestDelegate next, ILogger<ScopedSerilogSpecificLoggingMiddleware> logger)
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
                //Add as many nested usings as needed, for adding more properties 
                using(LogContext.PushProperty(CORRELATION_ID_HEADER_NAME, correlationId, true))
                {
                    await next.Invoke(context);
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