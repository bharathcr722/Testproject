namespace Testproject.MiddleWare
{
    public class CustomLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CustomLoggingMiddleware> _logger;

        public CustomLoggingMiddleware(RequestDelegate next, ILogger<CustomLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _logger.LogInformation("Request path: {Path}", context.Request.Path);

            await _next(context);
            _logger.LogInformation("Response status code: {StatusCode}", context.Response.StatusCode);
        }
    }

}
