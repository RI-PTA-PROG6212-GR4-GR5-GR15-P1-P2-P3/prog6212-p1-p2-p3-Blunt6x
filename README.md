# RaceDay API — Part 2

RaceDay is an ASP.NET Core 8 REST API for managing running, walking and cycling events. It is adapted from the supplied `CourseRegistrationApiNew` starter project: the course/student/registration domain has been replaced with the RaceDay event, category, enrolment and result domain.

## Technology

- C# / ASP.NET Core Web API (.NET 8)
- SQL Server with Entity Framework Core (Code First)
- Password hashing with ASP.NET Core `PasswordHasher<T>`
- JWT bearer authentication with Organiser and Participant roles
- Swagger UI
- xUnit unit tests and GitHub Actions CI

## Project structure

```text
RaceDayFinal/
├── RaceDay.Api/              # API, controllers, EF Core models and JWT service
├── RaceDay.Tests/            # Unit tests for domain rules
├── docs/endpoint-plan.md     # API endpoint specification
└── .github/workflows/        # Automated build and test workflow
```

## Requirements

Install the .NET 8 SDK, Visual Studio 2022 (ASP.NET and web development workload), and SQL Server / SQL Server Express. The supplied archive was developed in Visual Studio, but the API can be opened using `RaceDay.Api/RaceDay.Api.csproj`.

## Configure the database

1. Open `RaceDay.Api/appsettings.json`.
2. Change `ConnectionStrings:DefaultConnection` to your SQL Server instance. The default is LocalDB:
   `Server=(localdb)\\MSSQLLocalDB;Database=RaceDayDb;Trusted_Connection=True;TrustServerCertificate=True;`
3. For a local development database, install the EF tool if needed: `dotnet tool install --global dotnet-ef --version 8.*`.
4. From the repository root, create and apply the first migration:

   ```bash
   dotnet ef migrations add InitialRaceDay --project RaceDay.Api
   dotnet ef database update --project RaceDay.Api
   ```

The database schema is generated from the Code-First entity models. Keep it aligned with your approved Part 1 ERD and SQL script before submission.

## Run the API

From the repository root:

```bash
dotnet restore RaceDay.Api/RaceDay.Api.csproj
dotnet run --project RaceDay.Api
```

When running in Development, open the Swagger URL printed in the console (usually `https://localhost:7xxx/swagger`). Swagger lets you inspect and try the endpoints.

## Authentication and roles

- `POST /api/auth/register` creates an account. The role must be `Organiser` or `Participant`.
- `POST /api/auth/login` returns a bearer token.
- In Swagger, click **Authorize** and enter the token (without adding another `Bearer ` prefix if Swagger's bearer dialog adds it automatically).
- Authenticated endpoints require the token. Organiser-only actions use role authorization; participants cannot manage events/categories/results. Organisers can only manage events they own.
- Passwords are hashed; the original password is never stored.

> **Security before deployment:** the JWT key in `appsettings.json` is a development placeholder. Replace it with a long random secret stored in User Secrets or an environment variable. Do not commit production secrets. Use HTTPS outside local development.

## Run tests

```bash
dotnet test RaceDay.Tests/RaceDay.Tests.csproj
```

The GitHub Actions workflow restores packages, builds the API and runs the unit tests on pushes and pull requests. A successful green build screenshot should be added here after the workflow has run in your own GitHub repository.

## Roles

- **Organiser:** creates and manages their own events, defines categories, views enrolments for events they manage, and records/updates results.
- **Participant:** views events/categories, manages their own profile, enrols in an event/category, and views their own enrolments/results.

## Demonstration video

https://youtu.be/Iraa_1mhn7c
