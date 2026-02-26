using System;

namespace HomePlanner.Repository.IRepository;

public interface ITaskRepository
{
    int GetTotalTasks();

    ICollection<Task> GetTasks(int pageNumber, int pageSize);

    ICollection<Task> GetTasksByWeek(DateTime date);

    Task? GetTaskById(int id);

    bool CreateTask(Task task);

    bool UpdateTask(Task task);

    bool DeleteTask(Task task);

    bool Save();
}