# TaskTracker – Backend API 

This is the ASP.NET Core Web API backend for TaskTracker.  
It features JWT authentication with refresh tokens, role-based access control, data export (PDF/CSV), and in-memory database seeding.

## Tech Stack
- ASP.NET Core 9 Web API
- Entity Framework Core (InMemory)
- JWT + Refresh Token
- QuestPDF (PDF export)
- CSV export (StringBuilder)
- xUnit for testing
- Azure App Service deployment

## Folder Structure
- Controllers/: API endpoints
- Services/: Business logic
- Repositories/: Data access (if applicable)
- DTO/: Data transfer objects
- Models/: Entity definitions
- Data/: EF Core DbContext
- Tests/: xUnit test projects

## Authentication Overview
- JWT issued on login
- Access token stored client-side (localStorage)
- Refresh token stored in secure HttpOnly cookie
- Auto-refresh handled on client

## Database & Seeding
- Uses EF Core's InMemory provider (no SQL setup)
- Seeds demo users and tasks on startup

To persist data, update `Program.cs` to use `UseSqlServer(...)` and add your connection string in `appsettings.json`.

## Example appsettings.json
"Jwt": {
  "Key": "s3cr3tK3yJwtSecur3t0kenPassw0rd!!",
  "Issuer": "TaskTrackerApi",
  "Audience": "TaskTrackerClient"
},
"AdminCredentials": {
  "Email": "assignEmail@example.com",
  "UserName": "assignUser",
  "Password": "assignPassword!2#"
}