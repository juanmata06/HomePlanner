# HomePlanner API - Copilot Instructions

## Developer Context
**Learning Profile:** First-time .NET developer learning ASP.NET Core 9.0. This project serves as a hands-on learning exercise with the goal of deploying the API alongside an Angular frontend on an Ubuntu Server using Docker containers.

**Interaction Style:** If you need more context about requirements, architecture decisions, or existing code before providing a solution, please ask clarifying questions rather than making assumptions.

## Project Overview
HomePlanner is a household task management API built with ASP.NET Core 9.0. The application manages tasks and users, displaying tasks in columns based on status (todo/doing/done). Users can be assigned to tasks for household planning, and tasks track who created them and who they are assigned to.

## Architecture & Stack

- **Framework**: ASP.NET Core 9.0 Web API
- **Database**: SQL Server 2022 (containerized via Docker)
- **ORM**: Entity Framework Core 9.0.x
- **API Documentation**: OpenAPI/Swagger (Swashbuckle 7.2.0)
- **Object Mapping**: AutoMapper 14.0.0
- **Authentication**: ASP.NET Core Identity + JWT Bearer Tokens
- **Language Features**: Nullable reference types enabled, implicit usings enabled
- **Frontend** (planned): Angular application
- **Target Deployment**: Ubuntu Server with Docker containers

## Project Structure

```
HomePlanner/
├── Controllers/           # API Controllers (AuthController, UsersController, TaskController)
│   └── Constants/         # Controller constants (CustomErrorKey, DefaultImage)
├── Data/                  # ApplicationDbContext (IdentityDbContext<ApplicationUser>)
├── Migrations/            # EF Core migrations
├── Models/                # Domain entities (Task, User, ApplicationUser)
│   ├── Dtos/              # Data Transfer Objects organized by entity
│   │   ├── ApplicationUser/   # UserDataDto
│   │   ├── Task/              # TaskDto, CreateTaskDto, UpdateTaskDto
│   │   └── User/              # UserDto, UserGetDto, CreateUserDto, UserLoginDto, UserRegisterDto, UserRegisterResponseDto, UserLoginResponseDto
│   └── Responses/         # API response models (PaginationResponse)
├── Repository/            # Repository pattern implementations
│   └── IRepository/       # Repository interfaces
├── Mapping/               # AutoMapper profiles (UserProfile, TaskProfile)
├── Shared/                # Shared resources
│   └── Constants/         # CacheProfiles, PolicyNames
└── Program.cs             # Application entry point and service configuration
```

## Development Setup

### Running the Database
```bash
# Start SQL Server container
docker-compose up -d

# Stop container (preserves data)
docker-compose down
```

**Connection String**: `Server=localhost;Database=HomePlannerDotNet9;User ID=SA;Password=MyStrongPass123;TrustServerCertificate=true;MultipleActiveResultSets=true`

### Running the Application
```bash
dotnet run --project HomePlanner/HomePlanner.csproj
```

Access Swagger UI at: `https://localhost:<port>/swagger`

## Code Conventions

### Models & Entities
- **Location**: Domain entities in `Models/` (Task, User, ApplicationUser), DTOs in `Models/Dtos/` organized by entity
- **Naming**: Use singular names (e.g., `Task`, `User`)
- **Properties**: Use `string.Empty` for non-nullable string defaults
- **Nullable**: Use `?` for optional properties (`public string? Description { get; set; }`)
- **DTOs Structure**: Organized by entity (ApplicationUser/, Task/, User/)
- **Task Model**: Includes Id, Title, optional Description, Status (TaskStatus enum), StartDate, EndDate, LastCompletedDate, CreatedAt, and optional relationships to ApplicationUser (CreatedBy, AssignedTo). Foreign keys `CreatedById` and `AssignedToId` are nullable.
- **ApplicationUser Model**: Extends IdentityUser with optional Name, ImgUrl, and ImgUrlLocal properties for profile images

### TaskStatus Enum
Tasks have three states defined in the `TaskStatus` enum:
- **Todo** (0): Task not started
- **Doing** (1): Task in progress  
- **Done** (2): Task completed

### Authentication & Identity
- **Identity Framework**: ASP.NET Core Identity integrated with ApplicationUser model
- **ApplicationUser**: Extends IdentityUser, adds optional Name, ImgUrl, and ImgUrlLocal properties
- **JWT Configuration**: Located in appsettings.json under `ApiSettings`
- **Issuer and Audience**: `http://localhost:5089`
- **Authorization**: Controller-level `[Authorize]` with `[AllowAnonymous]` for public endpoints
- **User DTOs**: 
  - `CreateUserDto` - For creating users (requires Name, Email, Password, Role)
  - `UserDto` - Basic user info (Id, Name, Email)
  - `UserGetDto` - Extended user info with image fields (Id, Name, Email, ImgUrl, ImgUrlLocal)
  - `UserDataDto` - Full user data including Role and image fields
  - `UserLoginDto`, `UserLoginResponseDto` - For authentication
  - `UserRegisterDto`, `UserRegisterResponseDto` - For registration

### Repository Pattern
- **Interfaces**: Defined in `Repository/IRepository/` (IUserRepository, ITaskRepository)
- **Implementations**: Concrete repositories in `Repository/` (UserRepository, TaskRepository)
- **User Operations**: GetUsers, GetUserById, UserExistsByEmail, Login, Register, GenerateTokenAsync, UpdateUser, DeleteUser, SaveAsync
- **Task Operations**: GetTasks (paginated), GetTasksByWeek, GetTaskById, GetTotalTasks, CreateTask, UpdateTask, DeleteTask, Save

### Mapping Configuration
- **AutoMapper**: Profiles in `Mapping/` folder
- **UserProfile**: Maps ApplicationUser ↔ UserDataDto/UserDto/UserGetDto/UserRegisterResponseDto, CreateUserDto → ApplicationUser
- **TaskProfile**: Maps Task ↔ TaskDto/CreateTaskDto/UpdateTaskDto

### Controllers
- **AuthController**: Authentication endpoints (public by default)
  - `POST /api/auth/register` - User registration
  - `POST /api/auth/login` - User login, returns JWT token
  - `GET /api/auth/profile` - Get current user profile (requires auth), refreshes token
- **UsersController**: User management (Admin role required)
  - `GET /api/users` - List all users
  - `GET /api/users/{id}` - Get user by ID
  - `PUT /api/users/{id}` - Update user
  - `DELETE /api/users/{id}` - Delete user
- **TaskController**: Task management (requires auth, some endpoints allow anonymous)
  - `GET /api/task/tasks` - Paginated task listing (anonymous)
  - `GET /api/task/by-week` - Get tasks for a specific week (anonymous)
  - `GET /api/task/{id}` - Get task by ID (anonymous)
  - `POST /api/task` - Create task (auth required)
  - `PUT /api/task/{id}` - Update task (auth required, only creator can update)
  - `DELETE /api/task/{id}` - Delete task (auth required, only creator can delete)
- **Constants**: Controller-specific constants in `Controllers/Constants/` (CustomErrorKey, DefaultImage)
- **Response Caching**: Uses CacheProfiles (Default10, Default20) defined in `Shared/Constants/`

### API Responses
- **PaginationResponse<T>**: Generic paginated response with Page, Size, TotalPages, and Items properties
- **Location**: `Models/Responses/` folder

## Database Migrations
```bash
# Add migration
dotnet ef migrations add MigrationName --project HomePlanner

# Update database
dotnet ef database update --project HomePlanner
```

**Current Migrations**: 
- InitialMigration
- CreateTableUser
- AddIdentitySupport (with corrections)
- UpdateTaskWithCorrectForeignKeys
- AddImageFieldsToApplicationUser
- MakeTaskFieldsOptional

## Database Configuration
- **DbContext**: `ApplicationDbContext` extends `IdentityDbContext<ApplicationUser>`
- **DbSets**: Tasks, Users, ApplicationUsers
- **Relationships**: Task has CreatedBy and AssignedTo relationships to ApplicationUser
- **Delete Behavior**: CreatedBy uses NoAction, AssignedTo uses Cascade

## Important Notes
- **Null Safety**: Project uses nullable reference types - be explicit with nullability
- **Docker**: SQL Server runs in container `sqlserver2022-homeplanner` on port 1433
- **Data Persistence**: Docker volume `sqlserverdata` persists database across container restarts
- **Lowercase URLs**: Routes are configured to use lowercase (`options.LowercaseUrls = true`)

## Implemented Features
- ASP.NET Core Identity integrated with ApplicationUser
- User repository pattern with IUserRepository and UserRepository
- Task repository pattern with ITaskRepository and TaskRepository
- AuthController for authentication (register, login, profile)
- UsersController with CRUD operations (Admin role required)
- TaskController with full CRUD, pagination, and week-based filtering
- AutoMapper configuration for User and Task entities
- JWT Bearer Token authentication with token refresh on profile endpoint
- Role-based authorization (Admin role for user management)
- Task ownership validation (only creator can update/delete)
- Response caching with configurable profiles
- Swagger/OpenAPI documentation with JWT support
- User profile images support (ImgUrl, ImgUrlLocal fields)
