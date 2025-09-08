using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyTodo.Data;
using MyTodo.Models;
using MyTodo.Models.Enum; // Importe nosso enum!
using MyTodo.Services;
using MyTodo.ViewsModels;
using Serilog.Context; // Essencial para usar o LogContext

namespace MyTodo.Controllers;

[ApiController]
[Route("v1")]
public class TodoController : ControllerBase
{
    // GET: /v1/todos
    [HttpGet("todos")]
    public async Task<IActionResult> GetAsync([FromServices] AppDbContext context)
    {
        var todos = await context.Todos.AsNoTracking().ToListAsync();
        return Ok(todos);
    }

    // GET: /v1/todos/{id}
    [HttpGet("todos/{id}")]
    public async Task<IActionResult> GetByIdAsync(
        [FromServices] AppDbContext context,
        [FromServices] ILogger<TodoController> logger,
        [FromServices] UserContext userContext,
        [FromRoute] int id)
    {
        var todo = await context
            .Todos
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        // Usando o LogContext para adicionar a ActionId
        using (LogContext.PushProperty("ActionId", (int)LogAction.TodoRetrieved))
        {
            if (todo == null)
            {
                logger.LogWarning("Todo com Id {TodoId} não encontrado.", id);
                return NotFound();
            }
            
            logger.LogInformation("Todo {TodoId} retornado com sucesso.", id);
            return Ok(todo);
        }
    }

    // POST: /v1/todos
    [HttpPost("todos")]
    public async Task<IActionResult> PostAsync(
        [FromServices] AppDbContext context,
        [FromServices] ILogger<TodoController> logger,
        [FromBody] CreateTodoViewModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        var todo = new Todo
        {
            Date = DateTime.UtcNow, // Use UtcNow para consistência em servidores
            Done = false,
            Title = model.Title
        };

        try
        {
            await context.Todos.AddAsync(todo);
            await context.SaveChangesAsync();

            using (LogContext.PushProperty("ActionId", (int)LogAction.TodoCreated))
            {
                logger.LogInformation("Novo Todo '{TodoTitle}' (Id: {TodoId}) foi criado.", todo.Title, todo.Id);
            }

            return Created($"v1/todos/{todo.Id}", todo);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Falha ao criar um novo Todo com título {TodoTitle}", model.Title);
            return BadRequest();
        }
    }

    // PUT: /v1/todos/{id}
    [HttpPut("todos/{id}")]
    public async Task<IActionResult> PutAsync(
        [FromServices] AppDbContext context,
        [FromServices] ILogger<TodoController> logger,
        [FromBody] CreateTodoViewModel model,
        [FromRoute] int id)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        var todo = await context.Todos.FirstOrDefaultAsync(x => x.Id == id);

        if (todo == null)
            return NotFound();

        try
        {
            todo.Title = model.Title;
            context.Todos.Update(todo);
            await context.SaveChangesAsync();

            using (LogContext.PushProperty("ActionId", (int)LogAction.TodoUpdated))
            {
                logger.LogInformation("O Todo '{TodoTitle}' (Id: {TodoId}) foi atualizado.", todo.Title, todo.Id);
            }

            return Ok(todo);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Falha ao atualizar o Todo {TodoId}", id);
            return BadRequest();
        }
    }

    // DELETE: /v1/todos/{id}
    [HttpDelete("todos/{id}")]
    public async Task<IActionResult> DeleteAsync(
        [FromServices] AppDbContext context,
        [FromServices] ILogger<TodoController> logger,
        [FromRoute] int id)
    {
        var todo = await context
            .Todos
            .FirstOrDefaultAsync(x => x.Id == id);

        if (todo == null)
        {
            using (LogContext.PushProperty("ActionId", (int)LogAction.TodoDeleted))
            {
                logger.LogWarning("Tentativa de exclusão de um Todo não encontrado. Id: {TodoId}", id);
            }
            return NotFound();
        }

        try
        {
            context.Todos.Remove(todo);
            await context.SaveChangesAsync();

            using (LogContext.PushProperty("ActionId", (int)LogAction.TodoDeleted))
            {
                logger.LogInformation("O Todo '{TodoTitle}' (Id: {TodoId}) foi excluído.", todo.Title, todo.Id);
            }

            return Ok(new { message = $"Todo '{todo.Title}' excluído com sucesso." });
        }
        catch (Exception e)
        {
            using (LogContext.PushProperty("ActionId", (int)LogAction.TodoDeleted))
            {
                logger.LogError(e, "Falha ao excluir o Todo {TodoId}", id);
            }
            return BadRequest(new { message = "Não foi possível excluir o todo." });
        }
    }
}