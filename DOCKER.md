# Docker Setup for StudyShop

Run the entire application in Docker containers.

## 🚀 Quick Start

### Production Build

```bash
# Build and start all services
docker-compose up --build

# Run in background
docker-compose up -d --build

# View logs
docker-compose logs -f

# Stop all services
docker-compose down
```

**Access:**
- API: http://localhost:5170
- Swagger: http://localhost:5170/swagger
- UI: http://localhost:4200

### Development Mode (with hot-reload)

```bash
# Use development override
docker-compose -f docker-compose.yml -f docker-compose.dev.yml up
```

## 📦 What Gets Built

### Services

1. **api** (StudyShop.Api)
   - .NET 9 Web API with CQRS + Minimal API
   - Exposed on port 5170
   - InMemory database (seeded on startup)
   - Health check enabled

2. **ui** (studyshop-ui)
   - Angular 17 SPA
   - Served by nginx
   - Exposed on port 4200
   - Auto-generates API client from backend

## 🛠️ Docker Commands

### Build only
```bash
docker-compose build
```

### Start services
```bash
docker-compose up
```

### Stop services
```bash
docker-compose down
```

### Rebuild after code changes
```bash
docker-compose up --build --force-recreate
```

### View logs
```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f api
docker-compose logs -f ui
```

### Execute commands in container
```bash
# Enter API container
docker-compose exec api bash

# Enter UI container
docker-compose exec ui sh
```

## 🔧 Configuration

### Environment Variables

**API:**
- `ASPNETCORE_ENVIRONMENT`: `Production` or `Development`
- `ASPNETCORE_URLS`: Server URL binding
- `AllowedOrigins`: CORS allowed origins

**UI:**
- `API_BASE_URL`: Backend API URL (for runtime code generation)

### Volumes

The `docker-compose.yml` includes commented volume mounts for:
- SQLite database persistence (uncomment if switching from InMemory)
- Development hot-reload (in `docker-compose.dev.yml`)

### Networking

Both services run on a shared Docker network (`studyshop-network`) and can communicate using service names:
- API: `http://api:8080`
- UI: `http://ui:80`

## 📝 Development Workflow

### Without Docker (Recommended for development)
```bash
# Terminal 1
./start-backend.sh

# Terminal 2
./start-frontend.sh
```

### With Docker (For testing Docker deployment)

**Development mode:**
```bash
docker-compose -f docker-compose.yml -f docker-compose.dev.yml up --build
```

**Production mode:**
```bash
docker-compose up --build
```

## 🐛 Troubleshooting

### Port already in use
```bash
# Find what's using the port
lsof -i :5170
lsof -i :4200

# Kill the process or change ports in docker-compose.yml
```

### Can't connect to API from UI
- Ensure both services are in the same network
- Check `docker-compose ps` to verify both are running
- View API logs: `docker-compose logs api`
- Check CORS configuration in `Program.cs`

### API client not generated
- The build process runs `npm run gen:api`
- If backend is not ready, it's skipped gracefully
- Check UI logs: `docker-compose logs ui`

### Database data lost
- By default, using InMemory database (data resets on restart)
- Switch to SQLite by uncommenting volume mounts in `docker-compose.yml`
- Or use persistent Docker volumes

## 🔄 Switching to SQLite

1. Edit `docker-compose.yml`:
   ```yaml
   api:
     volumes:
       - ./data:/app/data  # Uncomment this line
   ```

2. Edit `StudyShop.Api/Program.cs` to use SQLite instead of InMemory

3. Rebuild:
   ```bash
   docker-compose down -v  # Remove old volumes
   docker-compose up --build
   ```

## 🚀 Deployment

### Production Deployment

```bash
# Build production images
docker-compose -f docker-compose.yml build

# Tag for registry
docker tag studyshop-api:latest your-registry/studyshop-api:latest
docker tag studyshop-ui:latest your-registry/studyshop-ui:latest

# Push to registry
docker push your-registry/studyshop-api:latest
docker push your-registry/studyshop-ui:latest

# Deploy (example with updated URLs)
docker-compose -f docker-compose.prod.yml up -d
```

## 📊 Resource Usage

View container resource usage:
```bash
docker stats
```

## 🧹 Cleanup

```bash
# Remove containers and volumes
docker-compose down -v

# Remove images
docker-compose down --rmi all

# Full cleanup (removes everything)
docker system prune -a --volumes
```
