# =========================
# Build stage
# =========================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy the project file first
# This allows Docker to cache the restore layer
COPY ["src/SavingsTracker.Api/SavingsTracker.Api.csproj", "src/SavingsTracker.Api/"]

# Restore NuGet packages
RUN dotnet restore "src/SavingsTracker.Api/SavingsTracker.Api.csproj"

# Copy the rest of the source code
COPY . .

# Build and publish the application
RUN dotnet publish "src/SavingsTracker.Api/SavingsTracker.Api.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false


# =========================
# Runtime stage
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

# Render uses port 10000 by default
ENV ASPNETCORE_URLS=http://+:10000

# Copy published application from build stage
COPY --from=build /app/publish .

# Start the API
ENTRYPOINT ["dotnet", "SavingsTracker.Api.dll"]