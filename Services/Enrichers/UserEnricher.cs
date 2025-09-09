using Serilog.Core;
using Serilog.Events;

namespace MyTodo.Services.Enrichers;

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
        if (httpContext is null)
        {
            return;
        }

        var userContext = httpContext.RequestServices.GetService<UserContext>();

        if (userContext?.UserId != null)
        {
            var userIdProperty = propertyFactory.CreateProperty("UserId", userContext.UserId);
            logEvent.AddPropertyIfAbsent(userIdProperty);
        }
    }
}