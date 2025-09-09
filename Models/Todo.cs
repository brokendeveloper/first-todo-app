using System.ComponentModel.DataAnnotations.Schema;

namespace MyTodo.Models;

[Table("todos")]
public class Todo
{
    [Column("id")]
    public int  Id { get; set; }
    
    [Column("title")]
    public string Title { get; set; }
   
    [Column("done")]
    public bool Done { get; set; }
    
    [Column("date")]
    public DateTime Date { get; set; }
    
    [Column("user_id")]
    public int UserId { get; set; }

    public User User { get; set; } = null!;
}