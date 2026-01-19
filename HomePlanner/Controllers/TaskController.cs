using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using AutoMapper;

using HomePlanner.Repository.IRepository;

using HomePlanner.Models.Dtos.Task;
using HomePlanner.Models.Responses;
using HomePlanner.Shared.Constants;

namespace HomePlanner.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class TaskController : ControllerBase
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public TaskController(ITaskRepository taskRepository, IUserRepository userRepository, IMapper mapper)
        {
            _taskRepository = taskRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(List<TaskDto>), StatusCodes.Status200OK)]
        public IActionResult GetTasks([FromQuery] int page = 1, [FromQuery] int size = 5)
        {
            if (page < 1 || size <= 0)
            {
                return BadRequest("Pagination params aren't valids.");
            }
            var totalItems = _taskRepository.GetTotalTasks();
            var totalPages = (int)Math.Ceiling((double)totalItems / size);
            if (page > totalPages)
            {
                return NotFound("Page not found");
            }
            var items = _taskRepository.GetTasks(page, size);
            var itemsDto = _mapper.Map<List<TaskDto>>(items);
            
            // Populate CreatedBy and AssignedTo for each task
            foreach (var taskDto in itemsDto)
            {
                var createdByUser = _userRepository.GetUserById(taskDto.CreatedById);
                var assignedToUser = _userRepository.GetUserById(taskDto.AssignedToId);
                
                taskDto.CreatedBy = _mapper.Map<UserDto>(createdByUser);
                taskDto.AssignedTo = _mapper.Map<UserDto>(assignedToUser);
            }
            
            var response = new PaginationResponse<TaskDto>
            {
                Page = page,
                Size = size,
                TotalPages = totalPages,
                Items = itemsDto
            };
            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult CreateTask([FromForm] CreateTaskDto createTaskDto)
        {
            if (createTaskDto == null)
            {
                return BadRequest(ModelState);
            }

            var createdById = _userRepository.GetUserById(createTaskDto.CreatedById);
            if (createdById == null)
            {
                ModelState.AddModelError(Constants.CustomErrorKey, $"User {createTaskDto.CreatedById} doesn't exists.");
                return BadRequest(ModelState);
            }

            var assignedToId = _userRepository.GetUserById(createTaskDto.AssignedToId);
            if (assignedToId == null)
            {
                ModelState.AddModelError(Constants.CustomErrorKey, $"User {createTaskDto.AssignedToId} doesn't exists.");
                return BadRequest(ModelState);
            }

            var task = _mapper.Map<Task>(createTaskDto);

            if (!_taskRepository.CreateTask(task))
            {
                ModelState.AddModelError(Constants.CustomErrorKey, "Something went wrong while creating the task.");
                return StatusCode(500, ModelState);
            }

            var createdTask = _taskRepository.GetTaskById(task.Id);
            var taskDto = _mapper.Map<TaskDto>(createdTask);
            return CreatedAtRoute("GetTaskById", new { id = task.Id }, taskDto);
        }

        [AllowAnonymous]
        [HttpGet("{id:int}", Name = "GetTaskById")]
        [ResponseCache(CacheProfileName = CacheProfiles.Default10)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
        public IActionResult GetTaskById(int id)
        {
            var item = _taskRepository.GetTaskById(id);
            if (item == null)
            {
                return NotFound($"No task {id} found");
            }
            var itemDto = _mapper.Map<TaskDto>(item);
            
            // Populate CreatedBy and AssignedTo
            var createdByUser = _userRepository.GetUserById(itemDto.CreatedById);
            var assignedToUser = _userRepository.GetUserById(itemDto.AssignedToId);
            
            itemDto.CreatedBy = _mapper.Map<UserDto>(createdByUser);
            itemDto.AssignedTo = _mapper.Map<UserDto>(assignedToUser);
            
            return Ok(itemDto);
        }
    }
}
