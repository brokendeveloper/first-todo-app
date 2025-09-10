using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyTodo.Data;
using MyTodo.Models;
using MyTodo.Models.Enum;
using MyTodo.Services;
using MyTodo.ViewsModels;
using MyTodo.Extensions; // ✅ Para usar as extensões

namespace MyTodo.Controllers;

[ApiController]
[Route("v1")]
public class TodoController : ControllerBase
{
    // GET: /v1/todos - Retorna todos os Todos do usuário logado
    [HttpGet("todos")]
    public async Task<IActionResult> GetAsync(
        [FromServices] AppDbContext context,
        [FromServices] ILogger<TodoController> logger,
        [FromServices] UserContext userContext)
    {
        if (userContext.UserId == null)
            return Unauthorized("Header X-User-Id é obrigatório.");

        var todos = await context
            .Todos
            .AsNoTracking()
            .Where(x => x.UserId == userContext.UserId)
            .ToListAsync();

        // ✅ Log simplificado
        logger.LogAudit(LogAction.AllTodosRetrivied, 
            "Todos retornados para usuário {UserId}. Total: {TodoCount}", 
            new { TodoCount = todos.Count });
            
        return Ok(todos);
    }

    // GET: /v1/todos/{id} - Retorna um Todo específico do usuário logado
    [HttpGet("todos/{id}")]
    public async Task<IActionResult> GetByIdAsync(
        [FromServices] AppDbContext context,
        [FromServices] ILogger<TodoController> logger,
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
            // ✅ Log de warning para tentativa de acesso a todo inexistente
            logger.LogAuditWarning(LogAction.TodoRetrieved,
                "Todo com Id {TodoId} não encontrado para o usuário", 
                new { TodoId = id });
            return NotFound();
        }
        
        // ✅ Log de sucesso
        logger.LogAudit(LogAction.TodoRetrieved,
            "Todo {TodoId} '{TodoTitle}' retornado com sucesso", 
            new { TodoId = id, TodoTitle = todo.Title });
            
        return Ok(todo);
    }

    // POST: /v1/todos - Cria um novo Todo para o usuário logado
    [HttpPost("todos")]
    public async Task<IActionResult> PostAsync(
        [FromServices] AppDbContext context,
        [FromServices] ILogger<TodoController> logger,
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
            
            // ✅ Log simplificado
            logger.LogAudit(LogAction.TodoCreated,
                "Novo Todo '{TodoTitle}' criado com Id {TodoId}",
                new { TodoTitle = todo.Title, TodoId = todo.Id, OriginalModel = model });

            return Created($"v1/todos/{todo.Id}", todo);
        }
        catch (Exception e)
        {
            // ✅ Log de erro
            logger.LogAuditError(LogAction.TodoCreated,
                "Falha ao criar novo Todo '{TodoTitle}'", e,
                new { TodoTitle = model.Title });
            return BadRequest();
        }
    }
    
    // PUT: /v1/todos/{id} - Atualiza um Todo do usuário logado
    [HttpPut("todos/{id}")]
    public async Task<IActionResult> PutAsync(
        [FromServices] AppDbContext context,
        [FromServices] ILogger<TodoController> logger,
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
            var oldTitle = todo.Title; // Capturar valor antigo
            todo.Title = model.Title;
            context.Todos.Update(todo);
            await context.SaveChangesAsync();

            // ✅ Log com dados antigos e novos
            logger.LogAudit(LogAction.TodoUpdated,
                "Todo {TodoId} atualizado com sucesso",
                new { 
                    TodoId = id,
                    OldTitle = oldTitle,
                    NewTitle = model.Title 
                });

            return Ok(todo);
        }
        catch (Exception e)
        {
            logger.LogAuditError(LogAction.TodoUpdated,
                "Falha ao atualizar Todo {TodoId}", e,
                new { TodoId = id });
            return BadRequest();
        }
    }

    // DELETE: /v1/todos/{id} - Deleta um Todo do usuário logado
    [HttpDelete("todos/{id}")]
    public async Task<IActionResult> DeleteAsync(
        [FromServices] AppDbContext context,
        [FromServices] ILogger<TodoController> logger,
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
            logger.LogAuditWarning(LogAction.TodoDeleted,
                "Tentativa de exclusão de Todo inexistente {TodoId}",
                new { TodoId = id });
            return NotFound();
        }

        try
        {
            context.Todos.Remove(todo);
            await context.SaveChangesAsync();
            
            // ✅ Log de sucesso na exclusão
            logger.LogAudit(LogAction.TodoDeleted,
                "Todo '{TodoTitle}' (Id: {TodoId}) excluído com sucesso",
                new { TodoTitle = todo.Title, TodoId = todo.Id });
            
            return Ok(new { message = $"Todo '{todo.Title}' excluído com sucesso." });
        }
        catch (Exception e)
        {
            logger.LogAuditError(LogAction.TodoDeleted,
                "Falha ao excluir Todo {TodoId}", e,
                new { TodoId = id, TodoTitle = todo.Title });
            return BadRequest(new { message = "Não foi possível excluir o todo." });
        }
    }

    // ✅ Endpoint de teste com logs estruturados
    [HttpPost("test-log")]
    public IActionResult TestLog([FromServices] ILogger<TodoController> logger)
    {
        // Teste com diferentes tipos de log estruturado
        logger.LogAudit(LogAction.TodoCreated, "Teste de criação", 
            new { TodoTitle = "Todo de Teste", TodoId = 999 });
        
        logger.LogAuditWarning(LogAction.TodoUpdated, "Teste de warning",
            new { TodoId = 888 });
        
        logger.LogAuditError(LogAction.TodoDeleted, "Teste de erro", 
            new Exception("Erro de teste"),
            new { TodoId = 777, TodoTitle = "Todo com Erro" });

        return Ok("Logs estruturados de teste enviados");
    }
}