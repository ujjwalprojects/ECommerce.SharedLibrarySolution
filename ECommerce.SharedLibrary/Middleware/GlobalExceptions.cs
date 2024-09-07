using ECommerce.SharedLibrary.Logs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace ECommerce.SharedLibrary.Middleware
{
    public class GlobalExceptions
    {
        private readonly RequestDelegate _next;

        public GlobalExceptions(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Declare Default Variables
            string message = "Sorry, an internal server error occurred. Kindly try again.";
            int statusCode = (int)HttpStatusCode.InternalServerError;
            string title = "Error";

            try
            {
                await _next(context);

                // Check if exception is too many requests // Error Code 429 status code
                if (context.Response.StatusCode == StatusCodes.Status429TooManyRequests)
                {
                    title = "Warning";
                    statusCode = (int)StatusCodes.Status429TooManyRequests;
                    message = "Sorry, too many requests. Kindly try again later.";
                    await ModifyHeader(context, title, message, statusCode);
                }
                //if response is UnAuthorized // 401 Status Code
                if(context.Response.StatusCode == StatusCodes.Status401Unauthorized)
                {
                    title = "Alert";
                    message = "Sorry, you are not authorized to access this resource.";
                    await ModifyHeader(context, title, message, statusCode);
                }
                // If response is forbidden // 403 status code
                if(context.Response.StatusCode == StatusCodes.Status403Forbidden)
                {
                    title = "Out of Access";
                    message = "Sorry, you are not authorized to access this resource.";
                    await ModifyHeader(context, title, message, statusCode);
                }
                // If response is not found // 404 status code
                if(context.Response.StatusCode == StatusCodes.Status404NotFound)
                {
                    title = "Not Found";
                    message = "Sorry, the requested resource was not found.";
                    await ModifyHeader(context, title, message, statusCode);
                }

            }
            catch (Exception ex)
            {
                // log original exceptions // File, Debugger, Console // 408 request timeout
                LogException.LogExceptions(ex);

                // check if exceptions is timeout
                if(ex is TaskCanceledException || ex is TimeoutException)
                {
                    title = "Out of time";
                    message = "Request timeout.. try again";
                    statusCode = StatusCodes.Status408RequestTimeout;
                }
                // If Exception is caught
                // If none of these exceptions  then do the default
                await ModifyHeader(context, title, message, statusCode);
            }
        }

        private async Task ModifyHeader(HttpContext context, string title, string message, int statusCode)
        {
            // Display a user-friendly message to the client
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = message
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
        }
    }
}
