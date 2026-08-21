using Korp.StockService.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Korp.StockService.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled exception while processing {Method} {Path}", context.Request.Method, context.Request.Path);
            await WriteProblemDetailsAsync(context, exception);
        }
    }

    private static async Task WriteProblemDetailsAsync(HttpContext context, Exception exception)
    {
        var (status, title) = exception switch
        {
            ProductNotFoundException => (404, "Produto não encontrado"),
            ProductAlreadyExistsException => (409, "Produto duplicado"),
            ArgumentException => (400, "Dados inválidos"),
            InvalidOperationException => (409, "Operação não permitida"),
            _ => (500, "Erro interno")
        };

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = status == 500 ? "Ocorreu um erro inesperado." : exception.Message,
            Instance = context.Request.Path
        });
    }
}
