using AutoMapper;
using HomePlanner.Models;
public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<ApplicationUser, UserDataDto>().ReverseMap();
        CreateMap<ApplicationUser, UserDto>().ReverseMap();
        CreateMap<ApplicationUser, UserGetDto>();
        CreateMap<ApplicationUser, UserRegisterResponseDto>();
        CreateMap<UserDataDto, UserRegisterResponseDto>();
        CreateMap<CreateUserDto, ApplicationUser>();
    }
}
