# Docker Setup for Donation Platform

## Prerequisites

- Docker Desktop installed
- Docker Compose installed

## Quick Start

### 1. Build and Start Services

```bash
docker-compose up -d
```

This will:
- Start PostgreSQL 15 on port 5432
- Build and start the .NET API on port 5000
- Automatically seed the database with schema

### 2. Verify Database

```bash
# Connect to PostgreSQL
docker exec -it donation-platform-db psql -U postgres -d DonationPlatform

# Run SQL queries
\dt  # List tables
SELECT COUNT(*) FROM "Organizers";
SELECT COUNT(*) FROM "Campaigns";
SELECT COUNT(*) FROM "Donations";
\q   # Exit
```

### 3. Check API

```bash
curl http://localhost:5000/api/campaigns
```

### 4. View Logs

```bash
# API logs
docker logs donation-platform-api -f

# Database logs
docker logs donation-platform-db -f
```

## Database Seeding

The database is automatically initialized with schema from `init-db.sql`.

To populate with test data, run:

```bash
# From inside the container
docker exec -it donation-platform-api dotnet DonationPlatform.API.dll

# Or manually trigger seeding via API endpoint (if implemented)
curl -X POST http://localhost:5000/api/seed
```

## Stop and Clean Up

```bash
# Stop services
docker-compose down

# Stop and remove volumes (delete database)
docker-compose down -v

# Remove images
docker-compose down --rmi all
```

## Troubleshooting

### Database won't start
```bash
docker-compose logs postgres
```

### API connection error
Wait 10-15 seconds for PostgreSQL to be ready (healthcheck interval)

### Port already in use
Change port mapping in docker-compose.yml:
```yaml
ports:
  - "5433:5432"  # Use 5433 instead
```

## Environment Variables

Edit `docker-compose.yml` to customize:
- `POSTGRES_PASSWORD` - Database password
- `POSTGRES_DB` - Database name
- `ASPNETCORE_ENVIRONMENT` - API environment
