using Serilog.Core;
using Serilog.Events;

namespace MyTodo.Services.Enrichers;

public class UtcTimestampEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        
        logEvent.AddOrUpdateProperty(
            propertyFactory.CreateProperty("Timestamp", logEvent.Timestamp.ToUniversalTime())
        );
    }
}