# PowerShell script to export Swagger JSON to a file
# Usage: .\Scripts\Export-Swagger.ps1

param(
    [string]$OutputFile = "../../swagger.json"
)

$projectPath = Join-Path $PSScriptRoot "..\StudyShop.Api.csproj"
$swaggerUrl = "http://localhost:5170/swagger/v1/swagger.json"

Write-Host "Exporting Swagger JSON..." -ForegroundColor Cyan

# Check if API is running
try {
    $response = Invoke-WebRequest -Uri $swaggerUrl -UseBasicParsing -TimeoutSec 5
    $swaggerJson = $response.Content
    
    $outputPath = Join-Path $PSScriptRoot $OutputFile
    $swaggerJson | Out-File -FilePath $outputPath -Encoding utf8
    
    Write-Host "✓ Swagger JSON exported to: $outputPath" -ForegroundColor Green
}
catch {
    Write-Host "✗ Error: API is not running on $swaggerUrl" -ForegroundColor Red
    Write-Host "  Please start the API first: dotnet run" -ForegroundColor Yellow
    exit 1
}

