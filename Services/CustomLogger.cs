using System.Text.Json;
using MyTodo.Data;
using MyTodo.Models;
using MyTodo.Models.Enum;
using MyTodo.Services;

public class CustomLogger : ICustomLogger
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CustomLogger(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    private int GetUserIdFromHeader()
    {
        var headers = _httpContextAccessor.HttpContext?.Request.Headers;
        if (headers != null && headers.TryGetValue("X-User-Id", out var userIdStr))
        {
            return int.TryParse(userIdStr, out var userId) ? userId : 0;
        }
        return 0;
    }

    public async Task LogAsync(LogAction action, object descricao)
    {
        var userId = GetUserIdFromHeader();

        var log = new CustomLog
        {
            UserId = userId,
            ActionId = (int)action,
            DescriptionJson = JsonSerializer.SerializeToDocument(descricao),
            CreatedAt = DateTime.UtcNow
        };

        _context.CustomLogs.Add(log);
        await _context.SaveChangesAsync();
    }
}