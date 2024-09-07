using Microsoft.AspNetCore.Http;

namespace ECommerce.SharedLibrary.Middleware
{
    public class ListenToOnlyAPIGateway
    {
        private readonly RequestDelegate _next;

        public ListenToOnlyAPIGateway(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Extract specific header from the request
            var signedHeader = context.Request.Headers["Api-Gateway"];

            //NUll means the request is not coming from the Api Gateway
            // 503 service unavailable
            if (signedHeader.FirstOrDefault() is null)
            {

                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                await context.Response.WriteAsync("Sorry, service is unavailable");
                return;
            }
            else
            {
                await _next(context);
            }
        }
    }
}
