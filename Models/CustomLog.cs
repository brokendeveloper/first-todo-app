using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace MyTodo.Models;


public class CustomLog
{
   
    public int Id { get; set; }

    
    public int? UserId { get; set; }

   
    public int ActionId { get; set; }
    
   
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

   
    public JsonDocument DescriptionJson { get; set; }
}