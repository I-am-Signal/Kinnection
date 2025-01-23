FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /App

# Copy backend files into layer
COPY Kinnection-Backend/Kinnection.sln .
COPY Kinnection-Backend/api ./api/

# Restore as distinct layers
RUN dotnet restore

# Build and publish a release for the container to run
RUN dotnet publish -o /App/out

# Create runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 as runtime
WORKDIR /App

# Copy in published release into runtime image 
COPY --from=build /App/out .

# Start the application from the release files
ENTRYPOINT ["dotnet", "Kinnection-Backend.dll"]