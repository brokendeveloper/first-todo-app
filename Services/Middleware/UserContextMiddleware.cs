namespace MyTodo.Services.Middleware;

public class UserContextMiddleware
{
    private readonly RequestDelegate _next;

    public UserContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, UserContext userContext)
    {
        Console.WriteLine("=== UserContextMiddleware executado ===");
        
        if (context.Request.Headers.TryGetValue("X-User-Id", out var userIdValue))
        {
            if (int.TryParse(userIdValue, out int userId))
            {
                userContext.UserId = userId;
                Console.WriteLine($"✅ UserId do header: {userId}");
            }
            else
            {
                Console.WriteLine($"❌ Header X-User-Id inválido: {userIdValue}");
            }
        }
        else
        {
            Console.WriteLine("ℹ️ Header X-User-Id não enviado");
        }
        
        await _next(context);
    }
}