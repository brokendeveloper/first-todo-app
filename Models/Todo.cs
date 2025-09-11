using System.ComponentModel.DataAnnotations.Schema;

namespace MyTodo.Models;


public class Todo
{
    
    public int  Id { get; set; }
    
    
    public string Title { get; set; }
   
    
    public bool Done { get; set; }
    
    
    public DateTime DueDate { get; set; }
    
    
    public int UserId { get; set; }
    
}