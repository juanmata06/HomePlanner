using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using AutoMapper;
using HomePlanner.Repository.IRepository;
using HomePlanner.Shared.Constants;

namespace HomePlanner.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UsersController(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
        public IActionResult GetUsers()
        {
            var items = _userRepository.GetUsers();
            var itemsDto = _mapper.Map<List<UserDto>>(items);
            return Ok(itemsDto);
        }

        [HttpGet("{id}", Name = "GetUserById")]
        [ResponseCache(CacheProfileName = CacheProfiles.Default10)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        public IActionResult GetUserById(string id)
        {
            var item = _userRepository.GetUserById(id);
            if (item == null)
            {
                return NotFound($"No user {id} found");
            }
            var itemDto = _mapper.Map<UserDto>(item);
            return Ok(itemDto);
        }

        // [AllowAnonymous]
        // [HttpPost("Register", Name = "RegisterUser")]
        // [ProducesResponseType(StatusCodes.Status403Forbidden)]
        // [ProducesResponseType(StatusCodes.Status400BadRequest)]
        // [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
        // [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        // public async Task<IActionResult> RegisterUser([FromBody] CreateUserDto createUserDto)
        // {
        //     if (createUserDto == null || !ModelState.IsValid)
        //     {
        //         return BadRequest(ModelState);
        //     }
        //     if (string.IsNullOrWhiteSpace(createUserDto.Username))
        //     {
        //         return BadRequest("Username is required");

        //     }
        //     if (_userRepository.UserExistsByUserName(createUserDto.Username))
        //     {
        //         ModelState.AddModelError(Constants.Constants.CustomErrorKey, $"User {createUserDto.Username} already exists.");
        //         return BadRequest(ModelState);
        //     }
        //     var result = await _userRepository.Register(createUserDto);
        //     if (result == null)
        //     {
        //         return StatusCode(StatusCodes.Status500InternalServerError, "Error registering user");
        //     }
        //     return CreatedAtRoute("GetUserById", new { id = result.Id }, createUserDto);
        // }

        // [AllowAnonymous]
        // [HttpPost("Login", Name = "LoginUser")]
        // [ProducesResponseType(StatusCodes.Status403Forbidden)]
        // [ProducesResponseType(StatusCodes.Status400BadRequest)]
        // [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        // [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        // public async Task<IActionResult> LoginUser([FromBody] UserLoginDto userLoginDto)
        // {
        //     if (userLoginDto == null || !ModelState.IsValid)
        //     {
        //         return BadRequest(ModelState);
        //     }
        //     var user = await _userRepository.Login(userLoginDto);
        //     if (user == null)
        //     {
        //         return Unauthorized();
        //     }
        //     return Ok(user);
        // }
    }
}
