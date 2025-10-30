# Swagger Export Guide

This guide explains how to export Swagger/OpenAPI documentation from the .NET API and use it for TypeScript code generation in Angular.

## Quick Start

### 1. Export Swagger JSON

Make sure the API is running (`dotnet run --project StudyShop.Api`), then:

**Option A: Node.js script (Cross-platform)**
```bash
node export-swagger.js
```

**Option B: PowerShell (Windows)**
```bash
powershell -ExecutionPolicy Bypass -File StudyShop.Api/Scripts/Export-Swagger.ps1
```

**Option C: Bash script (Linux/Mac)**
```bash
bash StudyShop.Api/Scripts/export-swagger.sh
```

**Option D: Manual export**
```bash
# While API is running
curl http://localhost:5170/swagger/v1/swagger.json > swagger.json
```

This creates `swagger.json` in the project root.

### 2. Generate TypeScript Services

After exporting the Swagger JSON:

```bash
cd studyshop-ui
npm run gen:api
```

This generates TypeScript services in `studyshop-ui/src/app/api/`.

## Workflow

### Development Workflow

1. **Start the API**
   ```bash
   cd StudyShop.Api
   dotnet run
   ```

2. **Make API changes** (add endpoints, modify DTOs, etc.)

3. **Export updated Swagger**
   ```bash
   node export-swagger.js
   ```

4. **Generate TypeScript client**
   ```bash
   cd studyshop-ui
   npm run gen:api
   ```

5. **Use the generated services** in your Angular components

### Using Live API (Alternative)

If you prefer to generate directly from the running API:

```bash
cd studyshop-ui
npm run gen:api:live
```

⚠️ **Note**: Requires the API to be running on `http://localhost:5170`

## File Structure

```
AngularDotNet/
├── swagger.json                    # Exported Swagger JSON (generated)
├── export-swagger.js               # Node.js export script
├── StudyShop.Api/
│   ├── Scripts/
│   │   ├── Export-Swagger.ps1     # PowerShell export script
│   │   └── export-swagger.sh      # Bash export script
│   └── ...
└── studyshop-ui/
    ├── src/app/api/                # Generated TypeScript services
    └── package.json                # npm scripts for code generation
```

## Generated Files

When you run `npm run gen:api`, it generates:

```
studyshop-ui/src/app/api/
├── services/
│   ├── ProductsService.ts         # Product API operations
│   └── OrdersService.ts            # Order API operations
├── models/
│   ├── ProductDto.ts              # Product DTO type
│   ├── OrderDto.ts                # Order DTO type
│   └── ...
└── core/
    └── ...
```

## Updating Services After API Changes

1. **Update API** (add/modify endpoints in `StudyShop.Api`)
2. **Restart API** (if needed)
3. **Export Swagger**: `node export-swagger.js`
4. **Regenerate TypeScript**: `cd studyshop-ui && npm run gen:api`
5. **Update components** to use newly generated service methods

## Troubleshooting

### "API is not running"
- Make sure the API is running: `dotnet run --project StudyShop.Api`
- Check the API is accessible: `curl http://localhost:5170/api/products`

### "swagger.json not found"
- Run the export script first: `node export-swagger.js`
- Check the file exists in project root

### "Generated services are outdated"
- Export fresh Swagger JSON: `node export-swagger.js`
- Regenerate: `cd studyshop-ui && npm run gen:api`

## Integration with CI/CD

For automated builds, you can:

1. **Build API** → Generate Swagger during build
2. **Export Swagger** → Save to artifact
3. **Generate TypeScript** → During frontend build
4. **Build Angular** → Use generated services

Example:
```yaml
# GitHub Actions / CI example
- name: Export Swagger
  run: |
    dotnet run --project StudyShop.Api &
    sleep 10
    node export-swagger.js
    
- name: Generate TypeScript Services
  run: |
    cd studyshop-ui
    npm run gen:api
```

## Manual Export

If scripts don't work, you can manually export:

```bash
# From project root
curl http://localhost:5170/swagger/v1/swagger.json > swagger.json
```

Then verify and format:
```bash
cat swagger.json | jq . > swagger.json
```

