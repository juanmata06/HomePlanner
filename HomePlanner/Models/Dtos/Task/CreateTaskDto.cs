using System;

namespace HomePlanner.Models.Dtos.Task;

public class CreateTaskDto
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TaskStatus Status { get; set; } = TaskStatus.Todo;

    public DateTime? LastCompletedDate { get; set; }

    public DateTime? StartDate { get; set; } = null;

    public DateTime? EndDate { get; set; } = null;

    public string CreatedById { get; set; } = string.Empty;

    public string AssignedToId { get; set; } = string.Empty;
}
