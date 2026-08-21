using System.Text.Json;
using Korp.BillingService.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Korp.BillingService.Middleware;

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
                "Erro ao processar {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            var (status, title) = exception switch
            {
                InvoiceNotFoundException =>
                    (StatusCodes.Status404NotFound, "Nota fiscal não encontrada"),
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
                    ? "Ocorreu um erro inesperado."
                    : exception.Message,
                Instance = context.Request.Path
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(problem),
                context.RequestAborted);
        }
    }
}
