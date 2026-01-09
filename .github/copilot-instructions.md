# HomePlanner API - Copilot Instructions

## Project Overview
HomePlanner is a household task management API built with ASP.NET Core 9.0. The application manages tasks and users, displaying tasks in two columns based on status (todo/done). Users can be assigned to tasks for household planning.

## Architecture & Stack

- **Framework**: ASP.NET Core 9.0 Web API
- **Database**: SQL Server 2022 (containerized via Docker)
- **ORM**: Entity Framework Core 10.0.1
- **API Documentation**: OpenAPI/Swagger (Swashbuckle 10.1.0)
- **Language Features**: Nullable reference types enabled, implicit usings enabled

## Project Structure

```
HomePlanner/
├── Controllers/      # API Controllers
│   └── Constants/    # Controller constants
├── Data/             # ApplicationDbContext and database configuration
├── Migrations/       # EF Core migrations (Identity support implemented)
├── Models/           # Domain entities (Task, User, ApplicationUser)
│   └── Dtos/         # Data Transfer Objects organized by entity
│       ├── ApplicationUser/
│       ├── Task/
│       └── User/
├── Repository/       # Repository pattern implementations
│   └── IRepository/  # Repository interfaces
├── Mapping/          # AutoMapper profiles
├── Shared/           # Shared resources
│   └── Constants/    # CacheProfiles, PolicyNames
└── Program.cs        # Application entry point and service configuration
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
- **Example**: Task model includes Id, Title (required), optional Description, and DueDate

### Authentication & Identity
- **Identity Framework**: ASP.NET Core Identity integrated with ApplicationUser model
- **JWT Configuration**: Located in appsettings.json under `ApiSettings`
- **Issuer and Audience**: `http://localhost:5089`
- **User DTOs**: CreateUserDto, UserDto, UserLoginDto, UserLoginResponseDto, UserRegisterDto
- **UserData**: UserDataDto for ApplicationUser information

### Repository Pattern
- **Interfaces**: Defined in `Repository/IRepository/` (e.g., IUserRepository)
- **Implementations**: Concrete repositories in `Repository/` (e.g., UserRepository)
- **Operations**: Standard CRUD operations (GetAll, GetById, Add, Update, Delete)

### Mapping Configuration
- **AutoMapper**: Profiles in `Mapping/` folder (UserProfile configured)
- **Profile Pattern**: Each entity type has its own mapping profile

### Controllers
- **Location**: `Controllers/` folder
- **Implemented**: UsersController for user management
- **Constants**: Controller-specific constants in `Controllers/Constants/`

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

## Task Status Management
Tasks have two states: **todo** and **done**. Ensure Task model includes a status property (e.g., `TaskStatus` enum or string) to support the two-column display requirement.

## Important Notes
- **Null Safety**: Project uses nullable reference types - be explicit with nullability
- **Docker**: SQL Server runs in container `sqlserver2022-homeplanner` on port 1433
- **Data Persistence**: Docker volume `sqlserverdata` persists database across container restarts

## Implemented Features
- ASP.NET Core Identity integrated with ApplicationUser
- User repository pattern with IUserRepository and UserRepository
- UsersController with complete CRUD operations
- AutoMapper configuration for User entity
- JWT authentication configuration
- Database migrations for Identity and User tables
