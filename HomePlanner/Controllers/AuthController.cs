using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

using AutoMapper;
using HomePlanner.Repository.IRepository;

namespace HomePlanner.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public AuthController(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        [HttpPost("register", Name = "RegisterUser")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(UserRegisterResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] CreateUserDto createUserDto)
        {
            if (createUserDto == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (string.IsNullOrWhiteSpace(createUserDto.Email))
            {
                return BadRequest("Email is required");
            }
            if (_userRepository.UserExistsByEmail(createUserDto.Email))
            {
                ModelState.AddModelError(Constants.CustomErrorKey, $"User {createUserDto.Email} already exists.");
                return BadRequest(ModelState);
            }
            var (user, errors) = await _userRepository.Register(createUserDto);
            if (errors != null && errors.Count > 0)
            {
                foreach (var error in errors)
                {
                    ModelState.AddModelError(Constants.CustomErrorKey, error);
                }
                return BadRequest(ModelState);
            }
            if (user == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error registering user");
            }
            var userDto = _mapper.Map<UserRegisterResponseDto>(user);
            return CreatedAtRoute("GetUserById", new { id = user.Id }, userDto);
        }

        [HttpPost("login", Name = "LoginUser")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] UserLoginDto userLoginDto)
        {
            if (userLoginDto == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await _userRepository.Login(userLoginDto);
            if (user == null)
            {
                return Unauthorized();
            }
            return Ok(user);
        }

        [HttpGet("profile", Name = "GetUserByToken")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(UserLoginResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserByToken()
        {
            var userId = User.FindFirst("id")?.Value;
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            var user = _userRepository.GetUserById(userId);
            if (user == null)
            {
                return NotFound($"User {userId} not found");
            }

            var newToken = await _userRepository.GenerateTokenAsync(user);
            var userDataDto = _mapper.Map<UserDataDto>(user);

            return Ok(new UserLoginResponseDto()
            {
                Token = newToken,
                User = userDataDto,
                Message = "Token refreshed successfully!"
            });
        }
    }
}
