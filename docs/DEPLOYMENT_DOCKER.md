# Deployment with Docker Compose

This guide provides instructions for deploying the Household Needs Manager application using Docker and Docker Compose.

## Prerequisites

- Docker Engine 20.10 or later
- Docker Compose 2.0 or later
- At least 2GB of free disk space
- Basic knowledge of Docker and command line

## Quick Start

1. Clone the repository
2. Navigate to the project directory
3. Run: `docker-compose up --build`
4. Access the application at `http://localhost:8080`

## Docker Compose Structure

The `docker-compose.yml` file defines two services:

### 1. SQL Server Service (`sqlserver`)
- **Image**: Microsoft SQL Server 2022
- **Purpose**: Database server for the application
- **Port**: 1433 (internal)
- **Data persistence**: Volume mounted to `/var/opt/mssql`

### 2. Web Application Service (`webapp`)
- **Build**: Built from Dockerfile in the project root
- **Purpose**: ASP.NET Core web application
- **Port**: 8080 (mapped to internal 8080)
- **Dependencies**: Depends on `sqlserver` service

## Configuration

### Environment Variables

The application uses the following environment variables (defined in `docker-compose.yml`):

**SQL Server:**
- `ACCEPT_EULA=Y` - Accept SQL Server license agreement
- `SA_PASSWORD` - SQL Server SA password (change in production!)
- `MSSQL_PID=Express` - SQL Server edition (Express for free tier)

**Web Application:**
- `ASPNETCORE_ENVIRONMENT=Production` - ASP.NET Core environment
- `ASPNETCORE_HTTP_PORTS=8080` - Port to listen on
- `ConnectionStrings__DefaultConnection` - Database connection string

### Volume Mounts

- `sqldata:/var/opt/mssql` - Persists SQL Server data between container restarts

## Step-by-Step Deployment

### Step 1: Review docker-compose.yml

Ensure `docker-compose.yml` exists in the project root with proper configuration:

```yaml
services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      ACCEPT_EULA: "Y"
      SA_PASSWORD: "YourStrong@Passw0rd"
      MSSQL_PID: "Express"
    ports:
      - "1433:1433"
    volumes:
      - sqldata:/var/opt/mssql
    networks:
      - householdnet

  webapp:
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "8080:8080"
    depends_on:
      - sqlserver
    environment:
      ASPNETCORE_ENVIRONMENT: "Production"
      ASPNETCORE_HTTP_PORTS: "8080"
      ConnectionStrings__DefaultConnection: "Server=sqlserver;Database=HouseholdNeedsManager;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True"
    networks:
      - householdnet

volumes:
  sqldata:

networks:
  householdnet:
    driver: bridge
```

### Step 2: Review Dockerfile

Ensure `Dockerfile` exists in the project root. It should use a multi-stage build:

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["HouseholdNeedsManager.csproj", "./"]
RUN dotnet restore "HouseholdNeedsManager.csproj"
COPY . .
RUN dotnet build "HouseholdNeedsManager.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HouseholdNeedsManager.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HouseholdNeedsManager.dll"]
```

### Step 3: Build and Run Containers

```bash
# Build and start all services
docker-compose up --build

# Or run in detached mode (background)
docker-compose up --build -d
```

This command will:
1. Build the web application Docker image
2. Pull the SQL Server image
3. Create a Docker network
4. Start both containers
5. Initialize the database

### Step 4: Run Database Migrations

After the containers are running, apply EF Core migrations:

```bash
# Access the webapp container
docker-compose exec webapp bash

# Run migrations
dotnet ef database update

# Exit the container
exit
```

Alternatively, migrations will run automatically if you add this to `Program.cs`:

```csharp
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}
```

### Step 5: Access the Application

Open your web browser and navigate to:
- **HTTP**: `http://localhost:8080`
- **Container IP**: Find IP with `docker inspect`

## Common Commands

### View Running Containers
```bash
docker-compose ps
```

### View Logs
```bash
# All services
docker-compose logs

# Specific service
docker-compose logs webapp
docker-compose logs sqlserver

# Follow logs (real-time)
docker-compose logs -f webapp
```

### Stop Containers
```bash
# Stop without removing
docker-compose stop

# Stop and remove containers
docker-compose down

# Stop and remove containers + volumes (WARNING: deletes database data)
docker-compose down -v
```

### Restart Containers
```bash
# Restart all
docker-compose restart

# Restart specific service
docker-compose restart webapp
```

### Execute Commands in Container
```bash
# Open bash shell in webapp
docker-compose exec webapp bash

# Run a one-off command
docker-compose exec webapp ls -la
```

### Rebuild After Code Changes
```bash
docker-compose up --build
```

## Production Deployment

For production deployment, make these changes:

### 1. Update SA Password
Change `SA_PASSWORD` in `docker-compose.yml` to a strong, unique password:

```yaml
SA_PASSWORD: "$(openssl rand -base64 32)"
```

### 2. Use Environment File
Create a `.env` file (not committed to git):

```env
SA_PASSWORD=YourProductionPassword
DB_NAME=HouseholdNeedsManager
```

Update `docker-compose.yml`:

```yaml
environment:
  SA_PASSWORD: ${SA_PASSWORD}
```

### 3. Enable HTTPS
Add SSL certificate and configure HTTPS:

```yaml
webapp:
  environment:
    ASPNETCORE_URLS: "https://+:443;http://+:80"
    ASPNETCORE_Kestrel__Certificates__Default__Path: "/app/cert.pfx"
    ASPNETCORE_Kestrel__Certificates__Default__Password: "YourCertPassword"
  volumes:
    - ./certs:/app/certs:ro
  ports:
    - "443:443"
    - "80:80"
```

### 4. Use Docker Secrets (for Docker Swarm)
```bash
echo "YourPassword" | docker secret create db_password -
```

### 5. Resource Limits
Add resource constraints:

```yaml
webapp:
  deploy:
    resources:
      limits:
        cpus: '2'
        memory: 2G
      reservations:
        cpus: '0.5'
        memory: 512M
```

## Backup and Restore

### Backup Database
```bash
# Create backup inside container
docker-compose exec sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P 'YourStrong@Passw0rd' \
  -Q "BACKUP DATABASE HouseholdNeedsManager TO DISK='/var/opt/mssql/backup/householdneeds.bak'"

# Copy backup to host
docker cp $(docker-compose ps -q sqlserver):/var/opt/mssql/backup/householdneeds.bak ./backup/
```

### Restore Database
```bash
# Copy backup to container
docker cp ./backup/householdneeds.bak $(docker-compose ps -q sqlserver):/var/opt/mssql/backup/

# Restore
docker-compose exec sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P 'YourStrong@Passw0rd' \
  -Q "RESTORE DATABASE HouseholdNeedsManager FROM DISK='/var/opt/mssql/backup/householdneeds.bak' WITH REPLACE"
```

## Troubleshooting

### Issue: SQL Server fails to start
**Solution**: 
- Ensure SA_PASSWORD meets complexity requirements (8+ chars, uppercase, lowercase, numbers, symbols)
- Check logs: `docker-compose logs sqlserver`
- Increase memory allocation if needed

### Issue: Cannot connect to SQL Server from webapp
**Solution**:
- Verify both containers are on the same network
- Check connection string uses service name `sqlserver` not `localhost`
- Ensure SQL Server is fully started before webapp (use `depends_on` with health check)

### Issue: Port already in use
**Solution**:
- Change the port mapping in `docker-compose.yml`: `"8081:8080"`
- Or stop the conflicting service

### Issue: Database not persisting after restart
**Solution**:
- Ensure volume is defined in `docker-compose.yml`
- Don't use `docker-compose down -v` (removes volumes)

### Issue: Slow performance
**Solution**:
- Increase resource limits in `docker-compose.yml`
- Use production builds, not debug builds
- Enable SQL Server memory optimization

## Monitoring

### Health Checks
Add health checks to `docker-compose.yml`:

```yaml
webapp:
  healthcheck:
    test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
    interval: 30s
    timeout: 3s
    retries: 3
```

### View Resource Usage
```bash
docker stats
```

## Additional Resources

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [SQL Server on Docker](https://docs.microsoft.com/sql/linux/quickstart-install-connect-docker)
- [ASP.NET Core Docker Images](https://hub.docker.com/_/microsoft-dotnet-aspnet)
