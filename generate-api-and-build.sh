#!/bin/bash
# Script to export Swagger, generate TypeScript services, and build the UI

set -e

echo "🚀 Starting Swagger export and code generation process..."
echo ""

# Step 1: Check if API is running
echo "📋 Step 1: Checking if API is running..."
if curl -s http://localhost:5170/api/products > /dev/null 2>&1; then
    echo "✅ API is running on http://localhost:5170"
else
    echo "❌ API is not running. Please start it first:"
    echo "   cd StudyShop.Api"
    echo "   dotnet run"
    exit 1
fi

# Step 2: Export Swagger JSON
echo ""
echo "📥 Step 2: Exporting Swagger JSON..."
if curl -s http://localhost:5170/swagger/v1/swagger.json -o swagger.json; then
    SIZE=$(wc -l < swagger.json)
    if [ "$SIZE" -gt 10 ]; then
        echo "✅ Swagger JSON exported successfully (${SIZE} lines)"
    else
        echo "⚠️  Warning: Swagger JSON seems incomplete (${SIZE} lines)"
        echo "   The API might be running in Production mode where Swagger is disabled"
        echo "   Trying to restart API in Development mode..."
        
        # Kill any existing API processes
        pkill -f "dotnet.*StudyShop.Api" 2>/dev/null || true
        
        # Start API in Development mode
        cd StudyShop.Api
        ASPNETCORE_ENVIRONMENT=Development dotnet run > ../api.log 2>&1 &
        API_PID=$!
        cd ..
        
        echo "⏳ Waiting for API to start in Development mode..."
        sleep 12
        
        # Try exporting again
        if curl -s http://localhost:5170/swagger/v1/swagger.json -o swagger.json; then
            SIZE=$(wc -l < swagger.json)
            echo "✅ Swagger JSON exported successfully (${SIZE} lines)"
        else
            echo "❌ Failed to export Swagger JSON"
            exit 1
        fi
        
        # Kill the background API
        kill $API_PID 2>/dev/null || true
    fi
else
    echo "❌ Failed to export Swagger JSON"
    exit 1
fi

# Step 3: Generate TypeScript services
echo ""
echo "🔧 Step 3: Generating TypeScript services..."
cd studyshop-ui

if npm run gen:api; then
    echo "✅ TypeScript services generated successfully"
else
    echo "❌ Failed to generate TypeScript services"
    cd ..
    exit 1
fi

# Step 4: Build the UI
echo ""
echo "🏗️  Step 4: Building the Angular UI..."
if npm run build; then
    echo "✅ UI built successfully!"
    echo ""
    echo "📁 Build output: studyshop-ui/dist/studyshop-ui/browser"
    echo ""
    echo "🎉 All done! You can now:"
    echo "   - Serve the UI: npm start"
    echo "   - Or use Docker: docker-compose up --build"
else
    echo "❌ UI build failed"
    cd ..
    exit 1
fi

cd ..
echo ""
echo "✨ Process completed successfully!"

