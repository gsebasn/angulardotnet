# Docker Setup Summary

Complete Docker setup for StudyShop - full documentation in [DOCKER.md](DOCKER.md).

## 🎯 Quick Start

```bash
# Build and start all services
docker-compose up --build

# Access:
# - API: http://localhost:5170
# - Swagger: http://localhost:5170/swagger  
# - UI: http://localhost:4200
```

## 📦 What's Included

### Files Created

1. **docker-compose.yml** - Main orchestration file
2. **StudyShop.Api/Dockerfile** - .NET 8 API container
3. **studyshop-ui/Dockerfile** - Angular + Nginx container
4. **studyshop-ui/nginx.conf** - Nginx configuration for SPA
5. **.dockerignore** - Docker build exclusions
6. **DOCKER.md** - Full documentation

### Configuration

✅ **API Service**
- .NET 8 runtime
- Port: 5170 → 8080
- InMemory database (seeded on startup)
- CORS configured for UI
- Health check enabled

✅ **UI Service**
- Angular 17 build
- Served by Nginx
- Port: 4200 → 80
- Auto-generates API client during build
- SPA routing support

✅ **Networking**
- Shared Docker network
- Services communicate by name
- External access on localhost

## 🔧 Customization

### Change Ports

Edit `docker-compose.yml`:
```yaml
api:
  ports:
    - "8080:8080"  # Change external port

ui:
  ports:
    - "3000:80"  # Change external port
```

### Use SQLite Instead of InMemory

1. Edit `StudyShop.Api/Program.cs` to use SQLite
2. Uncomment volume mount in `docker-compose.yml`:
   ```yaml
   volumes:
     - ./data:/app/data
   ```

### Add Environment Variables

```yaml
services:
  api:
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - CustomVar=Value
```

## 📊 Commands

```bash
# Start services
docker-compose up

# Start in background
docker-compose up -d

# Rebuild after code changes
docker-compose up --build --force-recreate

# View logs
docker-compose logs -f

# Stop services
docker-compose down

# Stop and remove volumes
docker-compose down -v
```

## 🐛 Troubleshooting

### Build fails
```bash
# Clean and rebuild
docker-compose down -v
docker-compose build --no-cache
docker-compose up
```

### API not accessible
```bash
# Check API is running
docker-compose ps

# Check API logs
docker-compose logs api

# Test API directly
curl http://localhost:5170/swagger/v1/swagger.json
```

### UI shows "Cannot connect to API"
- Verify API is running: `docker-compose ps api`
- Check API logs: `docker-compose logs api`
- Verify CORS configuration in `Program.cs`
- Check browser console for errors

## 🚀 Production Deployment

### Build for Production

```bash
# Build images
docker-compose build

# Tag for registry
docker tag studyshop-api your-registry/studyshop-api:v1.0
docker tag studyshop-ui your-registry/studyshop-ui:v1.0

# Push to registry
docker push your-registry/studyshop-api:v1.0
docker push your-registry/studyshop-ui:v1.0
```

### Deploy to Server

```bash
# On your server, create docker-compose.prod.yml
# Update environment variables for production URLs

docker-compose -f docker-compose.prod.yml up -d
```

## 📝 Notes

- **Data persistence**: Use SQLite with volume mounts for production
- **API Client Generation**: Happens during `npm install` in UI container
- **Health Checks**: Monitors container health automatically
- **Networking**: Services communicate via Docker network (not localhost)

For full documentation, see [DOCKER.md](DOCKER.md).

