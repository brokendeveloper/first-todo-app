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
        if (context.Request.Headers.TryGetValue("X-User-Id", out var userIdValue))
        {
            if (int.TryParse(userIdValue, out int userId))
            {
                userContext.UserId = userId;
            }
        }
        await _next(context);
    }
}