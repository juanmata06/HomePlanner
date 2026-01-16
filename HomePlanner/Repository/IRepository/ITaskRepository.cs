using System;

namespace HomePlanner.Repository.IRepository;

public interface ITaskRepository
{
    int GetTotalTasks();

    ICollection<Task> GetTasks(int pageNumber, int pageSize);

    Task? GetTaskById(int id);

    bool CreateTask(Task task);

    bool Save();
}