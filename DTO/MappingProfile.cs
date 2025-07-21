// TaskTrackerApi/DTO/MappingProfile.cs

using AutoMapper;
using TaskTrackerApi.Models;
using TaskTrackerApi.DTOs;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<TaskItem, TaskDto>();

        CreateMap<CreateTaskDto, TaskItem>();

        CreateMap<UpdateTaskDto, TaskItem>();

        CreateMap<TaskItem, TaskDto>();

        CreateMap<ApplicationUser, UserDto>();
    }
}
