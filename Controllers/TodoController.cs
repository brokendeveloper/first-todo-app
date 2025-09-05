using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyTodo.Data;
using MyTodo.Models;
using MyTodo.Models.Enum;
using MyTodo.Services;
using MyTodo.ViewsModels;
using Serilog.Context;

namespace MyTodo.Controllers;

[ApiController]
[Route("v1")]
public class TodoController : ControllerBase
{
    [HttpGet]
    [Route("todos")]
    public async Task<IActionResult> GetAsync(
        [FromServices] AppDbContext context)
    {
        var todos = await context
            .Todos
            .AsNoTracking()
            .ToListAsync();
        return Ok(todos);

    }

    [HttpGet]
    [Route("todos/{id}")]
    public async Task<IActionResult> GetByIdAsync(
        [FromServices] AppDbContext context,
        [FromRoute] int id)
    {
        var todo = await context
            .Todos
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        return todo == null
            ? NotFound()
            : Ok(todo);

    }

    [HttpPost("todos")]
    public async Task<IActionResult> PostAsync(
        [FromServices] AppDbContext context,
        [FromBody] CreateTodoViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        var todo = new Todo
        {
            Date = DateTime.Now,
            Done = false,
            Title = model.Title
        };

        try
        {
            await context.Todos.AddAsync(todo);
            await context.SaveChangesAsync();
            return Created($"v1/todos/{todo.Id}", todo);
        }
        catch (Exception e)
        {
            return BadRequest();
        }
    }

    [HttpPut("todos/{id}")]
    public async Task<IActionResult> PutAsync(
            [FromServices] AppDbContext context,
            [FromBody] CreateTodoViewModel model,
            [FromRoute] int id)
        // por preguiça está com create, mas deveria ter um viewModel para isso
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        var todo = await context
            .Todos
            .FirstOrDefaultAsync(x => x.Id == id);

        if (todo == null)
        {
            return NotFound();
        }

        try
        {
            todo.Title = model.Title;

            context.Todos.Update(todo);
            await context.SaveChangesAsync();
            return Ok(todo);
        }
        catch (Exception e)
        {
            return BadRequest();
        }


    }

    [HttpDelete("todos/{id}")]
    public async Task<IActionResult> DeleteAsync(
        [FromServices] AppDbContext context,
        [FromServices] ILogger<TodoController> logger,
        [FromServices] UserContext userContext,
        [FromRoute] int id)
    {
        var todo = await context
            .Todos
            .FirstOrDefaultAsync(x => x.Id == id);

        if (todo == null)
        {
            using (LogContext.PushProperty("ActionId", (int)LogAction.TodoDeleted))
            {
                logger.LogWarning("Try of exclusion of one not found Todo. Id: {TodoId}", id);
            }
            return NotFound();
        }
        try
        {
            context.Todos.Remove(todo);
            await context.SaveChangesAsync(); 
            // deveria ter uma mensagem aqui
            using (LogContext.PushProperty("ActionId", (int)LogAction.TodoDeleted))
            {
                logger.LogInformation("The Todo '{TodoTitle}' (Id: {TodoId} has been excluded", todo.Title, todo.Id);
            }
            
            
            return Ok(new {message = $"Todo '{todo.Title}' excluded sucessfully."});
        }
        catch (Exception e)
        {
            using (LogContext.PushProperty("ActionId", (int)LogAction.TodoDeleted))
            {
                logger.LogError(e, "Fail in deletion of Todo {TodoId}", id);
            }

            return BadRequest(new { message = "Its not possible delete the Todo." });
        }
    }
}