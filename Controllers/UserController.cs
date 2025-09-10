using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyTodo.Data;
using MyTodo.Models;
using MyTodo.Extensions;

namespace MyTodo.Controllers;

[ApiController]
[Route("v1")]
public class UserController : ControllerBase
{
    // POST: /v1/users
    [HttpPost("users")]
    public async Task<IActionResult> PostAsync(
        [FromServices] AppDbContext context,
        [FromServices] ILogger<UserController> logger,
        [FromBody] User model)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        try
        {
            await context.Users.AddAsync(model);
            await context.SaveChangesAsync();
            
            // ✅ Log de criação de usuário (se quiser auditar)
            logger.LogInformation("Usuário {UserName} criado com Id {UserId}", 
                model.Name, model.Id);
            
            return Created($"v1/users/{model.Id}", model);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Falha ao criar usuário {UserName}", model.Name);
            return BadRequest();
        }
    }

    // GET: /v1/users
    [HttpGet("users")]
    public async Task<IActionResult> GetAsync(
        [FromServices] AppDbContext context,
        [FromServices] ILogger<UserController> logger)
    {
        var users = await context.Users.AsNoTracking().ToListAsync();
        
        logger.LogInformation("Lista de usuários retornada. Total: {UserCount}", users.Count);
        
        return Ok(users);
    }
}