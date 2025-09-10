namespace MyTodo.Services.Enrichers;

using Serilog.Core;
using Serilog.Events;

public class ActionEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        // Se não tem ActionId, não processar
        if (!logEvent.Properties.TryGetValue("ActionId", out var actionIdProperty))
            return;

        // ✅ Garantir que ActionId seja sempre integer
        if (actionIdProperty is ScalarValue scalar && scalar.Value != null)
        {
            // Se já é int, manter
            if (scalar.Value is int intValue)
            {
                logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("ActionId", intValue));
                return;
            }

            // Se é enum, converter para int
            if (scalar.Value is Enum enumValue)
            {
                var intFromEnum = Convert.ToInt32(enumValue);
                logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("ActionId", intFromEnum));
                return;
            }

            // Se é string, tentar converter
            if (int.TryParse(scalar.Value.ToString(), out var parsedValue))
            {
                logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("ActionId", parsedValue));
            }
        }
    }
}