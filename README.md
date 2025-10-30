# StudyShop - Full-Stack Learning Project

A simple, clean learning project demonstrating end-to-end API→UI integration with code generation.

## 🎯 Project Goals

- **Backend**: .NET 9 Web API with CQRS + Minimal API, Swagger (OpenAPI v3), EF Core (InMemory/SQLite), FluentValidation
- **Frontend**: Angular 17+ with Material UI and auto-generated typed services from backend's Swagger JSON
- **Code Generation**: One-command script to regenerate TypeScript client from Swagger spec
- **Educational**: Clear structure, comments, and step-by-step instructions

## 🏗️ Architecture

```
StudyShop/
├── StudyShop.Api/          # .NET 9 Web API backend (CQRS + Minimal API)
│   ├── Features/            # Feature-based CQRS organization
│   │   ├── Products/
│   │   │   ├── Commands/    # CreateProduct, UpdateProduct, DeleteProduct
│   │   │   └── Queries/     # GetProducts, GetProductById
│   │   └── Orders/
│   │       ├── Commands/    # CreateOrder
│   │       └── Queries/     # GetOrders, GetOrderById
│   ├── Models/              # Entities (Product, Order, OrderItem)
│   ├── DTOs/                # Data Transfer Objects
│   └── Program.cs           # Minimal API endpoints + configuration
│
└── studyshop-ui/            # Angular 17+ frontend
    ├── src/app/
    │   ├── pages/           # ProductsPage, OrdersPage
    │   └── api/             # Generated TypeScript client (do not edit)
    └── package.json         # npm scripts for code generation
```

## 🚀 Quick Start

### Prerequisites

- **.NET 9 SDK**: [Download](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Node.js 18+**: [Download](https://nodejs.org/)
- **Angular CLI**: `npm install -g @angular/cli`

### Step 1: Start the Backend

```bash
# Navigate to backend
cd StudyShop.Api

# Restore packages and run
dotnet restore
dotnet run

# Backend will be running at http://localhost:5170
# Swagger UI: http://localhost:5170/swagger
```

**Expected output:**
```
✓ Seeded initial products
API:      http://localhost:5170
Swagger:  http://localhost:5170/swagger
```

### Step 2: Start the Frontend

```bash
# Open a NEW terminal (keep backend running)

# Navigate to frontend
cd studyshop-ui

# Install dependencies
npm install

# Generate TypeScript API client from Swagger
npm run gen:api

# Start Angular dev server
npm start

# Frontend will be running at http://localhost:4200
```

### Step 3: Try It Out

1. **Navigate to Products page**
   - View seeded products
   - Search by name
   - Create/Edit/Delete products
   - See validation errors

2. **Create an Order**
   - Go to Orders page
   - Click "New Order"
   - Select products and quantities
   - Submit (server validates stock availability)
   - View order total

## 🐳 Docker Deployment (Alternative)

### Prerequisites

- **Docker Desktop** installed and running
- At least 4GB of RAM available for Docker

### Quick Start with Docker

#### 1. Build and Start All Services

```bash
# From the project root directory
docker-compose up --build
```

This will:
- Build the .NET 9 API image (multi-stage build)
- Build the Angular UI image with Nginx
- Start both containers
- Automatically seed the database

#### 2. Run in Background (Detached Mode)

```bash
docker-compose up -d --build
```

#### 3. Access the Application

Once containers are running:

- **API**: http://localhost:5170 (✅ Currently Running)
- **Angular UI**: http://localhost:4200 (✅ Currently Running)
- **Swagger**: Disabled in production for security

**Verify it's working:**
```bash
# Test API
curl http://localhost:5170/api/products

# Test UI
curl -I http://localhost:4200
```

## 🐳 Complete Docker Compose Commands

### Lifecycle Management

#### Start Services

```bash
# Build and start all services (foreground)
docker-compose up --build

# Build and start in background (detached)
docker-compose up -d --build

# Start without rebuilding (if already built)
docker-compose up

# Start only specific service
docker-compose up api       # Start only API
docker-compose up ui        # Start only UI
```

#### Stop Services

```bash
# Stop containers (keeps containers)
docker-compose stop

# Stop specific service
docker-compose stop api     # Stop only API
docker-compose stop ui      # Stop only UI

# Stop and remove containers (keeps volumes)
docker-compose down

# Stop and remove containers + volumes (⚠️ deletes data)
docker-compose down -v

# Stop, remove containers and images
docker-compose down --rmi all
```

#### Restart Services

```bash
# Restart all services
docker-compose restart

# Restart specific service
docker-compose restart api
docker-compose restart ui

# Restart and rebuild
docker-compose up --build -d

# Restart with fresh containers (recreate)
docker-compose up --force-recreate
```

#### Pause/Unpause

```bash
# Pause all services (keeps resources but stops processing)
docker-compose pause

# Unpause all services
docker-compose unpause

# Pause specific service
docker-compose pause api
```

### Viewing Status and Logs

#### View Running Containers

```bash
# List running containers
docker-compose ps

# List all containers (including stopped)
docker-compose ps -a

# Or use Docker directly
docker ps

# Show container IDs only
docker-compose ps -q
```

#### View Logs

```bash
# View logs from all services (follow mode)
docker-compose logs -f

# View logs from specific service
docker-compose logs -f api    # Backend logs
docker-compose logs -f ui     # Frontend logs

# View last N lines
docker-compose logs --tail=50     # Last 50 lines
docker-compose logs --tail=100    # Last 100 lines
docker-compose logs --tail=1000   # Last 1000 lines

# View logs without follow
docker-compose logs

# View logs with timestamps
docker-compose logs -t

# View logs since timestamp
docker-compose logs --since="2024-01-01T00:00:00"
```

### Rebuilding and Updating

#### Rebuild After Code Changes

```bash
# Rebuild images and restart
docker-compose up --build --force-recreate

# Rebuild without cache (clean build)
docker-compose build --no-cache
docker-compose up

# Rebuild specific service
docker-compose build api    # Rebuild only API
docker-compose build ui     # Rebuild only UI

# Rebuild and start in one command
docker-compose up --build
```

#### Update and Pull Images

```bash
# Pull latest images
docker-compose pull

# Pull and restart
docker-compose pull && docker-compose up -d
```

### Container Management

#### Execute Commands in Containers

```bash
# Enter API container shell
docker-compose exec api sh

# Enter UI container shell
docker-compose exec ui sh

# Run a command in container
docker-compose exec api dotnet --version
docker-compose exec api ls -la
docker-compose exec ui ls -la

# Run command as root
docker-compose exec -u root api sh
```

#### Copy Files to/from Containers

```bash
# Copy file from container to host
docker-compose cp api:/app/file.txt ./file.txt

# Copy file from host to container
docker-compose cp ./file.txt api:/app/file.txt

# Copy directory
docker-compose cp api:/app/data ./backup
```

### Health and Monitoring

#### Check Container Health

```bash
# View health status
docker-compose ps

# Check specific container health
docker inspect studyshop-api --format='{{.State.Health.Status}}'

# View all container stats (live monitoring)
docker stats
```

#### Debug and Troubleshoot

```bash
# View container config
docker-compose config

# Validate compose file
docker-compose config --quiet

# View environment variables
docker-compose exec api env

# Check networking
docker network inspect angulardotnet_studyshop-network
```

### Cleanup Operations

#### Remove Containers and Images

```bash
# Remove stopped containers
docker-compose rm

# Remove containers and volumes
docker-compose down -v

# Remove containers, volumes, and images
docker-compose down -v --rmi all

# Remove everything and clean system
docker-compose down -v --rmi all
docker system prune -a --volumes
```

#### Clean Build

```bash
# Stop and remove everything
docker-compose down -v --rmi all

# Remove build cache
docker builder prune

# Full clean rebuild
docker-compose build --no-cache
docker-compose up -d
```

### Docker Services

The `docker-compose.yml` defines two services:

#### 1. API Service (StudyShop.Api)
- **Image**: .NET 9 runtime with ASP.NET Core
- **Architecture**: CQRS + Minimal API
- **Port**: 5170 → 8080
- **Environment**: 
  - InMemory database (seeded on startup)
  - CORS enabled for UI
- **Health Check**: Included

#### 2. UI Service (studyshop-ui)
- **Image**: Angular 17 + Nginx
- **Port**: 4200 → 80
- **Features**:
  - Pre-built Angular app
  - Served by Nginx
  - API client generated during build

### Docker Image Management

#### List Images

```bash
# View StudyShop images
docker images | grep studyshop
```

#### Remove Images

```bash
# Remove all StudyShop containers and images
docker-compose down --rmi all

# Clean up unused images
docker image prune -a
```

#### View Image Details

```bash
# Inspect image
docker inspect studyshop-api:latest
docker inspect studyshop-ui:latest
```

### Troubleshooting Docker

#### Port Already in Use

```bash
# Find process using port
lsof -i :5170   # API port
lsof -i :4200   # UI port

# Kill process or change ports in docker-compose.yml
```

#### Can't Connect to API from UI

```bash
# Check if both containers are running
docker-compose ps

# Check API logs
docker-compose logs api

# Verify API is responding
curl http://localhost:5170/swagger/v1/swagger.json
```

#### Container Won't Start

```bash
# Check logs for errors
docker-compose logs

# Check container status
docker-compose ps -a

# Remove and recreate
docker-compose down -v
docker-compose up --build
```

#### Out of Disk Space

```bash
# Clean up Docker system
docker system prune -a --volumes

# Remove unused images
docker image prune -a
```

### Development Mode with Hot Reload

For development with hot reload (optional):

```bash
# Use development override
docker-compose -f docker-compose.yml -f docker-compose.dev.yml up
```

This enables:
- Hot reload for API code changes
- Live reload for UI changes
- Source code mounted as volumes

### Production Deployment

#### Build for Production

```bash
# Build production images
docker-compose build

# Tag for registry (replace with your registry)
docker tag studyshop-api your-registry/studyshop-api:v1.0
docker tag studyshop-ui your-registry/studyshop-ui:v1.0

# Push to registry
docker push your-registry/studyshop-api:v1.0
docker push your-registry/studyshop-ui:v1.0
```

#### Pull and Run on Server

```bash
# Pull images
docker pull your-registry/studyshop-api:v1.0
docker pull your-registry/studyshop-ui:v1.0

# Run with production settings
docker-compose -f docker-compose.prod.yml up -d
```

### Docker Compose Files

- **`docker-compose.yml`** - Main production configuration (✅ Currently Running)
- **`docker-compose.dev.yml`** - Development with hot reload
- **`DOCKER.md`** - Detailed Docker documentation

### 🎉 Docker Status

**Current Status:** ✅ **Running Successfully**

- ✅ API container built and running (.NET 9)
- ✅ UI container built and running (Angular 17)
- ✅ API endpoints responding at http://localhost:5170
- ✅ UI serving at http://localhost:4200
- ✅ Products endpoint returning 5 seeded products

**Check status:**
```bash
docker-compose ps
```

**View logs:**
```bash
docker-compose logs -f
```

### Resource Usage

```bash
# Monitor resource usage
docker stats

# Check container resource limits
docker inspect <container-id> | grep -i memory
```

### Complete Cleanup

```bash
# Remove everything (⚠️ all data will be lost)
docker-compose down -v --rmi all
docker system prune -a --volumes
```

## 📚 Key Features

### Backend (.NET 9)

- ✅ **CQRS Pattern** - Commands (writes) and Queries (reads) separated
- ✅ **Minimal API** - Modern .NET 9 endpoint registration
- ✅ **MediatR** - Mediator pattern for CQRS implementation
- ✅ **Swagger/OpenAPI v3** - Full API documentation at `/swagger`
- ✅ **EF Core 9.0** - InMemory database for quick demos (easily switch to SQLite)
- ✅ **FluentValidation** - Server-side validation with clear error messages
- ✅ **CORS** - Configured for Angular on port 4200
- ✅ **ProblemDetails** - RFC 7807 compliant error responses
- ✅ **Data Seeding** - Sample products on startup
- ✅ **Pagination** - Products endpoint supports `skip` and `take`
- ✅ **Feature-based Organization** - All related code in one place

#### API Endpoints

```
GET    /api/products          # List with optional ?search=xxx&skip=0&take=100
GET    /api/products/{id}     # Get single product
POST   /api/products          # Create (validates with FluentValidation)
PUT    /api/products/{id}     # Update (partial update supported)
DELETE /api/products/{id}      # Delete

GET    /api/orders            # List all orders
GET    /api/orders/{id}       # Get single order
POST   /api/orders            # Create (validates stock & computes total)
```

### Frontend (Angular 17)

- ✅ **Standalone Components** - Modern Angular without NgModules
- ✅ **Angular Material** - Pre-built UI components
- ✅ **Auto-generated Client** - TypeScript services from Swagger JSON
- ✅ **Reactive Forms** - Client-side validation with Material
- ✅ **Snackbars** - User feedback for success/error
- ✅ **Search & Pagination** - Products search with real-time filtering

#### Components

- **ProductsPage** - Full CRUD with search
- **OrdersPage** - Create orders with multiple items
- **ProductDialog** - Create/Edit product modal
- **Auto-generated Services** - Typed HttpClient wrappers

## 🔧 Configuration

### Backend (InMemory vs SQLite)

**Default: InMemory** (fast, no setup)

To switch to SQLite with persistence:

1. Edit `StudyShop.Api/Program.cs`:
   ```csharp
   // Comment this line:
   // options.UseInMemoryDatabase("StudyShopDb");
   
   // Uncomment this line:
   options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
   ```

2. Run EF migrations:
   ```bash
   cd StudyShop.Api
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

### Frontend API URL

Edit `studyshop-ui/src/app/environments/environment.ts` to change API base URL.

### Code Generation

Regenerate TypeScript client after backend changes:

```bash
cd studyshop-ui
npm run gen:api
```

This runs `openapi-typescript-codegen` which:
1. Reads `/swagger/v1/swagger.json` from backend
2. Generates TypeScript models and services
3. Outputs to `src/app/api/`

## 📖 Learning Points

1. **Swagger Integration**
   - Auto-generated OpenAPI spec from Minimal API endpoints
   - XML comments become Swagger documentation
   - JSON schema for code generation

2. **Code Generation Workflow**
   - Backend defines API contract (Swagger)
   - Frontend generates client (TypeScript)
   - Type safety end-to-end

3. **Validation**
   - Client-side: Angular Reactive Forms
   - Server-side: FluentValidation
   - Consistent error messages

4. **API Design**
   - RESTful endpoints
   - Proper HTTP status codes (201, 400, 404, 500)
   - ProblemDetails for errors (RFC 7807)

## 🧪 Testing the Application

### Products Workflow

1. View list of 5 seeded products
2. Search for "Laptop"
3. Create new product:
   - Name: "SSD Drive"
   - Price: 79.99
   - Stock: 25
4. Edit "USB-C Cable" price to 9.99
5. Delete "USB-C Cable"

### Orders Workflow

1. Navigate to Orders page (should be empty)
2. Click "New Order"
3. Add 2 items:
   - Product: Laptop Computer, Quantity: 1
   - Product: Wireless Mouse, Quantity: 2
4. Submit
5. View order details (total should be $1059.97)
6. Try ordering more stock than available (error message appears)

## 📝 Swagger Documentation

Access the interactive API documentation at:
**http://localhost:5170/swagger**

- Browse all endpoints
- See request/response schemas
- Try endpoints directly in browser
- Download OpenAPI JSON for code generation

## 🔍 Troubleshooting

### Backend won't start
- Check .NET 9 SDK is installed: `dotnet --version`
- Ensure port 5170 is not in use
- Check console output for errors
- See [MIGRATION-TO-DOTNET9.md](MIGRATION-TO-DOTNET9.md) if upgrading

### Frontend won't generate API client
- Ensure backend is running at http://localhost:5170
- Run manually: `npm run gen:api`
- Check `src/app/api/` directory is created

### CORS errors in browser console
- Verify `Program.cs` has CORS configured for `http://localhost:4200`
- Clear browser cache and reload

### Products not showing
- Check backend seeded data in console output
- Verify API at http://localhost:5170/api/products returns data
- Check browser Network tab for failed requests

## 🎓 Next Steps

- ✅ **Upgraded to .NET 9** - Using latest features and performance improvements
- ✅ **CQRS Pattern** - Commands and Queries implemented
- ✅ **Minimal API** - Modern endpoint registration
- Add authentication (JWT tokens)
- Add pagination for orders
- Implement user roles
- Add unit tests for command/query handlers
- Add Angular component tests
- Deploy to cloud (Azure, AWS, etc.)

## 📚 Additional Documentation

- **[SWAGGER-EXPORT.md](SWAGGER-EXPORT.md)** - Guide for exporting Swagger JSON and generating TypeScript services
- **[MIGRATION-TO-DOTNET9.md](MIGRATION-TO-DOTNET9.md)** - Migration guide from .NET 8 to .NET 9
- **[CQRS-REFACTOR.md](StudyShop.Api/CQRS-REFACTOR.md)** - CQRS architecture details
- **[DOCKER.md](DOCKER.md)** - Docker deployment guide
- **[BUILD-STATUS.md](BUILD-STATUS.md)** - Current build status
- **[DOCKER-SUCCESS.md](DOCKER-SUCCESS.md)** - Docker deployment status

## 📄 License

This is a learning project. Feel free to use and modify as needed.

## 🤝 Contributing

This is a self-contained learning resource. Experiment freely!

