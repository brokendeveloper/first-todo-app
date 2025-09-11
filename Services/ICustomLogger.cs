using MyTodo.Models.Enum;

namespace MyTodo.Services;

public interface ICustomLogger
{
    Task LogAsync(LogAction action, object descricao);
}