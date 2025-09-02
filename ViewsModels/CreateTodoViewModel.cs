using System.ComponentModel.DataAnnotations;

namespace MyTodo.ViewsModels;

public class CreateTodoViewModel
{
    [Required]
    public string Title { get; set; }
}  