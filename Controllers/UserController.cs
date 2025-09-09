using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyTodo.Data;
using MyTodo.Models;

namespace MyTodo.Controllers;

[ApiController]
[Route("v1")]
public class UserController : ControllerBase
{
    // POST: /v1/users
    [HttpPost("users")]
    public async Task<IActionResult> PostAsync(
        [FromServices] AppDbContext context,
        [FromBody] User model) // Para simplificar, vamos receber a entidade User diretamente
    {
        if (!ModelState.IsValid)
            return BadRequest();

        try
        {
            await context.Users.AddAsync(model);
            await context.SaveChangesAsync();
            return Created($"v1/users/{model.Id}", model);
        }
        catch (Exception e)
        {
            // Em um app real, logaríamos o erro aqui
            return BadRequest();
        }
    }

    // GET: /v1/users
    [HttpGet("users")]
    public async Task<IActionResult> GetAsync([FromServices] AppDbContext context)
    {
        var users = await context.Users.AsNoTracking().ToListAsync();
        return Ok(users);
    }
}