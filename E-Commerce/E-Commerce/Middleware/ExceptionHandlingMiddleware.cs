using System.Net;
using System.Text.Json;
using E_Commerce.Utils;

namespace E_Commerce.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {

                await HandleException(context, ex);
            }
        }

        public async Task HandleException(HttpContext context, Exception ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var res = new ApiResponse(false, context.Response.StatusCode, ex.Message);
            var jsonResponse = JsonSerializer.Serialize(res);
            
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
