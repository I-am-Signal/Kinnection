# Kinnection

Kinnection is a web application that will allow the user to track their family tree. Using this system, the user will be able to:

- Login (via direct login or Google Sign-On, potentially both later)
- CRUD Family trees through a dashboard UI
- CRUD Family members in family trees through a tree-specific UI

## Composition

- `Docker/`: Docker related files
- `Kinnection-Database/`: MySQL database related static files and data
- `Kinnection-Backend/`: ASP.NET Core Web API Backend App
- `Kinnection-Frontend/`: Angular Frontend App

## Running the App

### Required Software

- C#/.NET SDK 8.0+
- Docker Desktop (or just the Docker engine)

### Running in Development Environment

- Ensure all environment variables are filled in in the `.env` file in `Kinnection/Docker`.
- Navigate to the `Kinnection/Docker` directory.
- `docker compose up -d --build`:
  - Builds and brings up the containers
- `docker compose down <-v>`:
  - Tears down the containers
  - `-v` will also remove any associated container volumes.
    - Only use this argument when you need a clean slate as it removes all associated permant data storage in the volumes.

## Running the tests

### Running in Development Environment

- Navigate to the `Kinnection/Kinnection-Backend/test` directory.
- Get environment variables needed for local run:
  - Windows:
    - `Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process`
    - `../app/GetEnvVars.ps1`
  - MAC/Linux:
    - `chmod +x /path/to/GetEnvVars.sh`
    - `../app/GetEnvVars.sh`
- `dotnet test` to run the tests

## Migrations

**Generating a migration**:

- Navigate to the `Kinnection/Kinnection-Backend/app` directory.
- `dotnet ef migrations add MIGRATION_NAME`

**Applying a migration**:

- Navigate to the `Kinnection/Kinnection-Backend/app` directory.
- Get environment variables needed for local run:
  - Windows:
    - `Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process`
    - `./GetEnvVars.ps1`
  - MAC/Linux:
    - `chmod +x /path/to/GetEnvVars.sh`
    - `./GetEnvVars.sh`
- Use `dotnet ef database update MIGRATION_NAME` to migration the database

  - You can also use this command to migrate to a previous migration by using the older migration's name.

- _NOTE_: Existing/in-the-pipeline migrations are auto-applied to the database upon startup of the Kinnection-Backend app based on the `APPLY_MIGRATIONS` environment variable. For more information, please see the [Database Management Vars](#database-management-vars) section.

## Environment Variables

The `.env.template` file contains a template of the `.env` file that should be placed in the `Docker/` directory. In it are the following variables:

### MySQL Container Vars

- `MYSQL_PROTOCOL`: Protocol for connecting to the database. For MySQL, it is `mysql`.
- `MYSQL_PORT`: Port at which the database can be accessed.
- `MYSQL_CONTAINER_PORT`: Port at which the container binds to the overlaying database.
- `MYSQL_ROOT_PASSWORD`: Root password for the database .
- `MYSQL_DATABASE`: Name of the database.
- `MYSQL_USER`: User of the database.
- `MYSQL_PASSWORD`: Password of the user.
- `MYSQL_HOST`: Name of the host connection for the database.
- `MYSQL_EXTERNAL_HOST`: Name of the local host connection of the database.

### ASP.NET Core Container Vars

- `ASPNETCORE_ENVIRONMENT`: Environment in which the ASP.NET Core webapi app should assume. (`Development`, `Staging`, `Production`)
- `ASP_PORT`: Port at which the ASP.NET Core webapi app can be accessed.
- `ASP_CONTAINER_PORT`: Port at which the container binds to the overlaying webapi app.

### Database Management Vars

- `RETRY_ATTEMPTS`: Number of times the ASP.NET Core webapi app reattempts to connection to the database
- `RETRY_IN`: Number of seconds the ASP.NET Core webapi app waits between attempts
- `APPLY_MIGRATIONS`: Whether to auto-apply migrations on startup of the ASP.NET Core app. 1 for True, 0 for False.

### Angular Container Vars

- `ANG_PORT`: Port at which the Angular frontend app can be accessed.
- `ANG_CONTAINER_PORT`: Port at which the container binds to the overlaying frontend app.

### Auth Management Vars

- `ACCESS_DURATION`: Number of minutes an Access JWT is valid for before expiration
- `REFRESH_DURATION`: Number of days a Refresh JWT is valid for before expiration
- `ISSUER`: The issuer URI for the JWTs
- `KEY`: The hash key for passwords
