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
├── Controllers/           # API Controllers (UsersController, TaskController)
│   └── Constants/         # Controller constants (CustomErrorKey, DefaultImage)
├── Data/                  # ApplicationDbContext (IdentityDbContext<ApplicationUser>)
├── Migrations/            # EF Core migrations
├── Models/                # Domain entities (Task, User, ApplicationUser)
│   ├── Dtos/              # Data Transfer Objects organized by entity
│   │   ├── ApplicationUser/   # UserDataDto
│   │   ├── Task/              # TaskDto, CreateTaskDto, UpdateTaskDto
│   │   └── User/              # UserDto, CreateUserDto, UserLoginDto, etc.
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
- **Required Properties**: Use `required` modifier for mandatory relationships (e.g., `public required ApplicationUser CreatedBy { get; set; }`)
- **DTOs Structure**: Organized by entity (ApplicationUser/, Task/, User/)
- **Task Model**: Includes Id, Title (required), optional Description, Status (TaskStatus enum), StartDate, EndDate, LastCompletedDate, CreatedAt, and relationships to ApplicationUser (CreatedBy, AssignedTo)

### TaskStatus Enum
Tasks have three states defined in the `TaskStatus` enum:
- **Todo** (0): Task not started
- **Doing** (1): Task in progress  
- **Done** (2): Task completed

### Authentication & Identity
- **Identity Framework**: ASP.NET Core Identity integrated with ApplicationUser model
- **ApplicationUser**: Extends IdentityUser, adds optional Name property
- **JWT Configuration**: Located in appsettings.json under `ApiSettings`
- **Issuer and Audience**: `http://localhost:5089`
- **Authorization**: Role-based with `[Authorize(Roles = "Admin")]` attribute
- **User DTOs**: CreateUserDto, UserDto, UserLoginDto, UserLoginResponseDto, UserDataDto

### Repository Pattern
- **Interfaces**: Defined in `Repository/IRepository/` (IUserRepository, ITaskRepository)
- **Implementations**: Concrete repositories in `Repository/` (UserRepository, TaskRepository)
- **User Operations**: GetUsers, GetUserById, UserExistsByEmail, Login, Register, SaveAsync
- **Task Operations**: GetTasks (paginated), GetTaskById, GetTotalTasks, CreateTask, Save

### Mapping Configuration
- **AutoMapper**: Profiles in `Mapping/` folder
- **UserProfile**: Maps User ↔ DTOs and ApplicationUser ↔ UserDto/UserDataDto
- **TaskProfile**: Maps Task ↔ TaskDto/CreateTaskDto/UpdateTaskDto

### Controllers
- **UsersController**: User management with Register/Login endpoints (anonymous) and CRUD operations (Admin only)
- **TaskController**: Task management with pagination, requires Admin role (GetTasks allows anonymous)
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
- UsersController with Register, Login, and CRUD operations
- TaskController with paginated task listing and task creation
- AutoMapper configuration for User and Task entities
- JWT Bearer Token authentication
- Role-based authorization (Admin role)
- Response caching with configurable profiles
- Swagger/OpenAPI documentation with JWT support
