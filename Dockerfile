# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY TrainTracking.sln .
COPY TrainTracking.Domain/*.csproj TrainTracking.Domain/
COPY TrainTracking.Application/*.csproj TrainTracking.Application/
COPY TrainTracking.Infrastructure/*.csproj TrainTracking.Infrastructure/
COPY TrainTracking.Web/*.csproj TrainTracking.Web/

# Restore dependencies
RUN dotnet restore

# Copy all source code
COPY . .

# Build and publish
WORKDIR /src/TrainTracking.Web
RUN dotnet publish -c Release -o /app/publish

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published files
COPY --from=build /app/publish .

# Expose port
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
ENTRYPOINT ["dotnet", "TrainTracking.Web.dll"]
