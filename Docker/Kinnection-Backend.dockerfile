FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /Kinnection

# Copy backend files into layer
COPY Kinnection-Backend/Kinnection.sln .
COPY Kinnection-Backend/app ./app/
COPY Kinnection-Backend/test ./test/

# Restore as distinct layers
RUN dotnet restore

# Build and publish a release for the container to run
RUN dotnet publish -o /Kinnection/out

# Create runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /Kinnection

# Copy in published release into runtime image 
COPY --from=build /Kinnection/out .

# Start the application from the release files
ENTRYPOINT ["dotnet", "Kinnection-Backend.dll"]