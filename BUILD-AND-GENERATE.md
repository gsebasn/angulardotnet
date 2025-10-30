# Build and Code Generation Guide

## Current Status

The project is set up with the proper structure for code generation:
- ✅ API structure created (`studyshop-ui/src/app/api/`)
- ✅ Models and Services separated
- ✅ Components updated to use generated code
- ⚠️  Need to export Swagger and regenerate

## Step-by-Step Instructions

### 1. Export Swagger JSON

**Start the API:**
```bash
cd StudyShop.Api
dotnet run
```

**In another terminal, export Swagger:**
```bash
curl http://localhost:5170/swagger/v1/swagger.json > swagger.json
```

Or use the export script:
```bash
node export-swagger.js
```

### 2. Generate TypeScript Services

**From project root:**
```bash
cd studyshop-ui
npm run gen:api
```

This will:
- Read `../swagger.json`
- Generate TypeScript interfaces in `src/app/api/models/`
- Generate service classes in `src/app/api/services/`
- Update the index files

### 3. Build the UI

**From studyshop-ui directory:**
```bash
npm run build
```

Or for development:
```bash
npm start
```

## Manual Alternative

Since the code generation tools may have issues, you can manually verify the structure is correct. The files have been set up with:

```
studyshop-ui/src/app/api/
├── models/
│   ├── Product.ts          ✅ Product, CreateProductDto, UpdateProductDto
│   ├── Order.ts            ✅ Order, OrderItem, CreateOrderDto
│   └── index.ts            ✅ Exports
├── services/
│   ├── ProductsService.ts  ✅ Service methods
│   ├── OrdersService.ts    ✅ Service methods
│   └── index.ts            ✅ Exports
└── index.ts                ✅ Main exports
```

## Next Steps

1. **Verify API structure**: Check that all files exist in `studyshop-ui/src/app/api/`
2. **Start development**: `npm start` in studyshop-ui
3. **Test the app**: Navigate to http://localhost:4200

## Troubleshooting

### Swagger Not Available
- Check API is running: `curl http://localhost:5170/api/products`
- Swagger might be disabled in production mode
- Try: `cd StudyShop.Api && ASPNETCORE_ENVIRONMENT=Development dotnet run`

### Code Generation Fails
- Ensure `swagger.json` exists in project root
- Try: `npm run gen:api:live` to use running API
- Check package.json has correct script

### Build Errors
- Check all imports are correct
- Verify generated files exist
- Run `npm install` to ensure dependencies

