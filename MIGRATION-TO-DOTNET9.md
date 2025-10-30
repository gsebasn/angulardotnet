# Migration Guide: .NET 8 → .NET 9

This guide explains how to upgrade StudyShop from .NET 8 to .NET 9 to take advantage of the latest features and performance improvements.

## 📋 Prerequisites

### 1. Install .NET 9 SDK

Download and install .NET 9 SDK from:
- https://dotnet.microsoft.com/download/dotnet/9.0

Or using the installer:

```bash
# macOS / Linux
brew install --cask dotnet-sdk9

# Or download from Microsoft website
```

### 2. Verify Installation

```bash
dotnet --version
# Should show 9.0.x or higher

dotnet --list-sdks
# Should list 9.0.x SDK
```

## 🚀 Migration Steps

### Step 1: Update Target Framework

**File:** `StudyShop.Api/StudyShop.Api.csproj`

```xml
<PropertyGroup>
  <!-- Change from -->
  <TargetFramework>net8.0</TargetFramework>
  
  <!-- To -->
  <TargetFramework>net9.0</TargetFramework>
</PropertyGroup>
```

### Step 2: Update Package References

**File:** `StudyShop.Api/StudyShop.Api.csproj`

```xml
<ItemGroup>
  <!-- Update EF Core packages -->
  <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="9.0.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="9.0.0" />
  
  <!-- Update MediatR if available -->
  <PackageReference Include="MediatR" Version="12.4.0" />
  
  <!-- Update Swagger -->
  <PackageReference Include="Swashbuckle.AspNetCore" Version="6.8.1" />
</ItemGroup>
```

### Step 3: Update Dockerfile

**File:** `StudyShop.Api/Dockerfile`

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# ...

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
```

### Step 4: Restore and Build

```bash
# Restore packages
dotnet restore StudyShop.Api/StudyShop.Api.csproj

# Build the project
dotnet build StudyShop.Api/StudyShop.Api.csproj

# Run to verify
dotnet run --project StudyShop.Api
```

## ✨ .NET 9 Features to Leverage

### 1. **Enhanced Minimal APIs**

.NET 9 introduces better support for Minimal APIs with improved parameter binding:

```csharp
// Enhanced route parameter validation
productsGroup.MapGet("/{id:int}", async (IMediator mediator, int id) =>
{
    // Automatic validation of route parameters
});
```

### 2. **Performance Improvements**

.NET 9 includes significant performance improvements:

- **15-20% faster JIT compilation**
- **Improved garbage collection**
- **Better memory allocation**
- **Enhanced async/await performance**

### 3. **New Analyzers**

.NET 9 adds new analyzers for:
- Async method recommendations
- LINQ performance improvements
- Memory allocation optimizations

### 4. **Enhanced Expression Trees**

Better support for complex LINQ queries:

```csharp
// More optimized expression trees
var products = await _context.Products
    .Where(p => p.Name.Contains(search))
    .Select(p => new ProductDto { ... })
    .ToListAsync();
```

### 5. **Improved AOT Compilation**

Better ahead-of-time compilation support for smaller deployments:

```xml
<PropertyGroup>
  <PublishAot>true</PublishAot>
</PropertyGroup>
```

### 6. **Enhanced Swagger Support**

Updated Swashbuckle.AspNetCore with better schema generation for Minimal APIs.

### 7. **Better Debugging Experience**

- Improved stepping through async code
- Better exception information
- Enhanced profiling tools

## 🔧 Optional: Leverage New Features

### Enable ReadyToRun for Better Performance

Add to `csproj`:

```xml
<PropertyGroup>
  <PublishReadyToRun>true</PublishReadyToRun>
  <PublishTrimmed>false</PublishTrimmed>
</PropertyGroup>
```

### Use New C# 13 Features (requires .NET 9)

```csharp
// C# 13 - Using alias for any type
using ProductKey = int;

// C# 13 - Improved pattern matching
if (product is { Price: > 100, Stock: > 0 })
{
    // ...
}
```

## 📊 Benefits of Upgrading to .NET 9

| Feature | .NET 8 | .NET 9 |
|---------|--------|--------|
| Performance | Baseline | 15-20% faster |
| Memory Usage | Baseline | 5-10% reduced |
| Cold Start | Baseline | 10-15% faster |
| Bundle Size | Baseline | Smaller with AOT |
| Async Performance | Good | Excellent |

## 🧪 Testing After Migration

### 1. Run All Tests

```bash
dotnet test
```

### 2. Test API Endpoints

```bash
# Get products
curl http://localhost:5170/api/products

# Create product
curl -X POST http://localhost:5170/api/products \
  -H "Content-Type: application/json" \
  -d '{"name":"Test","price":10,"stock":5}'

# Get orders
curl http://localhost:5170/api/orders
```

### 3. Test Swagger UI

```bash
# Open in browser
open http://localhost:5170/swagger
```

### 4. Test Docker Build

```bash
# Build Docker image with .NET 9
docker-compose build

# Run containers
docker-compose up
```

## 🐛 Troubleshooting

### Issue: SDK Not Found

**Error:** `NETSDK1045: The current .NET SDK does not support targeting .NET 9.0`

**Solution:**
```bash
# Install .NET 9 SDK
brew install --cask dotnet-sdk9

# Verify installation
dotnet --version
```

### Issue: Package Version Conflicts

**Error:** Package conflicts during restore

**Solution:**
```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Delete bin/obj folders
rm -rf StudyShop.Api/bin StudyShop.Api/obj

# Restore again
dotnet restore
```

### Issue: EF Core Migrations

**Error:** EF Core schema issues

**Solution:**
```bash
# Delete old migrations if using SQLite
rm -rf StudyShop.Api/Migrations

# Recreate migrations
cd StudyShop.Api
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## 📝 Breaking Changes

### Minimal

.NET 9 is mostly backward compatible with .NET 8. Main considerations:

1. **EF Core 9.0** - Some deprecated APIs removed
2. **ASP.NET Core** - Minor middleware changes
3. **C# 13** - Some new keywords (can use with older versions)

### Recommended Actions

1. Review EF Core migration notes: https://learn.microsoft.com/ef/core/what-is-new/ef-core-9.0/breaking-changes
2. Update any deprecated API calls
3. Test thoroughly before production deployment

## 🔄 Rollback Plan

If issues occur, you can rollback to .NET 8:

1. Revert `csproj` changes
2. Run `dotnet restore`
3. Test thoroughly

## ✅ Migration Checklist

- [ ] Install .NET 9 SDK
- [ ] Update target framework to `net9.0`
- [ ] Update all package references
- [ ] Update Dockerfile
- [ ] Restore packages
- [ ] Build successfully
- [ ] Run application
- [ ] Test all API endpoints
- [ ] Test Swagger UI
- [ ] Test Docker build
- [ ] Review breaking changes
- [ ] Update documentation

## 📚 Additional Resources

- **.NET 9 Release Notes**: https://devblogs.microsoft.com/dotnet/announcing-dotnet-9/
- **EF Core 9.0**: https://learn.microsoft.com/ef/core/what-is-new/ef-core-9.0/
- **ASP.NET Core 9.0**: https://learn.microsoft.com/aspnet/core/release-notes/aspnetcore-9.0
- **C# 13 Features**: https://learn.microsoft.com/dotnet/csharp/whats-new/csharp-13

## 🎯 Summary

Migrating to .NET 9 provides:
- ✅ Better performance (15-20% improvement)
- ✅ Lower memory usage
- ✅ Enhanced debugging experience
- ✅ New C# 13 features
- ✅ Better Minimal API support
- ✅ Improved AOT compilation

The migration is straightforward and mostly backward compatible!

