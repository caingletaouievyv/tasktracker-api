# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy csproj and restore
COPY *.sln .
COPY TaskTrackerApi/*.csproj ./TaskTrackerApi/
RUN dotnet restore

# Copy the full source and publish
COPY . .
WORKDIR /app/TaskTrackerApi
RUN dotnet publish -c Release -o out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/TaskTrackerApi/out .
ENTRYPOINT ["dotnet", "TaskTrackerApi.dll"]
