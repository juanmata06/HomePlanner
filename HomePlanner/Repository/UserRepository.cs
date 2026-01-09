using System;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using AutoMapper;

using HomePlanner.Repository.IRepository;

//TODO: expandir los campos de la clase user, usando identify solo puedo usar los propios de la libreria.
public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly string? secretKey;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IMapper _mapper;

    public UserRepository(
        ApplicationDbContext db,
        IConfiguration configuration,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IMapper mapper
    )
    {
        _db = db;
        _configuration = configuration;
        secretKey = configuration.GetValue<string>("ApiSettings:SecretKey");
        _userManager = userManager;
        _roleManager = roleManager;
        _mapper = mapper;
    }

    public ApplicationUser? GetUserById(string id)
    {
        return _db.ApplicationUsers.FirstOrDefault(u => u.Id == id);
    }

    public ICollection<ApplicationUser> GetUsers()
    {
        return _db.ApplicationUsers.OrderBy(u => u.UserName).ToList();
    }

    public bool UserExistsByEmail(string email)
    {
        return _db.Users.Any(u => u.Email.ToLower().Trim() == email.ToLower().Trim());
    }

    public async Task<UserLoginResponseDto> Login(UserLoginDto userLoginDto)
    {
        if (string.IsNullOrEmpty(userLoginDto.Email))
        {
            throw new InvalidOperationException("Email is required");
        }

        var user = await _db.ApplicationUsers.FirstOrDefaultAsync<ApplicationUser>(
            u => u.Email != null && u.Email.ToLower().Trim() == userLoginDto.Email.ToLower().Trim()
        );
        if (user == null)
        {
            throw new InvalidOperationException("Email not found");
        }

        if (userLoginDto.Password == null)
        {
            throw new InvalidOperationException("Password is required");
        }

        bool isValid = await _userManager.CheckPasswordAsync(user, userLoginDto.Password);
        if (!isValid)
        {
            throw new InvalidOperationException("Credentials are not correct");
        }

        var handlerToken = new JwtSecurityTokenHandler();
        if (string.IsNullOrWhiteSpace(secretKey))
        {
            throw new InvalidOperationException("SecretKey has not been configured");
        }
        var roles = await _userManager.GetRolesAsync(user);
        var key = Encoding.UTF8.GetBytes(secretKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("id", user.Id.ToString()),
                new Claim("email", user.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, roles.FirstOrDefault() ?? string.Empty),
            }
            ),
            Expires = DateTime.UtcNow.AddHours(24),
            Issuer = _configuration["ApiSettings:Issuer"],
            Audience = _configuration["ApiSettings:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = handlerToken.CreateToken(tokenDescriptor);

        var userDataDto = _mapper.Map<UserDataDto>(user);
        userDataDto.Role = roles.FirstOrDefault();

        return new UserLoginResponseDto()
        {
            Token = handlerToken.WriteToken(token),
            User = userDataDto,
            Message = "User logged succesfully!"
        };
    }

    // TODO: add error message for bad password
    public async Task<UserDataDto> Register(CreateUserDto createUserDto)
    {
        if (string.IsNullOrEmpty(createUserDto.Email))
        {
            throw new InvalidOperationException("Username is required");
        }

        if (createUserDto.Password == null)
        {
            throw new InvalidOperationException("Password is required");
        }

        var user = new ApplicationUser()
        {
            UserName = createUserDto.Email,
            Email = createUserDto.Email,
            NormalizedEmail = createUserDto.Email.ToUpper(),
            Name = createUserDto.Name,
        };
        var result = await _userManager.CreateAsync(user, createUserDto.Password);

        if (result.Succeeded)
        {
            var userRole = createUserDto.Role ?? "User";
            var roleExists = await _roleManager.RoleExistsAsync(userRole);
            if (!roleExists)
            {
                var identityRole = new IdentityRole(userRole);
                await _roleManager.CreateAsync(identityRole);
            }
            await _userManager.AddToRoleAsync(user, userRole);
            var createdUser = await _db.ApplicationUsers.FirstOrDefaultAsync(u => u.Email == createUserDto.Email);
            var userDataDto = _mapper.Map<UserDataDto>(createdUser);
            userDataDto.Role = userRole;
            return userDataDto;
        }

        throw new InvalidOperationException("An error has occurred during user registration");
    }

    public async Task<bool> SaveAsync()
    {
        return await _db.SaveChangesAsync() >= 0;
    }
}
