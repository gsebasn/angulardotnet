# Quick Swagger Export and Code Generation

## One-Command Solution

Run this script to complete all steps:

```bash
./generate-api-and-build.sh
```

This script will:
1. ✅ Check if API is running
2. ✅ Export Swagger JSON
3. ✅ Generate TypeScript services
4. ✅ Build the Angular UI

## Manual Steps (Alternative)

If the script doesn't work, follow these steps manually:

### Step 1: Start API in Development Mode

```bash
cd StudyShop.Api
ASPNETCORE_ENVIRONMENT=Development dotnet run
```

Keep this running in a terminal.

### Step 2: Export Swagger

In a new terminal:

```bash
curl http://localhost:5170/swagger/v1/swagger.json > swagger.json
```

### Step 3: Generate TypeScript Services

```bash
cd studyshop-ui
npm run gen:api
```

### Step 4: Build the UI

```bash
npm run build
```

Or for development:

```bash
npm start
```

## Troubleshooting

### "Swagger not available"
- API is running in Production mode
- Restart with: `ASPNETCORE_ENVIRONMENT=Development dotnet run`

### "Code generation fails"
- Ensure `swagger.json` exists in project root
- Check: `ls -lh swagger.json`

### "Build errors"
- Run `npm install` in studyshop-ui
- Check for TypeScript errors

## Verify Results

After completion, check:

```bash
# Swagger JSON
ls -lh swagger.json

# Generated API files
ls -la studyshop-ui/src/app/api/

# Build output
ls -la studyshop-ui/dist/
```

