using System;

using AutoMapper;

using HomePlanner.Models.Dtos.Task;

namespace HomePlanner.Mapping;

public class TaskProfile : Profile
{
    public TaskProfile()
    {
        CreateMap<Task, TaskDto>().ReverseMap();
        CreateMap<Task, CreateTaskDto>().ReverseMap();
        CreateMap<Task, UpdateTaskDto>().ReverseMap();
    }
}
