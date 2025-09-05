using Serilog.Core;
using Serilog.Events;

namespace MyTodo.Services;

public class UserEnricher : ILogEventEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserEnricher() : this(new HttpContextAccessor())
    {
    }

    public UserEnricher(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var userContext = _httpContextAccessor.HttpContext?.RequestServices.GetService<UserContext>();

        if (userContext?.UserId != null)
        {
            var userIdProperty = propertyFactory.CreateProperty("UserId", userContext.UserId);
            logEvent.AddPropertyIfAbsent(userIdProperty);
        }
    }
}