#!/usr/bin/env node
/**
 * Simple script to export Swagger JSON from the running API
 * Usage: node export-swagger.js
 */

const http = require('http');
const fs = require('fs');
const path = require('path');

const SWAGGER_URL = 'http://localhost:5170/swagger/v1/swagger.json';
const OUTPUT_FILE = path.join(__dirname, 'swagger.json');

console.log('📥 Fetching Swagger JSON from API...');

http.get(SWAGGER_URL, (res) => {
  let data = '';

  res.on('data', (chunk) => {
    data += chunk;
  });

  res.on('end', () => {
    try {
      // Validate JSON and format it
      const json = JSON.parse(data);
      const formatted = JSON.stringify(json, null, 2);
      
      fs.writeFileSync(OUTPUT_FILE, formatted, 'utf8');
      console.log(`✅ Swagger JSON exported to: ${OUTPUT_FILE}`);
      console.log(`   File size: ${(formatted.length / 1024).toFixed(2)} KB`);
    } catch (error) {
      console.error('❌ Error parsing JSON:', error.message);
      process.exit(1);
    }
  });
}).on('error', (error) => {
  console.error(`❌ Error: API is not running on ${SWAGGER_URL}`);
  console.error('   Please start the API first: dotnet run --project StudyShop.Api');
  process.exit(1);
});

