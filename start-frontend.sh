#!/bin/bash
# Quick start script for StudyShop UI

echo "Starting StudyShop UI..."
echo ""
cd studyshop-ui

# Check if node_modules exists
if [ ! -d "node_modules" ]; then
    echo "Installing dependencies..."
    npm install
fi

# Generate API client
echo "Generating API client..."
npm run gen:api || echo "Could not generate API client (backend may not be running)"

# Start dev server
echo "Starting Angular dev server..."
npm start

