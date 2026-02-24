

namespace HomePlanner.Repository.IRepository;

public interface IUserRepository
{
    ICollection<ApplicationUser> GetUsers();
    ApplicationUser? GetUserById(string id);
    bool UserExistsByEmail(string email);
    Task<UserLoginResponseDto> Login(UserLoginDto userLoginDto);
    Task<(UserDataDto? User, List<string>? Errors)> Register(CreateUserDto createUserDto);
    Task<string> GenerateTokenAsync(ApplicationUser user);
    bool UpdateUser(ApplicationUser user);
    bool DeleteUser(ApplicationUser user);
    Task<bool> SaveAsync();
}
