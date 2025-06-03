# Kinnection

Kinnection is a web application that will allow the user to track their family tree. Using this system, the user will be able to:

- Sign up or login via custom authentication system with 2FA
- Create, modify, and remove family trees through a dashboard UI
- Create, modify, and remove family members in family trees through a tree-specific UI
- Share a created family tree via view-only share URL (when enabled by the user)

## Composition

- `Docker/`: Docker related files
- `Kinnection-Database/`: MySQL database related static files and data
- `Kinnection-Backend/`: ASP.NET Core Web API Backend App
- `Kinnection-Frontend/`: Angular Frontend App

## Running the App

### Required Software

- C#/.NET SDK 8.0+
  - The C#/.NET SDK is required because the app requires at least one migration file to start up. Please follow the instructions under the [Migrations](#migrations) section to create one.
- Docker Desktop (or just the Docker engine)
- SendGrid API Key and Single Sender email address
  - More details in the [Auth Management Vars](#auth-management-vars) section.

### Running in Development Environment

- Ensure all environment variables are filled in in the `.env` file in `Kinnection/Docker`.
- Navigate to the `Kinnection/Docker` directory.
- `docker compose up -d --build`:
  - Builds and brings up the containers
- `docker compose down <-v>`:
  - Tears down the containers
  - `-v` will also remove any associated container volumes.
    - Only use this argument when you need a clean slate as it removes all associated permant data storage in the volumes.

### Running Local Development Environment

- Note: Currently only set up for frontend development this way. To use the backend alongside the locally-hosted frontend, bring up the containers.
- Navigate to the `Kinnection/Kinnection-Frontend/src/environments` directory.
- Enable file scripting permissions and set the environment files:
  - Windows:
    - `Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process`
    - `./SetEnvVars.ps1 -e "../../../Docker/.env"`
  - MAC/Linux:
    - `chmod +x /path/to/GetEnvVars.sh`
    - `./SetEnvVars.sh -e "../../../Docker/.env"`
  - Note: the environment file must be specified as `SetEnvVars` by default targets filepaths meant for use in the docker containers. They differ from the local development paths.
- Navigate back to `Kinnection/Kinnection-Frontend`.
- Start the frontend with `npm start`.

## Running the tests

### Running Locally

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

- `MYSQL_CONTAINER_PORT`: Port at which the container internally binds to the MySQL database.
- `MYSQL_DATABASE`: Name of the database.
- `MYSQL_EXTERNAL_PORT`: Port at which the MySQL database is bound to your host machine and can be accessed.
- `MYSQL_HOST`: Name of the host connection for the database.
- `MYSQL_PASSWORD`: Password of the user.
- `MYSQL_PROTOCOL`: Protocol for connecting to the database. For MySQL, it is `mysql`.
- `MYSQL_ROOT_PASSWORD`: Root password for the database.
- `MYSQL_USER`: User of the database.

### ASP.NET Core Container Vars

- `ASP_CONTAINER_PORT`: Port at which the container internally binds to the ASP.NET Core webapi app.
- `ASP_EXTERNAL_PORT`: Port at which the ASP.NET Core webapi app is bound to your host machine and can be accessed.
- `ASPNETCORE_ENVIRONMENT`: Environment in which the ASP.NET Core webapi app should assume. (`Development`, `Staging`, `Production`)

### Database Management Vars

- `APPLY_MIGRATIONS`: Whether to auto-apply migrations on startup of the ASP.NET Core app. 1 for True, 0 for False.
- `RETRY_ATTEMPTS`: Number of times the ASP.NET Core webapi app reattempts to connection to the database
- `RETRY_IN`: Number of seconds the ASP.NET Core webapi app waits between attempts

### Angular Container Vars

- `ANG_CONTAINER_PORT`: Port at which the container internally binds to the Angular frontend app.
- `ANG_EXTERNAL_PORT`: Port at which the Angular frontend app is bound to your host machine and can be accessed.

### Auth Management Vars

- `ACCESS_DURATION`: Number of minutes an Access JWT is valid for before expiration
- `ENC_KEY_DUR`: Number of days the encryption keys used for password E2EE are valid for, then rotated after.
- `FROM_EMAIL`: The email address to send emails from via SendGrid.
  - To create one after you have a SendGrid account, go to `Settings > Sender Authentication` and click `Verify a Single Sender` to set up an email to be used for sending emails to the client.
- `FROM_NAME`: The name of the sender in emails sent via SendGrid.
- `HASHING_KEY`: The hash key for passwords
- `ISSUER`: The issuer URI for the JWTs
- `REFRESH_DURATION`: Number of days a Refresh JWT is valid for before expiration
- `SENDGRID_API_KEY`: The API key provided by SendGrid for their emailing services.
  - After creating an account, got to `Settings > API Keys` and click `Create API Key` to set up a usable key.

### Local Development and Testing Vars

- `ANG_PORT_LOCAL`: Port of the Angular app when developing locally.
- `MANUAL_EMAIL_VERIFICATION`: Address to be sent emails to for manual verification emailing service is functional. This is NOT the sender email.
- `MYSQL_EXTERNAL_HOST`: Name of the local host connection of the database.
