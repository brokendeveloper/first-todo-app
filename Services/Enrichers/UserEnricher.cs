namespace MyTodo.Services.Enrichers;

using Serilog.Core;
using Serilog.Events;

public class UserEnricher : ILogEventEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserEnricher(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return;

        // ✅ Pegar UserContext do ServiceProvider
        var userContext = httpContext.RequestServices.GetService<UserContext>();
        
        if (userContext?.UserId.HasValue == true)
        {
            // ✅ Garantir que é integer, não string
            logEvent.AddPropertyIfAbsent(
                propertyFactory.CreateProperty("UserId", userContext.UserId.Value));
        }
        // ✅ Não adicionar propriedade se UserId for null (evita problemas no banco)
    }
}