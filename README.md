# Kinnection
Kinnection is (going to be) a web application that will allow the user to track their family tree. Using this system, the user will be able to:
- Login (via direct login or Google Sign-On, potentially both later)
- CRUD Family trees
- CRUD Family members in family trees

## Composition
* `Docker/`: Docker related files
* `Kinnection-Database/`: MySQL database related static files and data
* `Kinnection-Backend/`: ASP.NET Core Web API Backend App
* `Kinnection-Frontend/`: Angular Frontend App

## Environment Variables

The `.env.template` file contains a template of the `.env` file that should be placed in the `Docker/` directory. In it are the following variables:

### MySQL Container Vars
* `MYSQL_PROTOCOL`: Protocol for connecting to the database. For MySQL, it is `mysql`.
* `MYSQL_PORT`: Port at which the database can be accessed.
* `MYSQL_CONTAINER_PORT`: Port at which the container binds to the overlaying database.
* `MYSQL_ROOT_PASSWORD`: Root password for the database .
* `MYSQL_DATABASE`: Name of the database.
* `MYSQL_USER`: User of the database.
* `MYSQL_PASSWORD`: Password of the user.
* `MYSQL_HOST`: Name of the host connection for the database.

### ASP.NET Core Container Vars
* `ASPNETCORE_ENVIRONMENT`: Environment in which the ASP.NET Core webapi app should assume. (`Development`, `Staging`, `Production`)
* `ASP_PORT`: Port at which the ASP.NET Core webapi app can be accessed.
* `ASP_CONTAINER_PORT`: Port at which the container binds to the overlaying webapi app.

### Database Connection Vars
* `RETRY_ATTEMPTS`: Number of times the ASP.NET Core webapi app reattempts to connection to the database
* `RETRY_IN`: Number of seconds the ASP.NET Core webapi app waits between attempts

### Angular Container Vars
* `ANG_PORT`: Port at which the Angular frontend app can be accessed.
* `ANG_CONTAINER_PORT`: Port at which the container binds to the overlaying frontend app.