# Donation Platform - Setup & Installation Guide

This guide will walk you through setting up the Donation Platform project from scratch.

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Quick Start](#quick-start)
3. [Database Setup](#database-setup)
4. [Running the Application](#running-the-application)
5. [API Testing](#api-testing)
6. [Running Tests](#running-tests)
7. [Docker Setup (Optional)](#docker-setup-optional)
8. [Troubleshooting](#troubleshooting)

## Prerequisites

Make sure you have the following installed on your system:

### Required
- **.NET SDK 10.0 or higher** - Download from [dotnet.microsoft.com](https://dotnet.microsoft.com/download)
- **PostgreSQL 13 or higher** - Download from [postgresql.org](https://www.postgresql.org/download/)
- **Git** - Download from [git-scm.com](https://git-scm.com/)

### Optional (for advanced features)
- **Docker & Docker Compose** - For containerized database and testing
- **Node.js & npm** - For k6 performance testing
- **Visual Studio Code** or **Visual Studio 2022+** - IDE recommendation

## Quick Start

### 1. Clone the Repository

```bash
git clone https://github.com/MaksymPolish/TestingFinalTask.git
cd TestingFinalTask
```

### 2. Verify .NET Installation

```bash
dotnet --version
```

### 3. Build the Solution

```bash
dotnet build
```

### 4. Set Up PostgreSQL

**Windows (pgAdmin):**
1. Open pgAdmin (comes with PostgreSQL)
2. Create a new database named `DonationPlatform`
3. Verify connection settings

**Command Line:**
```bash
psql -U postgres -c "CREATE DATABASE \"DonationPlatform\";"
```

### 5. Update Connection String

Edit `DonationPlatform.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=DonationPlatform;Username=postgres;Password=YOUR_PASSWORD"
  }
}
```

Replace `YOUR_PASSWORD` with your PostgreSQL password.

### 6. Run the Application

```bash
cd DonationPlatform.API
dotnet run
```

The API will start at `https://localhost:7001` (or check console for actual port)

## Database Setup

### Automatic Database Initialization

The application automatically creates the database schema when it starts. No manual migration is needed for the initial setup.

### Manual Migration (if needed)

If you want to manage migrations manually:

```bash
cd DonationPlatform.API
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Seed Database with Sample Data

To populate the database with test data:

1. Create a seed script or use the API endpoints to create organizers and campaigns
2. Example POST to `/api/campaigns`:

```bash
curl -X POST https://localhost:7001/api/campaigns \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Help Local Children",
    "description": "Support education for underprivileged children",
    "goalAmount": 10000,
    "startDate": "2026-01-01T00:00:00Z",
    "endDate": "2026-12-31T23:59:59Z",
    "organizerId": 1
  }'
```

## Running the Application

### Development Mode

```bash
cd DonationPlatform.API
dotnet run --configuration Development
```

This enables detailed logging and Swagger documentation.

### Production Mode

```bash
cd DonationPlatform.API
dotnet run --configuration Release
```

### With Custom Port

```bash
dotnet run --urls "https://localhost:8080"
```

## API Testing

### Using Swagger UI

1. Navigate to `https://localhost:7001/swagger/index.html`
2. Expand endpoints and click "Try it out"
3. Enter parameters and click "Execute"

### Using curl

**Get Active Campaigns:**
```bash
curl https://localhost:7001/api/campaigns \
  -H "Accept: application/json" \
  -k  # Ignore SSL certificate for development
```

**Create Campaign:**
```bash
curl -X POST https://localhost:7001/api/campaigns \
  -H "Content-Type: application/json" \
  -k \
  -d '{
    "title": "Campaign Title",
    "description": "Campaign Description",
    "goalAmount": 5000.00,
    "startDate": "2026-01-01T00:00:00Z",
    "endDate": "2026-12-31T23:59:59Z",
    "organizerId": 1
  }'
```

**Make Donation:**
```bash
curl -X POST https://localhost:7001/api/campaigns/1/donate \
  -H "Content-Type: application/json" \
  -k \
  -d '{
    "donorName": "John Doe",
    "donorEmail": "john@example.com",
    "amount": 100.00,
    "message": "Keep up the great work!",
    "isAnonymous": false
  }'
```

### Using Postman

1. Download [Postman](https://www.postman.com/downloads/)
2. Import the OpenAPI spec from `https://localhost:7001/swagger/v1/swagger.json`
3. Test endpoints with sample data

## Running Tests

### Unit Tests

```bash
dotnet test --configuration Release
```

### Filter Tests by Category

```bash
# Run only unit tests
dotnet test --filter "Category=Unit" --configuration Release

# Run only integration tests
dotnet test --filter "Category=Integration" --configuration Release

# Run only database tests
dotnet test --filter "Category=Database" --configuration Release
```

### Generate Test Report

```bash
dotnet test /p:CollectCoverageMetrics=true \
  --logger "trx;LogFileName=testResults.trx" \
  --configuration Release
```

## Docker Setup (Optional)

### Using Docker Compose for PostgreSQL

Create a `docker-compose.yml` file in the project root:

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:15-alpine
    container_name: donation_platform_db
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
      POSTGRES_DB: DonationPlatform
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

volumes:
  postgres_data:
```

Start the database:

```bash
docker-compose up -d
```

Stop the database:

```bash
docker-compose down
```

### Building Docker Image for API

Create a `Dockerfile`:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app
COPY . .
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/out .
EXPOSE 80
ENTRYPOINT ["dotnet", "DonationPlatform.API.dll"]
```

Build and run:

```bash
docker build -t donation-platform-api .
docker run -p 7001:80 \
  -e ConnectionStrings__DefaultConnection="Host=host.docker.internal;..." \
  donation-platform-api
```

## Performance Testing with k6

### Install k6

```bash
# Windows with Chocolatey
choco install k6

# Or download from https://k6.io/docs/getting-started/installation/
```

### Run Performance Tests

```bash
k6 run performance-tests.js
```

### Sample Performance Test Script

Create `performance-tests.js`:

```javascript
import http from 'k6/http';
import { check } from 'k6';

export let options = {
  stages: [
    { duration: '30s', target: 20 },   // Ramp-up
    { duration: '1m', target: 50 },    // Stay at peak
    { duration: '30s', target: 0 },    // Ramp-down
  ],
};

export default function () {
  let response = http.get('https://localhost:7001/api/campaigns');
  check(response, {
    'status is 200': (r) => r.status === 200,
    'response time < 500ms': (r) => r.timings.duration < 500,
  });
}
```

## Troubleshooting

### Connection String Issues

**Error: "could not connect to server"**

1. Verify PostgreSQL is running:
   ```bash
   psql -U postgres -c "SELECT 1"
   ```

2. Check connection string in `appsettings.json`

3. Verify database exists:
   ```bash
   psql -U postgres -l | grep DonationPlatform
   ```

### Port Already in Use

If port 7001 is in use, specify a different port:

```bash
dotnet run --urls "https://localhost:7777"
```

### HTTPS Certificate Issues

For development, disable HTTPS validation:

```bash
set ASPNETCORE_ENVIRONMENT=Development
dotnet run
```

### Migrations Not Applied

```bash
# Reset database
dotnet ef database drop -f
dotnet ef database update
```

### Dependencies Not Restoring

```bash
dotnet clean
dotnet restore
dotnet build
```

## Project Structure Reference

```
DonationPlatform/
├── DonationPlatform.API/
│   ├── Controllers/          # API endpoints
│   ├── Services/             # Business logic
│   ├── DTOs/                 # Data transfer objects
│   ├── Program.cs            # Startup configuration
│   └── appsettings.json      # Settings
│
├── DonationPlatform.Data/
│   └── DonationPlatformDbContext.cs  # EF Core context
│
├── DonationPlatform.Core/
│   └── Entities/             # Domain models
│
└── DonationPlatform.sln      # Solution file
```

## Environment Variables

Commonly used environment variables:

```bash
# Database
DB_HOST=localhost
DB_PORT=5432
DB_USER=postgres
DB_PASSWORD=postgres
DB_NAME=DonationPlatform

# Application
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=https://localhost:7001
```

## Next Steps

1. Review the [README.md](README.md) for API documentation
2. Explore the project structure
3. Set up IDE debugging
4. Run the test suite
5. Check the GitHub Actions CI/CD pipeline

## Support & Documentation

- [.NET Documentation](https://docs.microsoft.com/dotnet/)
- [Entity Framework Core Docs](https://docs.microsoft.com/en-us/ef/core/)
- [ASP.NET Core Docs](https://docs.microsoft.com/en-us/aspnet/core/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)

---

**Last Updated:** April 2026

**Version:** 1.0

**License:** Educational Purpose
