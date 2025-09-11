using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyTodo.Data;
using MyTodo.Models;
using MyTodo.Models.Enum;
using MyTodo.Services; // para ICustomLogger
using MyTodo.ViewsModels;

namespace MyTodo.Controllers;

[ApiController]
[Route("v1")]
public class TodoController : ControllerBase
{
    // GET: /v1/todos
    [HttpGet("todos")]
    public async Task<IActionResult> GetAsync(
        [FromServices] AppDbContext context,
        [FromServices] ICustomLogger logger,
        [FromServices] UserContext userContext)
    {
        if (userContext.UserId == null)
            return Unauthorized("Header X-User-Id é obrigatório.");

        var todos = await context
            .Todos
            .AsNoTracking()
            .Where(x => x.UserId == userContext.UserId)
            .ToListAsync();

        await logger.LogAsync(LogAction.AllTodosRetrivied, 
            new { Message = $"Todos retornados para usuário {userContext.UserId}. Total: {todos.Count}" });

        return Ok(todos);
    }

    // GET: /v1/todos/{id}
    [HttpGet("todos/{id}")]
    public async Task<IActionResult> GetByIdAsync(
        [FromServices] AppDbContext context,
        [FromServices] ICustomLogger logger,
        [FromServices] UserContext userContext,
        [FromRoute] int id)
    {
        if (userContext.UserId == null)
            return Unauthorized("Header X-User-Id é obrigatório.");

        var todo = await context
            .Todos
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userContext.UserId);

        if (todo == null)
        {
            await logger.LogAsync(LogAction.TodoRetrieved,
                new { Message = $"Todo com Id {id} não encontrado para o usuário {userContext.UserId}" });
            return NotFound();
        }
        
        await logger.LogAsync(LogAction.TodoRetrieved,
            new { Message = $"Todo {id} '{todo.Title}' retornado com sucesso" });

        return Ok(todo);
    }

    // POST: /v1/todos
    [HttpPost("todos")]
    public async Task<IActionResult> PostAsync(
        [FromServices] AppDbContext context,
        [FromServices] ICustomLogger logger,
        [FromServices] UserContext userContext,
        [FromBody] CreateTodoViewModel model)
    {
        if (userContext.UserId == null)
            return Unauthorized("Header X-User-Id é obrigatório.");

        if (!ModelState.IsValid)
            return BadRequest();

        var todo = new Todo
        {
            Done = false,
            Title = model.Title,
            UserId = userContext.UserId.Value
        };

        try
        {
            await context.Todos.AddAsync(todo);
            await context.SaveChangesAsync();

            await logger.LogAsync(LogAction.TodoCreated,
                new { TodoTitle = todo.Title, TodoId = todo.Id, OriginalModel = model });

            return Created($"v1/todos/{todo.Id}", todo);
        }
        catch (Exception e)
        {
            // Aqui você pode querer criar um método adicional no logger para erros com exception, ou logar de outra forma
            await logger.LogAsync(LogAction.TodoCreated,
                new { Message = $"Falha ao criar novo Todo '{model.Title}'", Exception = e.Message });
            return BadRequest();
        }
    }

    // PUT: /v1/todos/{id}
    [HttpPut("todos/{id}")]
    public async Task<IActionResult> PutAsync(
        [FromServices] AppDbContext context,
        [FromServices] ICustomLogger logger,
        [FromServices] UserContext userContext,
        [FromBody] CreateTodoViewModel model,
        [FromRoute] int id)
    {
        if (userContext.UserId == null)
            return Unauthorized("Header X-User-Id é obrigatório.");

        if (!ModelState.IsValid)
            return BadRequest();

        var todo = await context.Todos.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userContext.UserId);

        if (todo == null)
            return NotFound();

        try
        {
            var oldTitle = todo.Title;
            todo.Title = model.Title;
            context.Todos.Update(todo);
            await context.SaveChangesAsync();

            await logger.LogAsync(LogAction.TodoUpdated,
                new { TodoId = id, OldTitle = oldTitle, NewTitle = model.Title });

            return Ok(todo);
        }
        catch (Exception e)
        {
            await logger.LogAsync(LogAction.TodoUpdated,
                new { Message = $"Falha ao atualizar Todo {id}", Exception = e.Message });
            return BadRequest();
        }
    }

    // DELETE: /v1/todos/{id}
    [HttpDelete("todos/{id}")]
    public async Task<IActionResult> DeleteAsync(
        [FromServices] AppDbContext context,
        [FromServices] ICustomLogger logger,
        [FromServices] UserContext userContext,
        [FromRoute] int id)
    {
        if (userContext.UserId == null)
            return Unauthorized("Header X-User-Id é obrigatório.");

        var todo = await context
            .Todos
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userContext.UserId);

        if (todo == null)
        {
            await logger.LogAsync(LogAction.TodoDeleted,
                new { Message = $"Tentativa de exclusão de Todo inexistente {id}" });
            return NotFound();
        }

        try
        {
            context.Todos.Remove(todo);
            await context.SaveChangesAsync();

            await logger.LogAsync(LogAction.TodoDeleted,
                new { TodoTitle = todo.Title, TodoId = todo.Id });

            return Ok(new { message = $"Todo '{todo.Title}' excluído com sucesso." });
        }
        catch (Exception e)
        {
            await logger.LogAsync(LogAction.TodoDeleted,
                new { Message = $"Falha ao excluir Todo {id}", Exception = e.Message, TodoTitle = todo.Title });
            return BadRequest(new { message = "Não foi possível excluir o todo." });
        }
    }
}
