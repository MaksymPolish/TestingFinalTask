# API Reference

Complete API documentation for the Donation Platform.

## Base URL

```
https://localhost:7001/api
```

All timestamps are in UTC format (ISO 8601).

## Authentication

Currently, the API does not require authentication. In a production environment, bearer token authentication would be implemented.

## Response Format

All responses are JSON formatted.

### Success Response

```json
{
  "id": 1,
  "title": "Campaign Title",
  "description": "Campaign description",
  "goalAmount": 10000.00,
  "currentAmount": 5000.00,
  "status": "Active"
}
```

### Error Response

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Error description"
}
```

## Endpoints

### Campaigns

#### Get All Active Campaigns

Retrieve a list of all active campaigns.

**Request:**
```
GET /campaigns
```

**Response:** `200 OK`
```json
[
  {
    "id": 1,
    "title": "Help Local Children",
    "description": "Support education for underprivileged children",
    "goalAmount": 10000.00,
    "currentAmount": 5000.00,
    "startDate": "2026-01-01T00:00:00Z",
    "endDate": "2026-12-31T23:59:59Z",
    "status": "Active",
    "organizerId": 1,
    "organizer": {
      "id": 1,
      "name": "Charity Foundation",
      "email": "charity@example.com",
      "organization": "Global Charity",
      "isVerified": true
    }
  }
]
```

**Query Parameters:**
- None currently, but extensible for filtering

**Example:**
```bash
curl https://localhost:7001/api/campaigns -k
```

---

#### Create Campaign

Create a new donation campaign.

**Request:**
```
POST /campaigns
Content-Type: application/json
```

**Request Body:**
```json
{
  "title": "Campaign Title",
  "description": "Campaign description",
  "goalAmount": 10000.00,
  "startDate": "2026-01-01T00:00:00Z",
  "endDate": "2026-12-31T23:59:59Z",
  "organizerId": 1
}
```

**Response:** `201 Created`
```json
{
  "id": 1,
  "title": "Campaign Title",
  "description": "Campaign description",
  "goalAmount": 10000.00,
  "currentAmount": 0.00,
  "startDate": "2026-01-01T00:00:00Z",
  "endDate": "2026-12-31T23:59:59Z",
  "status": "Active",
  "organizerId": 1
}
```

**Error Responses:**
- `400 Bad Request` - Invalid input or organizer not verified
- `404 Not Found` - Organizer not found

**Business Rules:**
- Organizer must be verified to create campaigns
- Status defaults to "Active"
- CurrentAmount defaults to 0

**Example:**
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

---

#### Get Campaign by ID

Retrieve a specific campaign with progress information.

**Request:**
```
GET /campaigns/{id}
```

**Parameters:**
- `id` (path, required, integer) - Campaign ID

**Response:** `200 OK`
```json
{
  "id": 1,
  "title": "Campaign Title",
  "description": "Campaign description",
  "goalAmount": 10000.00,
  "currentAmount": 5000.00,
  "startDate": "2026-01-01T00:00:00Z",
  "endDate": "2026-12-31T23:59:59Z",
  "status": "Active",
  "organizerId": 1,
  "organizer": {
    "id": 1,
    "name": "Charity Foundation",
    "email": "charity@example.com",
    "organization": "Global Charity",
    "isVerified": true
  }
}
```

**Error Responses:**
- `404 Not Found` - Campaign not found

**Example:**
```bash
curl https://localhost:7001/api/campaigns/1 -k
```

---

#### Update Campaign

Update campaign details.

**Request:**
```
PUT /campaigns/{id}
Content-Type: application/json
```

**Parameters:**
- `id` (path, required, integer) - Campaign ID

**Request Body:**
```json
{
  "title": "Updated Title",
  "description": "Updated description",
  "goalAmount": 15000.00,
  "endDate": "2027-12-31T23:59:59Z"
}
```

**Response:** `200 OK`
```json
{
  "id": 1,
  "title": "Updated Title",
  "description": "Updated description",
  "goalAmount": 15000.00,
  "currentAmount": 5000.00,
  "status": "Active",
  "organizerId": 1
}
```

**Error Responses:**
- `404 Not Found` - Campaign not found

**Example:**
```bash
curl -X PUT https://localhost:7001/api/campaigns/1 \
  -H "Content-Type: application/json" \
  -k \
  -d '{
    "title": "Updated Title",
    "goalAmount": 15000.00
  }'
```

---

#### Make Donation

Create a donation to a campaign.

**Request:**
```
POST /campaigns/{id}/donate
Content-Type: application/json
```

**Parameters:**
- `id` (path, required, integer) - Campaign ID

**Request Body:**
```json
{
  "donorName": "John Doe",
  "donorEmail": "john@example.com",
  "amount": 100.00,
  "message": "Keep up the great work!",
  "isAnonymous": false
}
```

**Response:** `201 Created`
```json
{
  "id": 1,
  "campaignId": 1,
  "donorName": "John Doe",
  "donorEmail": "john@example.com",
  "amount": 100.00,
  "message": "Keep up the great work!",
  "createdAt": "2026-04-19T10:30:00Z",
  "isAnonymous": false
}
```

**Error Responses:**
- `400 Bad Request` - Donation amount < $1 or campaign is closed
- `404 Not Found` - Campaign not found

**Business Rules:**
- Minimum donation amount is $1.00
- Cannot donate to closed/cancelled campaigns
- Campaign automatically completes when goal is reached
- Anonymous donations are processed but can be masked in listings

**Example:**
```bash
curl -X POST https://localhost:7001/api/campaigns/1/donate \
  -H "Content-Type: application/json" \
  -k \
  -d '{
    "donorName": "John Doe",
    "donorEmail": "john@example.com",
    "amount": 100.00,
    "message": "Great cause!",
    "isAnonymous": false
  }'
```

---

#### Get Campaign Donations

Retrieve all donations for a campaign.

**Request:**
```
GET /campaigns/{id}/donations
```

**Parameters:**
- `id` (path, required, integer) - Campaign ID

**Response:** `200 OK`
```json
[
  {
    "id": 1,
    "campaignId": 1,
    "donorName": "John Doe",
    "donorEmail": "john@example.com",
    "amount": 100.00,
    "message": "Keep up the great work!",
    "createdAt": "2026-04-19T10:30:00Z",
    "isAnonymous": false
  },
  {
    "id": 2,
    "campaignId": 1,
    "donorName": "Anonymous",
    "donorEmail": null,
    "amount": 50.00,
    "message": "Love this cause",
    "createdAt": "2026-04-19T11:15:00Z",
    "isAnonymous": true
  }
]
```

**Business Rules:**
- Anonymous donations show "Anonymous" as donor name
- Email is null for anonymous donations

**Example:**
```bash
curl https://localhost:7001/api/campaigns/1/donations -k
```

---

#### Get Campaign Statistics

Retrieve statistical information about campaign donations.

**Request:**
```
GET /campaigns/{id}/stats
```

**Parameters:**
- `id` (path, required, integer) - Campaign ID

**Response:** `200 OK`
```json
{
  "totalAmount": 5000.00,
  "averageAmount": 250.00,
  "donorCount": 20
}
```

**Metrics:**
- `totalAmount` - Total donations received
- `averageAmount` - Average donation amount (total / count)
- `donorCount` - Number of donations

**Error Responses:**
- `404 Not Found` - Campaign not found

**Example:**
```bash
curl https://localhost:7001/api/campaigns/1/stats -k
```

---

#### Close Campaign

Close/cancel a campaign.

**Request:**
```
PATCH /campaigns/{id}/close
```

**Parameters:**
- `id` (path, required, integer) - Campaign ID

**Response:** `200 OK`
```json
{
  "id": 1,
  "title": "Campaign Title",
  "description": "Campaign description",
  "goalAmount": 10000.00,
  "currentAmount": 5000.00,
  "status": "Cancelled",
  "organizerId": 1
}
```

**Error Responses:**
- `404 Not Found` - Campaign not found

**Business Rules:**
- Changing status to "Cancelled" prevents further donations
- Status change is final

**Example:**
```bash
curl -X PATCH https://localhost:7001/api/campaigns/1/close -k
```

---

## Data Models

### Campaign

| Field | Type | Description |
|-------|------|-------------|
| id | integer | Unique identifier |
| title | string | Campaign title |
| description | string | Campaign description |
| goalAmount | decimal | Target donation amount |
| currentAmount | decimal | Current donations received |
| startDate | string (ISO 8601) | Campaign start date/time |
| endDate | string (ISO 8601) | Campaign end date/time |
| status | string | "Active", "Completed", or "Cancelled" |
| organizerId | integer | ID of organizing entity |
| organizer | object | Organizer details (populated in GET) |

### Donation

| Field | Type | Description |
|-------|------|-------------|
| id | integer | Unique identifier |
| campaignId | integer | Campaign ID |
| donorName | string | Donor name (or "Anonymous") |
| donorEmail | string | Donor email (null if anonymous) |
| amount | decimal | Donation amount |
| message | string | Optional donor message |
| createdAt | string (ISO 8601) | When donation was made |
| isAnonymous | boolean | Whether donation is anonymous |

### Organizer

| Field | Type | Description |
|-------|------|-------------|
| id | integer | Unique identifier |
| name | string | Organization name |
| email | string | Contact email |
| organization | string | Organization type/category |
| isVerified | boolean | Whether verified to create campaigns |

### Campaign Stats

| Field | Type | Description |
|-------|------|-------------|
| totalAmount | decimal | Sum of all donations |
| averageAmount | decimal | Average donation amount |
| donorCount | integer | Number of donations |

---

## Status Codes

| Code | Meaning |
|------|---------|
| 200 | OK - Request successful |
| 201 | Created - Resource created successfully |
| 400 | Bad Request - Invalid input or business rule violation |
| 404 | Not Found - Resource not found |
| 500 | Internal Server Error - Server error |

---

## Rate Limiting

Currently not implemented. Will be added in future versions.

---

## Pagination

Not implemented in current version. Extensible for large datasets.

---

## API Versioning

Not implemented. API uses `/api` base path for future extensibility.

---

## CORS

CORS is enabled for all origins in development mode.

---

## Error Handling

All errors follow standard HTTP status codes with descriptive messages:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Donation amount must be at least $1"
}
```

---

## Examples with Different Tools

### PowerShell

```powershell
# Get campaigns
$uri = "https://localhost:7001/api/campaigns"
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
$response = Invoke-WebRequest -Uri $uri -SkipCertificateCheck
$response.Content | ConvertFrom-Json
```

### JavaScript/Node.js

```javascript
const fetch = require('node-fetch');
const https = require('https');

const agent = new https.Agent({  
  rejectUnauthorized: false // For development only
});

fetch('https://localhost:7001/api/campaigns', { agent })
  .then(res => res.json())
  .then(data => console.log(data));
```

### Python

```python
import requests
from requests.packages.urllib3.exceptions import InsecureRequestWarning

requests.packages.urllib3.disable_warnings(InsecureRequestWarning)

response = requests.get(
    'https://localhost:7001/api/campaigns',
    verify=False  # For development only
)
print(response.json())
```

---

**Last Updated:** April 2026

**API Version:** 1.0
