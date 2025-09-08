namespace MyTodo.Services;
using Serilog.Core;
using Serilog.Events;

public class UserEnricher : ILogEventEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    // O construtor é chamado pela injeção de dependência, que fornece o IHttpContextAccessor
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