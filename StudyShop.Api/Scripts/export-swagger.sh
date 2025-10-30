#!/bin/bash
# Shell script to export Swagger JSON to a file
# Usage: ./Scripts/export-swagger.sh

OUTPUT_FILE="${1:-../../swagger.json}"
SWAGGER_URL="http://localhost:5170/swagger/v1/swagger.json"

echo "Exporting Swagger JSON..."

# Check if API is running and export
if curl -s -f "$SWAGGER_URL" > /dev/null 2>&1; then
    curl -s "$SWAGGER_URL" | jq '.' > "$(dirname "$0")/$OUTPUT_FILE"
    echo "✓ Swagger JSON exported to: $(dirname "$0")/$OUTPUT_FILE"
else
    echo "✗ Error: API is not running on $SWAGGER_URL"
    echo "  Please start the API first: dotnet run"
    exit 1
fi

