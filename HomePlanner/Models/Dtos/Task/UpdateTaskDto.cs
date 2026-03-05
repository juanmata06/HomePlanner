using System;

namespace HomePlanner.Models.Dtos.Task;

public class UpdateTaskDto
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public TaskStatus? Status { get; set; }

    public DateTime? LastCompletedDate { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? AssignedToId { get; set; }
}
