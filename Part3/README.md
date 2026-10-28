# RaceDay — Part 3: MVC Web Application

## Description

RaceDay is a web-based event management platform for the South African road running, walking and cycling community. Part 3 adds the web front end: an ASP.NET Core MVC application that gives Organisers and Participants a role-specific interface. The MVC application contains **no database access and no duplicated business logic**. Every operation goes through the Part 2 RESTful API.

Part 3 also adds:
- **Azure Blob Storage**, integrated into the **API** (the MVC app never talks to Azure directly), for event banner images and profile pictures
- **Docker**, so the MVC application can be started with a single command

## Roles

- **Organiser**: has a dashboard (events, enrolments per event, upcoming dates), creates, edits and deletes events (name, description, date, location, distance, event type, banner image), manages categories, views the enrolments for their events and captures results. Saving a result publishes it straight away.
- **Participant**: browses and filters upcoming events, views event details and categories, enrols by choosing a category, sees their enrolments with colour-coded status, sees their race history on My Results (for example "47th out of 312 finishers") and manages their profile and profile picture.

Each role has its own navigation menu. Role checks are enforced in the API (Part 2) and again in the MVC controllers with `[Authorize(Roles = ...)]`. Unauthenticated users who open a protected page are redirected to the login page, and a user of the wrong role sees an "Access denied" page.

## Architecture

```
Browser ──► RaceDay.Mvc (ASP.NET Core MVC) ──► RaceDay.Api (ASP.NET Core Web API) ──► SQL Server
                                                         └──────────────────────────► Azure Blob Storage
```

- **Session management:** on login the MVC app calls the API's login endpoint, keeps the API's auth cookie in its own **server-side session**, and signs the user in with an MVC cookie that carries the user's id, name and role. A small `DelegatingHandler` attaches the stored API cookie to every API request, so the user stays authenticated, with their role, across all pages. Logging out clears the session on both sides and redirects to the login page.
- **API client:** one typed service (`RaceDayApiClient`) wraps all calls to the API. Controllers handle API failures gracefully (expired session, access denied, service unavailable) and never show raw error messages to the user.

## Schema changes since Part 1

The Part 3 requirements need three pieces of data that the Part 1 schema did not hold. They are added by `docs/part3_schema_changes.sql`:

| Table | New column | Purpose |
|---|---|---|
| Event | `eventType` (Run, Walk or Cycle, default Run) | Event type required by the brief |
| Event | `bannerImageUrl` | URL of the banner stored in Azure Blob Storage |
| User | `profilePictureUrl` | URL of the profile picture stored in Azure Blob Storage |

An event's location is stored in the existing `Venue` table (address and city), so no extra column was needed for it.

## Other design decisions

- **Enrolment status colours:** the database stores Active, Completed and Withdrawn. The interface shows Active as green **Confirmed**, Completed as blue, Withdrawn as grey and any other status as amber **Pending**.
- **Results:** saving a result publishes it immediately, so it appears on the participant's My Results page. The "out of N finishers" figure counts finishers in the same category.
- **Browsing is public:** anyone can view events and event details. Enrolling requires a logged-in Participant.
- **Deleting:** deleting an event also removes its venues, categories, enrolments and results. A category that already has enrolments cannot be deleted. An enrolment that already has a result cannot be withdrawn.

## Setup and run instructions

### Prerequisites
- .NET 10 SDK
- SQL Server (Express is fine) and SQL Server Management Studio
- Docker Desktop (for the container)
- Optional: an Azure Storage account (for image uploads)

### 1. Database
In SSMS, run these scripts in order:
1. `docs/raceday_database_script.sql` (Part 1 schema and sample data)
2. `docs/part3_schema_changes.sql` (Part 3 columns)

### 2. API
1. Open `Part2/RaceDay.Api/RaceDay.Api/appsettings.json` and set `ConnectionStrings:RaceDayDb` to your SQL Server instance.
2. (Optional, for image uploads) store the Azure connection string in user-secrets. It is **never committed to the repository**:
   ```
   cd Part2/RaceDay.Api/RaceDay.Api
   dotnet user-secrets set "AzureStorage:ConnectionString" "<your storage connection string>"
   ```
   Create two containers in the storage account, `event-banners` and `profile-pictures`, with anonymous read access at **Blob** level. If no connection string is set, the rest of the app works normally and uploads show the message "Image storage isn't available right now."
3. Start the API with the `http` profile:
   ```
   dotnet run --launch-profile http
   ```
   It listens on `http://localhost:5214` (local use) and `http://0.0.0.0:5080` (Docker). Swagger is at `http://localhost:5214/swagger`.

### 3. MVC application (local)
```
cd Part3/RaceDay.Mvc
dotnet run
```
Open the address printed in the console. The API address is set in `appsettings.json` under `ApiSettings:BaseUrl` (default `http://localhost:5214/`).

Register a new Organiser and a new Participant from the Register page to try both interfaces.

### 4. Docker
The API keeps running on your machine, because it uses your local SQL Server. The container reaches it through `host.docker.internal`.

**Single command** (from `Part3/RaceDay.Mvc`):
```
docker compose up --build
```
Then open `http://localhost:8080`.

Equivalent plain Docker commands:
```
docker build -t raceday-mvc .
docker run -d --name raceday-mvc -p 8080:8080 -e ApiSettings__BaseUrl=http://host.docker.internal:5080/ raceday-mvc
```

The containerised application behaves the same as the local one. The Dockerfile uses a multi-stage build, caches the NuGet restore layer, runs as a non-root user and takes the API address from an environment variable.

## CI/CD

The GitHub Actions workflow (`.github/workflows/dotnet-ci.yml`) runs on every push. It builds the API and runs its unit tests, builds the MVC project, and builds the MVC Docker image.

**Successful build screenshot:**

`[INSERT SCREENSHOT HERE]`

## Video walkthrough

Unlisted YouTube video demonstrating the running application, the MVC structure, the Blob Storage integration and the Docker setup:

`[INSERT YOUTUBE LINK HERE]`
