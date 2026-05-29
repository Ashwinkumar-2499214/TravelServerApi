using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using System.Text.Json;

namespace TravelEaseServer.Middleware
{
    public static class GlobalExceptionHandler
    {
        public static async Task HandleAsync(HttpContext context)
        {
            var feature = context.Features.Get<IExceptionHandlerFeature>();
            var exception = feature?.Error;

            if (exception is null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    message = Constant.GeneralConstants.InternalServerError,
                    error = "Unknown error"
                }));
                return;
            }

            int statusCode = exception switch
            {
                KeyNotFoundException => (int)HttpStatusCode.NotFound,
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                ArgumentException => (int)HttpStatusCode.BadRequest,
                _ => (int)HttpStatusCode.InternalServerError
            };

            var message = statusCode switch
            {
                (int)HttpStatusCode.NotFound => Constant.GeneralConstants.ResourceNotFound,
                (int)HttpStatusCode.Unauthorized => Constant.GeneralConstants.OperationFailed,
                (int)HttpStatusCode.BadRequest => Constant.GeneralConstants.InvalidInput,
                _ => Constant.GeneralConstants.InternalServerError
            };

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                message,
                error = exception.Message
            }));
        }
    }
}

