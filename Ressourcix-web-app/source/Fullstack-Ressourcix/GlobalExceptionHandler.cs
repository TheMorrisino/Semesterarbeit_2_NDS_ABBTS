namespace FullstackRessourcix;

using Microsoft.AspNetCore.Diagnostics;

// Fängt jede sonst unbehandelte Ausnahme aus den Minimal-API-Handlern ab, loggt sie strukturiert
// und liefert dem Client eine einheitliche { message } - Antwort statt einer rohen 500-Fehlerseite
// oder eines Stacktraces. In Development wird die tatsächliche Exception-Message durchgereicht,
// in Produktion bewusst nur ein generischer Text (keine internen Details wie Connection-Strings
// oder Stacktraces nach aussen).
internal sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "Unbehandelte Ausnahme bei {Method} {Path}",
            httpContext.Request.Method,
            httpContext.Request.Path);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/json";

        var message = _environment.IsDevelopment()
            ? exception.Message
            : "Ein unerwarteter Fehler ist aufgetreten.";

        await httpContext.Response.WriteAsJsonAsync(new { message }, cancellationToken);
        return true;
    }
}
