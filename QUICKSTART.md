# StudyShop - Quick Start Guide

## 🚀 Get Running in 3 Steps

### 1️⃣ Start Backend

Open terminal 1:

```bash
./start-backend.sh
```

Or manually:
```bash
cd StudyShop.Api
dotnet restore
dotnet run
```

✅ Backend running at **http://localhost:5170**  
✅ Swagger UI at **http://localhost:5170/swagger**

### 2️⃣ Start Frontend

Open terminal 2:

```bash
./start-frontend.sh
```

Or manually:
```bash
cd studyshop-ui
npm install          # First time only
npm run gen:api      # Generate TypeScript client from Swagger
npm start
```

✅ Frontend running at **http://localhost:4200**

### 3️⃣ Try It Out

1. Open http://localhost:4200
2. Go to **Products** page
3. Create a new product, edit existing, or delete
4. Go to **Orders** page
5. Create an order with multiple products
6. Watch stock validation work!

## 📁 Project Structure

```
.
├── README.md                  # Full documentation
├── QUICKSTART.md              # This file
├── start-backend.sh           # Quick backend launcher
├── start-frontend.sh          # Quick frontend launcher
│
├── StudyShop.Api/             # .NET 9 Backend (CQRS + Minimal API)
│   ├── Features/              # CQRS commands & queries
│   │   ├── Products/          # Products Commands & Queries
│   │   └── Orders/            # Orders Commands & Queries
│   ├── Models/                # Product, Order entities
│   ├── DTOs/                  # Data transfer objects
│   └── Program.cs             # Minimal API endpoints
│
└── studyshop-ui/              # Angular 17 Frontend
    ├── src/app/
    │   ├── pages/
    │   │   ├── products-page/ # Products CRUD
    │   │   └── orders-page/   # Orders creation
    │   └── api/               # Generated API client
    └── package.json           # npm scripts
```

## 🎯 What You Get

### Backend (C# / .NET 9)
- ✅ CQRS pattern with MediatR
- ✅ Minimal API endpoints
- ✅ Swagger/OpenAPI documentation
- ✅ FluentValidation for request validation
- ✅ EF Core 9.0 with InMemory database (5 seeded products)
- ✅ CORS configured for Angular
- ✅ Proper HTTP status codes (201, 400, 404, 500)
- ✅ RFC 7807 error responses

### Frontend (Angular 17)
- ✅ Angular Material UI
- ✅ Auto-generated TypeScript client from Swagger
- ✅ Products page with full CRUD
- ✅ Orders page with multi-item support
- ✅ Client-side validation
- ✅ Error handling with snackbars

## 🔧 Key Commands

### Backend

```bash
# Restore packages
dotnet restore

# Run application
dotnet run

# Add EF migration (if using SQLite)
dotnet ef migrations add InitialCreate

# Apply migrations
dotnet ef database update
```

### Frontend

```bash
# Install dependencies
npm install

# Generate API client from Swagger
npm run gen:api

# Start dev server
npm start

# Build for production
npm run build
```

## 📚 Learn More

- Full README: `README.md`
- Backend docs: `StudyShop.Api/README.md`
- Frontend docs: `studyshop-ui/README.md`

## ⚡ Troubleshooting

**Backend won't start?**  
- Check .NET 9 SDK: `dotnet --version`
- Ensure port 5170 is free
- See [MIGRATION-TO-DOTNET9.md](MIGRATION-TO-DOTNET9.md) if upgrading

**Frontend won't start?**  
- Run `npm install` first
- Ensure backend is running for code generation

**API generation fails?**  
- Make sure backend is running at http://localhost:5170
- Run `npm run gen:api` manually

**CORS errors?**  
- Check browser console
- Verify CORS is configured in `Program.cs`

## 🎓 Learning Resources

This project demonstrates:
- OpenAPI/Swagger integration
- Code generation workflow
- Client-server validation
- RESTful API design
- Modern Angular patterns (Standalone Components)
- Material Design components

Enjoy exploring! 🎉

