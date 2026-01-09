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
├── Models/           # Domain entities (Task.cs) and DTOs (Models/Dtos/)
├── Data/             # DbContext and database configuration (to be implemented)
├── Repository/       # Repository pattern interfaces and implementations
│   └── IRepository/  # Repository interfaces
├── Mapping/          # AutoMapper profiles or mapping configuration
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
- **Location**: Place domain entities in `Models/`, DTOs in `Models/Dtos/`
- **Naming**: Use singular names (e.g., `Task`, `User`)
- **Properties**: Use `string.Empty` for non-nullable string defaults
- **Nullable**: Use `?` for optional properties (`public string? Description { get; set; }`)
- **Example**: See [Models/Task.cs](HomePlanner/Models/Task.cs) - includes Id, Title (required), optional Description, and DueDate

### User Model Pattern
- User model includes: Id, Name (optional), Email (required), Password (required), Role (optional)
- Use Data Annotations: `[Key]`, `[Required(ErrorMessage = "...")]`
- See [Models/Dtos/User.cs](HomePlanner/Models/Dtos/User.cs) for reference

### Repository Pattern
- Define interfaces in `Repository/IRepository/`
- Implement concrete repositories in `Repository/`
- Follow standard CRUD operations: GetAll, GetById, Add, Update, Delete

### Authentication & Security
- JWT configuration in appsettings.json under `ApiSettings`
- Issuer and Audience: `http://localhost:5089`
- Secret key must be sufficiently long for encryption

## Database Migrations
```bash
# Add migration
dotnet ef migrations add MigrationName --project HomePlanner

# Update database
dotnet ef database update --project HomePlanner
```

## Task Status Management
Tasks have two states: **todo** and **done**. Ensure Task model includes a status property (e.g., `TaskStatus` enum or string) to support the two-column display requirement.

## Important Notes
- **Null Safety**: Project uses nullable reference types - be explicit with nullability
- **Docker**: SQL Server runs in container `sqlserver2022-homeplanner` on port 1433
- **Data Persistence**: Docker volume `sqlserverdata` persists database across container restarts

## Next Steps for Development
1. Create DbContext in `Data/` folder
2. Implement Repository interfaces in `Repository/IRepository/`
3. Create concrete repository implementations
4. Add Controllers for Tasks and Users endpoints
5. Configure AutoMapper in `Mapping/` for DTO conversions
6. Add authentication middleware and JWT bearer configuration to Program.cs
7. Add Task status property and enum
