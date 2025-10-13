# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy csproj and restore
COPY TaskTrackerApi/TaskTrackerApi.csproj TaskTrackerApi/
RUN dotnet restore TaskTrackerApi/TaskTrackerApi.csproj

# Copy the rest of the files
COPY . .
WORKDIR /app/TaskTrackerApi
RUN dotnet publish -c Release -o /out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /out .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "TaskTrackerApi.dll"]

