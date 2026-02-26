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
            .OrderBy(item => item.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public ICollection<Task> GetTasksByWeek(DateTime date)
    {
        // Calculate Sunday of the week
        int daysToSunday = (int)date.DayOfWeek;
        var sunday = date.AddDays(-daysToSunday).Date;
        var saturday = sunday.AddDays(6); // Start of Saturday (to exclude it and keep up to Friday)
        
        return _db.Tasks
            .Where(item => item.CreatedAt >= sunday && item.CreatedAt < saturday)
            .OrderBy(item => item.CreatedAt)
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
    public bool UpdateTask(Task task)
    {
        if (task == null)
        {
            return false;
        }
        _db.Tasks.Update(task);
        return Save();
    }

    public bool DeleteTask(Task task)
    {
        if (task == null)
        {
            return false;
        }
        _db.Tasks.Remove(task);
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
