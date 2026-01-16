using System;

namespace HomePlanner.Models.Dtos.Task;

public class TaskDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TaskStatus Status { get; set; } = TaskStatus.Todo;

    public DateTime? LastCompletedDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? StartDate { get; set; } = null;

    public DateTime? EndDate { get; set; } = null;

    public string CreatedById { get; set; } = string.Empty;

    public required UserDto CreatedBy { get; set; }

    public string AssignedToId { get; set; } = string.Empty;
    
    public required UserDto AssignedTo { get; set; }
}
