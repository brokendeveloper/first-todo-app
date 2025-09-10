namespace MyTodo.Services.Enrichers;

using Serilog.Core;
using Serilog.Events;

public class RequestEnricher : ILogEventEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RequestEnricher(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return;

        // ✅ Capturar informações úteis da requisição
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("RequestPath", 
            httpContext.Request.Path.Value ?? "unknown"));
        
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("HttpMethod", 
            httpContext.Request.Method));
        
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("IpAddress", 
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"));
    }
}