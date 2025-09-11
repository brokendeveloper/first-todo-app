namespace MyTodo.Extensions;

using Serilog.Context;
using MyTodo.Models.Enum;

public static class LoggerExtensions
{
    public static void LogAudit(this ILogger logger, LogAction action, string operationDescription, object details = null)
    {
        using (LogContext.PushProperty("ActionId", (int)action))
        {
            if (details != null)
            {
                AddDetailsAsProperties(details);
            }
            
            logger.LogInformation(operationDescription);
        }
    }

    public static void LogAuditWarning(this ILogger logger, LogAction action, string operationDescription, object details = null)
    {
        using (LogContext.PushProperty("ActionId", (int)action))
        {
            if (details != null) AddDetailsAsProperties(details);
            logger.LogWarning(operationDescription);
        }
    }

    public static void LogAuditError(this ILogger logger, LogAction action, string operationDescription, Exception ex = null, object details = null)
    {
        using (LogContext.PushProperty("ActionId", (int)action))
        {
            if (details != null) AddDetailsAsProperties(details);
            logger.LogError(ex, operationDescription);
        }
    }

    private static void AddDetailsAsProperties(object details)
    {
        if (details == null) return;

        var properties = details.GetType().GetProperties();
        foreach (var prop in properties)
        {
            var value = prop.GetValue(details);
            if (value != null)
            {
                LogContext.PushProperty(prop.Name, value);
            }
        }
    }
}