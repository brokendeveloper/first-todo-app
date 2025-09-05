namespace MyTodo.Services.Middleware;

public class UserContextMiddleware
{
   private readonly RequestDelegate _next;

   public UserContextMiddleware(RequestDelegate next)
   {
      _next = next;
   }

   public async Task Invoke(HttpContext context, UserContext userContext)
   {
      if (context.Request.Headers.TryGetValue("X-User-Id", out var userIdValue))
      {
         if (long.TryParse(userIdValue, out long userId))
         {
            userContext.UserId = userId;
         }
      }
      await _next(context); 
   }
}