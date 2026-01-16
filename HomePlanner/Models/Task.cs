using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public enum TaskStatus
{
    Todo = 0,
    Doing = 1,
    Done = 2
}

public class Task
{
    [Key]
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TaskStatus Status { get; set; } = TaskStatus.Todo;

    public DateTime? LastCompletedDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [ForeignKey("CreatedBy")]
    public required string CreatedById { get; set; }

    public required ApplicationUser CreatedBy { get; set; }

    [ForeignKey("AssignedTo")]
    public required string AssignedToId { get; set; }
    public required ApplicationUser AssignedTo { get; set; }
}