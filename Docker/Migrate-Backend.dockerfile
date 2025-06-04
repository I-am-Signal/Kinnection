FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /Kinnection

RUN apt-get update && apt-get install -y bash default-mysql-client

# Copy backend files
COPY Kinnection-Backend/Kinnection.sln .
COPY Kinnection-Backend/app ./app/
COPY Kinnection-Backend/test ./test/

# Restore
RUN dotnet restore

# Install EF globally
RUN dotnet tool install --global dotnet-ef

ENV PATH="$PATH:/root/.dotnet/tools"

WORKDIR /Kinnection/app

# Copy migration script
COPY Kinnection-Backend/app/migrate.sh ./migrate.sh
RUN chmod +x ./migrate.sh

# Create the migration
ENTRYPOINT [ "./migrate.sh" ]