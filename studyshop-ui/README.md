# StudyShop UI - Angular Frontend

Angular 17+ frontend for the StudyShop project with auto-generated API clients.

## Setup

```bash
# Install dependencies
npm install

# Make sure backend is running at http://localhost:5170
# (run this from the StudyShop.Api directory first)

# Generate TypeScript API client from Swagger
npm run gen:api

# Start development server
npm start

# App will be available at http://localhost:4200
```

## Features

- **Products Management**
  - View, create, edit, delete products
  - Search by name
  - Material UI table with validation

- **Orders**
  - Create orders with multiple items
  - Product selection dropdown
  - Client-side total calculation
  - Server-side stock validation

## Code Generation

The TypeScript API client is generated from the Swagger/OpenAPI specification.

### Workflow

1. **Export Swagger JSON** (from project root):
   ```bash
   # Make sure API is running first
   node export-swagger.js
   ```

2. **Generate TypeScript Services**:
   ```bash
   npm run gen:api
   ```

3. **Alternative: Generate from Live API**:
   ```bash
   npm run gen:api:live  # Requires API running on http://localhost:5170
   ```

**Note**: See the main [SWAGGER-EXPORT.md](../SWAGGER-EXPORT.md) guide for detailed instructions.

Generated code is in `src/app/api/` - **do not edit manually**.

## Project Structure

```
src/
├── app/
│   ├── pages/
│   │   ├── products-page/      # Products CRUD
│   │   └── orders-page/        # Order creation
│   ├── api/                     # Generated (from Swagger)
│   ├── environments/           # API configuration
│   ├── app.component.ts        # Main app component
│   └── app.routes.ts           # Routing
├── index.html
└── styles.scss                  # Global styles
```

## Development

### Build
```bash
npm run build
```

### Serve
```bash
npm start
```

### Watch
```bash
npm run watch
```

## Dependencies

- @angular/core ^17.0.0
- @angular/material ^17.0.0
- openapi-typescript-codegen ^0.25.0

