# FinalAssignment.Therapy.Web

Therapy booking final assignment scaffold based on the therapy domain from the previous assignment, rebuilt to satisfy the final requirements:

- Repository + Unit of Work over EF Core only.
- Full async from service layer down to repository and `SaveChangesAsync`.
- Cookie authentication with role-based authorization for `Admin` and `Therapist`.
- SignalR admin dashboard updates.
- Hosted background worker that expires pending unpaid appointments older than 24 hours.
- Admin CRUD screens for therapist profiles and therapy services.

## Demo accounts

- Admin: `admin@therapy.local` / `Admin@123`
- Therapist: `anna@therapy.local` / `Therapist@123`

## SQL Server with Docker

1. Copy `.env.example` to `.env`.
2. From this folder run `docker compose up -d`.
3. The SQL Server container persists data in the named volume `final_therapy_sqlserver_data`, so data is retained after the container stops.
4. The app reads the password from `Database:Password` or `FINAL_THERAPY_DB_PASSWORD` and replaces the `{DB_PASSWORD}` placeholder in the connection string.
5. On startup the app applies EF Core migrations with `Database.Migrate()` and then seeds demo data if the database is empty.

## EF Core migrations

- Initial migration has been generated in the infrastructure project.
- To add a new migration later, run `dotnet ef migrations add <Name> --project .\FinalAssignment.Therapy.Infrastructure\FinalAssignment.Therapy.Infrastructure.csproj --startup-project .\FinalAssignment.Therapy.Web\FinalAssignment.Therapy.Web.csproj --output-dir Migrations` from the repository root.

## Run the web app

1. Start SQL Server with Docker.
2. Run `dotnet run --project ..\FinalAssignment.Therapy.Web\FinalAssignment.Therapy.Web.csproj` from the repository root, or start the web project from VS Code.
3. The app seeds demo users, therapists, and therapy services on first launch.

## Assignment mapping

- Controllers only depend on services, never `DbContext`.
- Repositories and unit of work are registered as `AddScoped`.
- `IClock`, `IPasswordHasher`, and the SignalR notifier are singleton services.
- `IUserClaimsPrincipalFactory` is a transient service.