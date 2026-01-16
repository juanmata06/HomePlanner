using System;
using HomePlanner.Repository.IRepository;

public class TaskRepository : ITaskRepository
{
    private readonly ApplicationDbContext _db;

    public TaskRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public int GetTotalTasks()
    {
        return _db.Tasks.Count();
    }

    public ICollection<Task> GetTasks(int pageNumber, int pageSize)
    {
        return _db.Tasks
            .OrderBy(item => item.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public bool CreateTask(Task task)
    {
        if (task == null)
        {
            return false;
        }
        task.CreatedAt = DateTime.Now;
        _db.Tasks.Add(task);
        return Save();
    }

    public Task? GetTaskById(int id)
    {

        return _db.Tasks.FirstOrDefault(p => p.Id == id);
    }

    public bool Save()
    {
        return _db.SaveChanges() >= 0;
    }
}
