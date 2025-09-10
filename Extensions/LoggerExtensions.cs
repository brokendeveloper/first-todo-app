using MyTodo.Models.Enum;
using Serilog.Context;

namespace MyTodo.Extensions;

public static class LoggerExtensions
{
    // ✅ Método específico para auditoria que funciona com seu enum
    public static void LogAudit(this ILogger logger, LogAction action, string message, object data = null)
    {
        using (LogContext.PushProperty("ActionId", (int)action))
        {
            if (data != null)
            {
                using (LogContext.PushProperty("AuditData", data, true))
                {
                    logger.LogInformation(message);
                }
            }
            else
            {
                logger.LogInformation(message);
            }
        }
    }

    public static void LogAuditWarning(this ILogger logger, LogAction action, string message, object data = null)
    {
        using (LogContext.PushProperty("ActionId", (int)action))
        {
            if (data != null)
            {
                using (LogContext.PushProperty("AuditData", data, true))
                {
                    logger.LogWarning(message);
                }
            }
            else
            {
                logger.LogWarning(message);
            }
        }
    }

    public static void LogAuditError(this ILogger logger, LogAction action, string message, Exception ex = null, object data = null)
    {
        using (LogContext.PushProperty("ActionId", (int)action))
        {
            if (data != null)
            {
                using (LogContext.PushProperty("AuditData", data, true))
                {
                    logger.LogError(ex, message);
                }
            }
            else
            {
                logger.LogError(ex, message);
            }
        }
    }
}