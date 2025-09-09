using System.ComponentModel.DataAnnotations.Schema;

namespace MyTodo.Models;

[Table("users")]
public class User
{
    [Column("id")]
    public int Id { get; set; }
    
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    public ICollection<Todo> Todos { get; set; } = new List<Todo>();
}