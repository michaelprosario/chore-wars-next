# ChoreWars Docker Setup

This guide explains how to run ChoreWars using Docker and Docker Compose.

## Prerequisites

- [Docker](https://docs.docker.com/get-docker/) (20.10 or later)
- [Docker Compose](https://docs.docker.com/compose/install/) (v2.0 or later)

## Quick Start

### Production Mode

1. **Build and start the application:**
   ```bash
   docker-compose up -d
   ```

2. **Access the application:**
   - Open your browser to http://localhost:5287

3. **View logs:**
   ```bash
   docker-compose logs -f chorewars-web
   ```

4. **Stop the application:**
   ```bash
   docker-compose down
   ```

### Development Mode

1. **Run in development mode:**
   ```bash
   docker-compose -f docker-compose.dev.yml up -d
   ```

2. **Access the application:**
   - Open your browser to http://localhost:5287

## Database Persistence

The SQLite database is persisted using Docker volumes:
- **Production**: Database stored in `./data/chorewars.db`
- **Development**: Database stored in `./data/chorewars.db`

### Initializing the Database

If you're starting fresh or need to run migrations:

```bash
# Enter the container
docker exec -it chorewars-web bash

# Run migrations (if needed)
dotnet ef database update
```

Or create the data directory and copy your existing database:

```bash
mkdir -p data
cp src/ChoreWars.Web/chorewars.db data/
```

## Docker Commands Reference

### Building

```bash
# Build the image
docker-compose build

# Build without cache
docker-compose build --no-cache

# Build for a specific service
docker-compose build chorewars-web
```

### Running

```bash
# Start services in background
docker-compose up -d

# Start services with logs visible
docker-compose up

# Start specific service
docker-compose up chorewars-web
```

### Monitoring

```bash
# View logs
docker-compose logs

# Follow logs in real-time
docker-compose logs -f

# View logs for specific service
docker-compose logs chorewars-web

# Check container status
docker-compose ps
```

### Stopping and Cleaning Up

```bash
# Stop services
docker-compose stop

# Stop and remove containers
docker-compose down

# Stop, remove containers, and remove volumes
docker-compose down -v

# Remove all (containers, networks, images, volumes)
docker-compose down --rmi all -v
```

### Maintenance

```bash
# Restart a service
docker-compose restart chorewars-web

# Execute command in running container
docker exec -it chorewars-web bash

# View container resource usage
docker stats chorewars-web
```

## Environment Variables

You can customize the application by setting environment variables in `docker-compose.yml`:

- **ASPNETCORE_ENVIRONMENT**: Set to `Development`, `Staging`, or `Production`
- **ASPNETCORE_URLS**: Configure listening URLs
- **ConnectionStrings__DefaultConnection**: Database connection string
- **Logging__LogLevel__Default**: Set logging level

Example:

```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Production
  - ConnectionStrings__DefaultConnection=Data Source=/app/data/chorewars.db
```

## Health Checks

The application includes a health check endpoint:
- **URL**: `http://localhost:5287/health`
- **Interval**: 30 seconds
- **Timeout**: 3 seconds
- **Retries**: 3

Check health status:

```bash
docker inspect --format='{{.State.Health.Status}}' chorewars-web
```

## Troubleshooting

### Port Already in Use

If port 5287 is already in use, modify the port mapping in `docker-compose.yml`:

```yaml
ports:
  - "5288:8080"  # Use port 5288 instead
```

### Database Permission Issues

If you encounter database permission errors:

```bash
# Fix permissions on the data directory
chmod -R 777 data/

# Or set specific ownership
chown -R 1000:1000 data/
```

### Container Won't Start

1. Check logs:
   ```bash
   docker-compose logs chorewars-web
   ```

2. Verify the image built successfully:
   ```bash
   docker images | grep chorewars
   ```

3. Rebuild without cache:
   ```bash
   docker-compose build --no-cache
   ```

### Database Not Persisting

Ensure the volume is properly configured:

```bash
# Check volumes
docker volume ls

# Inspect the volume
docker volume inspect chorewars_chorewars-data
```

## Production Deployment

For production deployment:

1. **Update environment variables** in `docker-compose.yml`
2. **Secure the database** with proper file permissions
3. **Use a reverse proxy** (nginx, Caddy) for HTTPS
4. **Set up backups** for the database volume
5. **Monitor logs** and set up log rotation

### Example Nginx Reverse Proxy

```nginx
server {
    listen 80;
    server_name chorewars.example.com;

    location / {
        proxy_pass http://localhost:5287;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

## Performance Optimization

### Multi-stage Build

The Dockerfile uses a multi-stage build:
- **Build stage**: Uses `dotnet/sdk:10.0` (~1.5GB) for compilation
- **Runtime stage**: Uses `dotnet/aspnet:10.0` (~200MB) for running

This results in a much smaller final image (~230MB vs ~1.5GB).

### Image Size

Check image size:

```bash
docker images chorewars-chorewars-web
```

Typical size: ~220-250 MB

## Security Considerations

The Docker configuration includes several security best practices:

1. **Non-root user**: Application runs as `chorewars` user (not root)
2. **Minimal base image**: Uses official Microsoft ASP.NET runtime
3. **Read-only source mounts**: Development volumes are mounted read-only
4. **Health checks**: Automatic container health monitoring
5. **Network isolation**: Uses dedicated bridge network

## Backup and Restore

### Backup Database

```bash
# Create backup
docker exec chorewars-web tar -czf /tmp/backup.tar.gz /app/data
docker cp chorewars-web:/tmp/backup.tar.gz ./backup-$(date +%Y%m%d).tar.gz
```

### Restore Database

```bash
# Stop the application
docker-compose down

# Restore data directory
tar -xzf backup-20240101.tar.gz -C ./

# Start the application
docker-compose up -d
```

## Additional Resources

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [ASP.NET Core in Docker](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/)
- [.NET Docker Samples](https://github.com/dotnet/dotnet-docker/tree/main/samples)
