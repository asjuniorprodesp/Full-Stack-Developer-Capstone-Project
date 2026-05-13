# SkillSnap

## Project Summary

SkillSnap is a full-stack portfolio application built with ASP.NET Core and Blazor WebAssembly. The app lets users browse portfolio profiles, inspect projects and skills, and manage portfolio data through a REST API backed by Entity Framework Core and SQLite.

The solution is split into two parts:
- `SkillSnap.Api`: ASP.NET Core Web API with Identity, JWT authentication, caching, and EF Core persistence.
- `SkillSnap.Client`: Blazor WebAssembly front end with authentication, session state, and a responsive UI.

## Key Features

### CRUD and Data Management
- Portfolio profile viewing and editing.
- Project listing and creation.
- Skill listing and creation.
- Database seed and reset workflow for quick testing.
- EF Core relationships with related data loaded efficiently using `Include()` and `AsNoTracking()`.

### Security
- ASP.NET Identity for user registration and login.
- JWT token generation and validation.
- Role-based authorization for elevated actions such as project and skill creation.
- Token storage in browser local storage with automatic reuse after reload.

### Caching and Performance
- In-memory caching for common API queries.
- Cache expiration and fallback logic to reduce database load.
- Cache refresh after CRUD operations to keep returned data consistent.
- Logging of cache hit, miss, and refresh events for verification.

### Blazor State Management
- Scoped state container for logged-in user information.
- Persisted session state for selected profile and editing context.
- Shared state between components without reloading the page.

## Development Process and Use of Copilot

This project was built incrementally with Copilot assisting in planning, code generation, and review. Copilot was used to:
- scaffold API controllers and Blazor pages,
- implement authentication and JWT handling,
- add client-side token storage and session state,
- refactor components for cleaner data flow,
- optimize API queries and caching,
- polish the UI for desktop and mobile layouts.

The workflow was iterative: implement a feature, build both projects, fix compile/runtime issues, and then refine the user experience. Copilot was especially useful for accelerating repetitive code such as service classes, form components, and API response handling.

## Known Issues and Future Improvements

### Known Issues
- Some CRUD screens are still basic and could use better validation and feedback messages.
- Admin role assignment still depends on backend setup or manual seeding.
- The profile update flow is simple and could be expanded to handle multiple users more explicitly.

### Future Improvements
- Add a proper role seeding flow for an initial admin account.
- Replace the current profile editing approach with dedicated API methods and cleaner services.
- Add unit and integration tests for authentication, caching, and state management.
- Improve loading states, empty states, and error handling across the Blazor UI.
- Add pagination or search for projects and skills if the dataset grows.
- Consider using a delegated `HttpMessageHandler` for bearer token injection instead of setting headers in individual services.

## Running the Solution

### API
```bash
dotnet run --project SkillSnap.Api
```

### Client
```bash
dotnet run --project SkillSnap.Client
```

Make sure the API is available before using the client.

## Notes

The project includes:
- JWT authentication
- role-based authorization
- in-memory caching with fallback
- responsive Blazor UI
- local session persistence
