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

# Build and publish for Linux x64
WORKDIR /src/TrainTracking.Web
RUN dotnet publish -c Release -r linux-x64 --self-contained false -o /app/publish

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
# Install ALL possible native dependencies for SkiaSharp/QuestPDF
RUN apt-get update && apt-get install -y \
    libfontconfig1 \
    libfreetype6 \
    fontconfig \
    fonts-liberation \
    libicu72 \
    libx11-6 \
    libglib2.0-0 \
    libssl3 \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app

# Copy published files
COPY --from=build /app/publish .

# Environment variables for Railway
ENV ASPNETCORE_ENVIRONMENT=Production

# Railway automatically sets PORT. ASP.NET Core picks up PORT env and maps to 8080 by default.
# We will use explicit binding in Program.cs as a failsafe.

# Run the application
ENTRYPOINT ["dotnet", "TrainTracking.Web.dll"]
