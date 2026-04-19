# Donation Platform API

A comprehensive .NET-based API for managing charitable donation campaigns with full testing coverage (unit, integration, database, and performance tests).

## Features

- **Campaign Management**: Create, update, and manage donation campaigns
- **Donation Processing**: Accept donations with anonymous donor support
- **Campaign Statistics**: Track donation statistics and campaign progress
- **Organizer Verification**: Only verified organizers can create campaigns
- **Automatic Campaign Completion**: Campaigns auto-complete when goal is reached
- **PostgreSQL Database**: Persistent data storage with proper constraints

## Project Structure

```
DonationPlatform/
├── DonationPlatform.API/        # ASP.NET Core Web API
│   ├── Controllers/             # API endpoints
│   ├── Services/                # Business logic services
│   ├── DTOs/                    # Data transfer objects
│   └── Program.cs               # Configuration
├── DonationPlatform.Data/       # Data access layer
│   └── DonationPlatformDbContext.cs  # Entity Framework context
├── DonationPlatform.Core/       # Domain entities
│   └── Entities/                # Campaign, Donation, Organizer
└── DonationPlatform.sln         # Solution file
```

## Prerequisites

- .NET 10.0 or higher
- PostgreSQL 13 or higher
- Docker (for Testcontainers testing)
- Node.js (for k6 performance testing)

## Setup Instructions

### 1. Database Setup

Install PostgreSQL and create a database:

```sql
CREATE DATABASE DonationPlatform;
```

Update the connection string in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=DonationPlatform;Username=postgres;Password=your_password"
}
```

### 2. Build the Solution

```bash
cd DonationPlatform
dotnet build
```

### 3. Run the API

```bash
cd DonationPlatform.API
dotnet run
```

The API will be available at `https://localhost:7001` (or your configured HTTPS port)

### 4. Access Swagger Documentation

Open your browser and navigate to `https://localhost:7001/swagger/index.html` to explore the API endpoints.

## API Endpoints

### Campaigns

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/campaigns` | Get all active campaigns |
| POST | `/api/campaigns` | Create a new campaign |
| GET | `/api/campaigns/{id}` | Get campaign by ID |
| PUT | `/api/campaigns/{id}` | Update a campaign |
| PATCH | `/api/campaigns/{id}/close` | Close/cancel a campaign |

### Donations

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/campaigns/{id}/donate` | Create a donation |
| GET | `/api/campaigns/{id}/donations` | Get campaign donations |
| GET | `/api/campaigns/{id}/stats` | Get campaign statistics |

## Business Rules

1. **Minimum Donation**: Donations must be at least $1
2. **Closed Campaigns**: Cannot accept donations for closed/cancelled campaigns
3. **Automatic Completion**: Campaign automatically completes when goal is reached
4. **Anonymous Donations**: Anonymous donations hide donor name in listings
5. **Verified Organizers Only**: Only verified organizers can create campaigns

## Database Schema

### Campaign Entity
- Id (int, primary key)
- Title (string)
- Description (string)
- GoalAmount (decimal)
- CurrentAmount (decimal)
- StartDate (datetime)
- EndDate (datetime)
- Status (enum: Active, Completed, Cancelled)
- OrganizerId (int, foreign key)

### Donation Entity
- Id (int, primary key)
- CampaignId (int, foreign key)
- DonorName (string)
- DonorEmail (string)
- Amount (decimal)
- Message (string)
- CreatedAt (datetime)
- IsAnonymous (bool)

### Organizer Entity
- Id (int, primary key)
- Name (string)
- Email (string)
- Organization (string)
- IsVerified (bool)

## Testing

### Unit Tests
Tests for business logic, calculations, and validation rules.

### Integration Tests
Tests using WebApplicationFactory for complete request/response flow.

### Database Tests
Tests using Testcontainers with PostgreSQL for data consistency and integrity.

### Performance Tests
Load and stress tests using k6 for:
- Campaign list retrieval
- Concurrent donation processing
- High-traffic scenarios

## Development

### Adding Migrations

```bash
cd DonationPlatform.API
dotnet ef migrations add MigrationName
dotnet ef database update
```

### Running Tests

```bash
dotnet test
```

### Running Performance Tests

```bash
k6 run performance-tests.js
```

## CI/CD Pipeline

This project includes a GitHub Actions workflow that:
- Runs on every push and pull request
- Executes unit tests
- Runs integration tests
- Performs static code analysis
- Generates test reports

## Configuration Files

- `appsettings.json` - Application settings and connection strings
- `appsettings.Development.json` - Development-specific settings
- `.github/workflows/ci.yml` - GitHub Actions CI/CD pipeline

## Error Handling

The API returns appropriate HTTP status codes:
- 200 OK - Successful request
- 201 Created - Resource created successfully
- 400 Bad Request - Invalid input or business rule violation
- 404 Not Found - Resource not found
- 500 Internal Server Error - Server error

## Future Enhancements

- [ ] Authentication and authorization
- [ ] Email notifications for donors
- [ ] Campaign search and filtering
- [ ] Pagination for large datasets
- [ ] Caching layer
- [ ] API versioning
- [ ] Rate limiting
- [ ] Comprehensive logging

## License

This project is for educational purposes.

## Support

For questions or issues, please create a GitHub issue in the repository.
