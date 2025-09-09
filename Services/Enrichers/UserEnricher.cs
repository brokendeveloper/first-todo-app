namespace MyTodo.Services;
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
        if (httpContext is null)
        {
            return;
        }
    
        var userContext = httpContext.RequestServices.GetService<UserContext>();
   
        if (userContext?.UserId.HasValue == true)
        {
            var longUserId = userContext.UserId.Value;
        
            
            if (longUserId <= int.MaxValue && longUserId >= int.MinValue)
            {
                var userId = (int)longUserId;
                var userIdProperty = propertyFactory.CreateProperty("UserId", userId);
                logEvent.AddPropertyIfAbsent(userIdProperty);
            }
        }
    }
}
