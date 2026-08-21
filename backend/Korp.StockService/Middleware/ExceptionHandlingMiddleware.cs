using System.Text.Json;
using Korp.StockService.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Korp.StockService.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Unhandled exception while processing {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            await WriteProblemDetailsAsync(context, exception);
        }
    }

    private static async Task WriteProblemDetailsAsync(
        HttpContext context,
        Exception exception)
    {
        var (status, title) = exception switch
        {
            ProductNotFoundException =>
                (StatusCodes.Status404NotFound, "Produto não encontrado"),
            ProductAlreadyExistsException =>
                (StatusCodes.Status409Conflict, "Produto duplicado"),
            InsufficientStockException =>
                (StatusCodes.Status409Conflict, "Saldo insuficiente"),
            ArgumentException =>
                (StatusCodes.Status400BadRequest, "Dados inválidos"),
            InvalidOperationException =>
                (StatusCodes.Status409Conflict, "Operação não permitida"),
            _ =>
                (StatusCodes.Status500InternalServerError, "Erro interno")
        };

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = status == StatusCodes.Status500InternalServerError
                ? "Ocorreu um erro inesperado no processamento da requisição."
                : exception.Message,
            Instance = context.Request.Path
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(problem),
            context.RequestAborted);
    }
}
