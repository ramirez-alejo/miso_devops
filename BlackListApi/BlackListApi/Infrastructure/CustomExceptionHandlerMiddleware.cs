using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace BlackListApi.Infrastructure;

 public class CustomExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public CustomExceptionHandlerMiddleware(RequestDelegate next, ILogger<CustomExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex).ConfigureAwait(false);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var code = HttpStatusCode.InternalServerError;
            object result = null;
            
            LogException(exception);

            switch (exception)
            {
                case FluentValidation.ValidationException fluentValException:
                    result = new { errors = BuildDictionaryFromFluentValidation(fluentValException) };
                    code = HttpStatusCode.BadRequest;
                    break;
                case Exceptions.ValidationException validationException:
                    code = HttpStatusCode.PreconditionFailed;
                    result = new { errors = validationException.Failures };
                    break;
                case NotSupportedException:
                    code = HttpStatusCode.BadRequest;
                    break;
                case Exceptions.ForbiddenAccessException:
                    code = HttpStatusCode.Forbidden;
                    result = new ProblemDetails
                    {
                        Status = StatusCodes.Status403Forbidden,
                        Title = "Forbidden",
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3"
                    };
                    break;
                case Exceptions.UnauthenticatedException:
                    code = HttpStatusCode.Unauthorized;
                    result = new ProblemDetails
                    {
                        Status = StatusCodes.Status401Unauthorized,
                        Title = "Unauthorized",
                        Type = "https://datatracker.ietf.org/doc/html/rfc7235#section-3.1"
                    };
                    break;
                case Exceptions.NotFoundException _:
                    code = HttpStatusCode.NotFound;
                    break;
                default:
                    break;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;

            var jsonResult = JsonSerializer.Serialize(result);

            result ??= new { error = exception.Message };

            if (context.Response.StatusCode >= 500)
            {
                _logger.LogError(exception, "API Error {ApiException}", jsonResult);    
            }
            else
            {
                //errors will trigger our monitoring alerts..
                //400-499 should be treated as a warning to be diagnosed only if needed.
                _logger.LogWarning("API Error {ApiException}", jsonResult);
            }
            
            return context.Response.WriteAsync(jsonResult);
        }

        private void LogException(Exception exception)
        {
            //No innerException logging is necessary because NLog handles it
            _logger.LogError(exception, "An unexpected error occurred: {ExceptionMessage}", exception.Message);
        }
        
        private IDictionary<string, string[]> BuildDictionaryFromFluentValidation(
            FluentValidation.ValidationException exc)
        {
            var errorsGroupedByProperty = exc.Errors.GroupBy(err => err.PropertyName);
            var errorDictionary = errorsGroupedByProperty.ToDictionary(
                g => g.Key,
                g => g.Select(err => err.ErrorMessage).ToArray()
            );
            return errorDictionary;
        }
    }

    public static class CustomExceptionHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CustomExceptionHandlerMiddleware>();
        }
    }
