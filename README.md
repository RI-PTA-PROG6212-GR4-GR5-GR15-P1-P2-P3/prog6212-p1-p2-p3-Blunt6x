# README

# RaceDay — Part 1 · Part 2 · Part 3

RaceDay is a platform for running, walking and cycling events: a REST API (Part 2) plus an ASP.NET Core MVC web front end (Part 3) with Azure Blob Storage image uploads, packaged with Docker.

> **Part 3 lives on the `part3-mvc` branch** (project `RaceDay.Web/`, API changes in `RaceDay.Api/`).
> 

## Part 3 — MVC web application

### Architecture

```
Browser ──► RaceDay.Web (MVC, session) ──HTTP/JSON──► RaceDay.Api ──► SQL Server
                                                          └────────► Azure Blob Storage (images)
```

- `RaceDay.Web` has **no database access and no business rules**. Every read/write goes through `Infrastructure/RaceDayApiClient` (a typed `HttpClient`) to the Part 2 API, including image uploads.
- **Session authentication:** on login the API returns a JWT; the MVC app keeps the token, name and role in server-side session. `[SessionAuthorize]` / `[SessionAuthorize("Organiser")]` protect pages; an expired token (API 401) ends the session and returns the user to the login page. Logout clears the session.
- **Role-specific interfaces:** Participants get Events, My Enrolments, My Results, Profile. Organisers get Dashboard, Events, Categories, Enrolments, Results, Profile.
- **Azure Blob Storage** is integrated in the **API** (`Services/BlobStorageService.cs`): `POST /api/events/{id}/banner` and `POST /api/profile/picture`. The MVC app never talks to Azure; the returned blob URL is shown on the event pages.
- Errors from the API are translated into friendly messages; unexpected exceptions render a branded error page.

### New / changed API endpoints (Part 3)

| Method | Route | Purpose |
| --- | --- | --- |
| POST | `/api/events/{id}/banner` | Upload event banner (Organiser, owner) → Blob Storage |
| POST | `/api/profile/picture` | Upload profile picture → Blob Storage |
| GET | `/api/events/mine` | Organiser's events with enrolment/category/result counts |
| PUT | `/api/enrolments/{id}/status` | Organiser sets Confirmed / Pending / Cancelled |
| GET | `/api/results/my` | Now includes `totalFinishers` (for "47th out of 312") |

New enrolments are recorded immediately as **Confirmed**. Results can only be captured once the event day has arrived.

### Run locally

1. **Database columns.** Apply EF migrations (`dotnet ef migrations add Part3_ImageUrls --project RaceDay.Api` then `dotnet ef database update --project RaceDay.Api`) **or**, if you created the DB from SQL, run `docs/part3-schema-changes.sql`.
2. **Azure storage.** Create a Storage Account, then give the API its connection string (never commit it):
The container `raceday-images` is created automatically with public *blob* read access, so the storage account must allow anonymous blob access. (Azurite works for local testing: `UseDevelopmentStorage=true`.)
    
    ```bash
    cd RaceDay.Api
    dotnet user-secrets init
    dotnet user-secrets set "AzureBlobStorage:ConnectionString" "<your storage account connection string>"
    ```
    
3. **Start the API** (https on 7243): `dotnet run --project RaceDay.Api`
4. **Start the MVC app** (https on 7100): `dotnet run --project RaceDay.Web` — it points at `https://localhost:7243/` by default (`ApiSettings:BaseUrl`).

### Run with Docker

The container needs to reach the API running on your machine over plain HTTP. Start the API with the Docker-friendly profile (listens on `http://*:5243`, HTTPS redirection off):

```bash
dotnet run --project RaceDay.Api --launch-profile "RaceDay.Api (http, Docker-friendly)"
```

Then build and run the MVC app (single command to start it once built):

```bash
docker build -t raceday-web ./RaceDay.Web
docker run --rm -p 8081:8080 --add-host=host.docker.internal:host-gateway -e ApiSettings__BaseUrl=http://host.docker.internal:5243/ raceday-web
```

Open http://localhost:8081. To use a different API address change `ApiSettings__BaseUrl`. The image is a multi-stage build (SDK → ASP.NET runtime), runs as a non-root user and behaves the same as `dotnet run`.

### Part 3 checklist

- [x]  MVC app consuming the API only · session auth · role-based navigation
- [x]  Organiser: dashboard, events CRUD + banner upload, categories, enrolments (with status changes), results entry
- [x]  Participant: filtered event list, event details + categories, enrol, My Enrolments, My Results, Profile + picture
- [x]  Azure Blob Storage via the API · Dockerfile · CI builds the MVC project and Docker image
- [ ]  Unlisted YouTube demo video: *add link here*

---

# Part 2 — RaceDay API

RaceDay is an ASP.NET Core 8 REST API for managing running, walking and cycling events. It is adapted from the supplied `CourseRegistrationApiNew` starter project: the course/student/registration domain has been replaced with the RaceDay event, category, enrolment and result domain.

## Technology

- C# / ASP.NET Core Web API (.NET 8)
- SQL Server with Entity Framework Core (Code First)
- Password hashing with ASP.NET Core `PasswordHasher<T>`
- JWT bearer authentication with Organiser and Participant roles
- Swagger UI
- xUnit unit tests and GitHub Actions CI

## Project structure

```
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
- In Swagger, click **Authorize** and enter the token (without adding another `Bearer`  prefix if Swagger's bearer dialog adds it automatically).
- Authenticated endpoints require the token. Organiser-only actions use role authorization; participants cannot manage events/categories/results. Organisers can only manage events they own.
- Passwords are hashed; the original password is never stored.

> **Security before deployment:** the JWT key in `appsettings.json` is a development placeholder. Replace it with a long random secret stored in User Secrets or an environment variable. Do not commit production secrets. Use HTTPS outside local development.
> 

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