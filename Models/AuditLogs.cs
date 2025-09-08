using System.ComponentModel.DataAnnotations.Schema;

namespace MyTodo.Models;

[Table(("audit_logs"))]
public class AuditLogs
{
    [Column("id")]
    public int Id { get; set; }

    [Column("user_id")]
    public int? UserId { get; set; }

    [Column("action_id")]
    public int ActionId { get; set; }
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("description", TypeName = "jsonb")]
    public String Description { get; set; } = string.Empty;
}