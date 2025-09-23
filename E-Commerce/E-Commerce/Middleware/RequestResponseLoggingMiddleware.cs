using System.Diagnostics;
using System.Text.Json;
using E_Commerce.DTO.LoggingDtos;
using Serilog;

namespace E_Commerce.Middleware
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestResponseLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            var requestInfo = await GetRequestInfo(context);

            var originalResponseBodyStream = context.Response.Body;         // Stores the original Response Body

            await using var responseBodyMemoryStream = new MemoryStream();
            context.Response.Body = responseBodyMemoryStream;

            await _next(context);

            var responseInfo = await GetResponseInfo(context);
            var executionTime = stopwatch.Elapsed.TotalSeconds;

            Log.Information(@"
                {Timestamp} | IP [{IpAddress}]
                {Url} ({Method})
                USER [{User}]
                REQUEST HEADERS: {RequestHeaders}
                REQUEST BODY: {RequestBody}
                RESPONSE HEADERS: {ResponseHeaders}
                RESPONSE BODY: {ResponseBody}
                STATUS CODE ~ {StatusCode} | EXECUTION_TIME | {ExecutionTime} seconds",
                requestInfo.Timestamp, requestInfo.IpAddress, requestInfo.Url, requestInfo.Method,
                requestInfo.User, requestInfo.Headers, requestInfo.Body, responseInfo.Headers,
                responseInfo.Body, responseInfo.StatusCode, executionTime);


            responseBodyMemoryStream.Seek(0, SeekOrigin.Begin);
            await responseBodyMemoryStream.CopyToAsync(originalResponseBodyStream);

            context.Response.Body = originalResponseBodyStream;
        }

        private static async Task<RequestDto> GetRequestInfo(HttpContext context)
        {
            var request = context.Request;

            string requestBody = "";  // Initialise empty string to store the Request Body

            // Reading the Request body
            if (request.ContentLength != 0)
            {
                request.EnableBuffering();  // Allows the request to be read multiple times
                using var reader = new StreamReader(request.Body, leaveOpen: true);
                requestBody = await reader.ReadToEndAsync();

                request.Body.Position = 0;  // Reset the position to start so it can be read again in further cases
            }

            return new RequestDto
            {
                Timestamp = DateTime.UtcNow.ToLocalTime().ToString("hh:mm:ss tt (UTC)"),
                IpAddress = context.Connection.RemoteIpAddress.ToString(),
                Method = request.Method,
                Url = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}",
                User = (context.User.Identity?.IsAuthenticated) == true ? context.User.Identity.Name : "Anonymous User (Not Logged In)",
                Headers = JsonSerializer.Serialize(request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())),
                Body = requestBody
            };
        }

        private static async Task<ResponseDto> GetResponseInfo(HttpContext context)
        {
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);

            return new ResponseDto
            {
                StatusCode = context.Response.StatusCode,
                Headers = JsonSerializer.Serialize(context.Response.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())),
                Body = responseBody
            };
        }
    }
}
