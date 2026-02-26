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
    [Authorize]
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
        [HttpGet("tasks")]
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
                if (!string.IsNullOrWhiteSpace(taskDto.CreatedById))
                {
                    var createdByUser = _userRepository.GetUserById(taskDto.CreatedById);
                    taskDto.CreatedBy = _mapper.Map<UserDto>(createdByUser);
                }
                
                if (!string.IsNullOrWhiteSpace(taskDto.AssignedToId))
                {
                    var assignedToUser = _userRepository.GetUserById(taskDto.AssignedToId);
                    taskDto.AssignedTo = _mapper.Map<UserDto>(assignedToUser);
                }
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
        [HttpGet("by-week")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(List<TaskDto>), StatusCodes.Status200OK)]
        public IActionResult GetTasksByWeek([FromQuery] DateTime date)
        {
            var items = _taskRepository.GetTasksByWeek(date);
            var itemsDto = _mapper.Map<List<TaskDto>>(items);
            
            // Populate CreatedBy and AssignedTo for each task
            foreach (var taskDto in itemsDto)
            {
                if (!string.IsNullOrWhiteSpace(taskDto.CreatedById))
                {
                    var createdByUser = _userRepository.GetUserById(taskDto.CreatedById);
                    taskDto.CreatedBy = _mapper.Map<UserDto>(createdByUser);
                }
                
                if (!string.IsNullOrWhiteSpace(taskDto.AssignedToId))
                {
                    var assignedToUser = _userRepository.GetUserById(taskDto.AssignedToId);
                    taskDto.AssignedTo = _mapper.Map<UserDto>(assignedToUser);
                }
            }
            
            return Ok(itemsDto);
        }

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

            var userId = User.FindFirst("id")?.Value;
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            var createdByUser = _userRepository.GetUserById(userId);
            if (createdByUser == null)
            {
                return UnprocessableEntity($"User {userId} doesn't exist.");
            }

            // Validate AssignedToId only if provided
            if (!string.IsNullOrWhiteSpace(createTaskDto.AssignedToId))
            {
                var assignedToUser = _userRepository.GetUserById(createTaskDto.AssignedToId);
                if (assignedToUser == null)
                {
                    ModelState.AddModelError(Constants.CustomErrorKey, $"User {createTaskDto.AssignedToId} doesn't exists.");
                    return BadRequest(ModelState);
                }
            }

            var task = _mapper.Map<Task>(createTaskDto);
            task.CreatedById = userId;

            if (!_taskRepository.CreateTask(task))
            {
                ModelState.AddModelError(Constants.CustomErrorKey, "Something went wrong while creating the task.");
                return StatusCode(500, ModelState);
            }

            var createdTask = _taskRepository.GetTaskById(task.Id);
            var taskDto = _mapper.Map<TaskDto>(createdTask);
            
            // Populate CreatedBy and AssignedTo if they exist
            if (createdTask?.CreatedById != null)
            {
                var createdByData = _userRepository.GetUserById(createdTask.CreatedById);
                taskDto.CreatedBy = _mapper.Map<UserDto>(createdByData);
            }
            
            if (createdTask?.AssignedToId != null)
            {
                var assignedToData = _userRepository.GetUserById(createdTask.AssignedToId);
                taskDto.AssignedTo = _mapper.Map<UserDto>(assignedToData);
            }

            return CreatedAtRoute("GetTaskById", new { id = task.Id }, taskDto);
        }

        [HttpPut("{id:int}", Name = "UpdateTask")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult UpdateTask(int id, [FromBody] UpdateTaskDto updateTaskDto)
        {
            if (updateTaskDto == null || id <= 0)
            {
                return BadRequest("Task ID and data are required");
            }

            var task = _taskRepository.GetTaskById(id);
            if (task == null)
            {
                return NotFound($"Task {id} not found");
            }

            var userId = User.FindFirst("id")?.Value;
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            var user = _userRepository.GetUserById(userId);
            if (user == null)
            {
                return UnprocessableEntity($"User {userId} doesn't exist.");
            }

            if (task.CreatedById != userId)
            {
                return Forbid();
            }

            // Validate AssignedToId only if provided
            if (!string.IsNullOrWhiteSpace(updateTaskDto.AssignedToId))
            {
                var assignedToUser = _userRepository.GetUserById(updateTaskDto.AssignedToId);
                if (assignedToUser == null)
                {
                    ModelState.AddModelError(Constants.CustomErrorKey, $"User {updateTaskDto.AssignedToId} doesn't exists.");
                    return BadRequest(ModelState);
                }
            }

            _mapper.Map(updateTaskDto, task);
            // Don't update CreatedById - it should remain as is
            // Allow updating CreatedAt if provided
            if (updateTaskDto.CreatedAt.HasValue)
            {
                task.CreatedAt = updateTaskDto.CreatedAt.Value;
            }

            if (!_taskRepository.UpdateTask(task))
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating task");
            }

            var updatedTask = _taskRepository.GetTaskById(id);
            var taskDto = _mapper.Map<TaskDto>(updatedTask);
            
            // Populate CreatedBy and AssignedTo if they exist
            if (updatedTask?.CreatedById != null)
            {
                var createdByData = _userRepository.GetUserById(updatedTask.CreatedById);
                taskDto.CreatedBy = _mapper.Map<UserDto>(createdByData);
            }
            
            if (updatedTask?.AssignedToId != null)
            {
                var assignedToData = _userRepository.GetUserById(updatedTask.AssignedToId);
                taskDto.AssignedTo = _mapper.Map<UserDto>(assignedToData);
            }

            return Ok(taskDto);
        }

        [HttpDelete("{id:int}", Name = "DeleteTask")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult DeleteTask(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Task ID is required");
            }

            var task = _taskRepository.GetTaskById(id);
            if (task == null)
            {
                return NotFound($"Task {id} not found");
            }

            var userId = User.FindFirst("id")?.Value;
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            var user = _userRepository.GetUserById(userId);
            if (user == null)
            {
                return UnprocessableEntity($"User {userId} doesn't exist.");
            }

            if (task.CreatedById != userId)
            {
                return Forbid();
            }

            if (!_taskRepository.DeleteTask(task))
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting task");
            }

            return NoContent();
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
            
            // Populate CreatedBy and AssignedTo if they exist
            if (item.CreatedById != null)
            {
                var createdByUser = _userRepository.GetUserById(item.CreatedById);
                itemDto.CreatedBy = _mapper.Map<UserDto>(createdByUser);
            }
            
            if (item.AssignedToId != null)
            {
                var assignedToUser = _userRepository.GetUserById(item.AssignedToId);
                itemDto.AssignedTo = _mapper.Map<UserDto>(assignedToUser);
            }
            
            return Ok(itemDto);
        }
    }
}
