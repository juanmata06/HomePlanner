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
    [Authorize(Roles = "Admin")]
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
        [ProducesResponseType(typeof(List<UserGetDto>), StatusCodes.Status200OK)]
        public IActionResult GetUsers()
        {
            var items = _userRepository.GetUsers();
            var itemsDto = _mapper.Map<List<UserGetDto>>(items);
            return Ok(itemsDto);
        }

        [HttpGet("{id}", Name = "GetUserById")]
        [ResponseCache(CacheProfileName = CacheProfiles.Default10)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(UserGetDto), StatusCodes.Status200OK)]
        public IActionResult GetUserById(string id)
        {
            var item = _userRepository.GetUserById(id);
            if (item == null)
            {
                return NotFound($"No user {id} found");
            }
            var itemDto = _mapper.Map<UserGetDto>(item);
            return Ok(itemDto);
        }

        [AllowAnonymous]
        [HttpPost("Register", Name = "RegisterUser")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(UserRegisterResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RegisterUser([FromBody] CreateUserDto createUserDto)
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

        [AllowAnonymous]
        [HttpPost("Login", Name = "LoginUser")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> LoginUser([FromBody] UserLoginDto userLoginDto)
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

        // TODO: use UserGetDto for this endpoint
        [HttpPut("{id}", Name = "UpdateUser")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] CreateUserDto updateUserDto)
        {
            if (updateUserDto == null || string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("User ID and data are required");
            }

            var user = _userRepository.GetUserById(id);
            if (user == null)
            {
                return NotFound($"User {id} not found");
            }

            _mapper.Map(updateUserDto, user);
            
            if (!_userRepository.UpdateUser(user))
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating user");
            }

            if (!await _userRepository.SaveAsync())
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error saving changes");
            }

            var userDto = _mapper.Map<UserDto>(user);
            return Ok(userDto);
        }

        [HttpDelete("{id}", Name = "DeleteUser")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("User ID is required");
            }

            var user = _userRepository.GetUserById(id);
            if (user == null)
            {
                return NotFound($"User {id} not found");
            }

            if (!_userRepository.DeleteUser(user))
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting user");
            }

            if (!await _userRepository.SaveAsync())
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error saving changes");
            }

            return NoContent();
        }
    }
}
