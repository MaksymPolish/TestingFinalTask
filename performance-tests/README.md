# Performance Testing with k6

## Overview
This directory contains k6 performance testing scripts for the Donation Platform API. k6 is a modern load testing tool that allows us to test API performance under various conditions.

## Test Scripts

### 1. Smoke Test (`smoke-test.js`)
**Purpose**: Quick validation that the API is working properly
- 5 concurrent virtual users
- Tests all main endpoints
- Low duration - suitable for CI/CD pipelines
- **Run locally**: `k6 run smoke-test.js`
- **Run with custom base URL**: `k6 run -e BASE_URL=http://your-api:5000 smoke-test.js`

### 2. Load Test (`load-test.js`)
**Purpose**: Evaluate API performance under sustained load
- Ramps up to 200 concurrent users
- Tests both GET and POST operations
- 16 minute duration
- **Run locally**: `k6 run load-test.js`
- **Run with custom base URL**: `k6 run -e BASE_URL=http://your-api:5000 load-test.js`

### 3. User Journey Test (`user-journey.js`)
**Purpose**: Simulate realistic user behavior patterns
- Tests typical user workflows (browse → view → donate)
- 50 concurrent users
- Includes realistic think time between actions
- **Run locally**: `k6 run user-journey.js`
- **Run with custom base URL**: `k6 run -e BASE_URL=http://your-api:5000 user-journey.js`

## Installation

### Local Machine
```bash
# Windows (using Chocolatey)
choco install k6

# macOS (using Homebrew)
brew install k6

# Linux
sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys C5AD17C747E3415A3642D57D77C6C491D6AC1D69
echo "deb https://dl.k6.io/deb stable main" | sudo tee /etc/apt/sources.list.d/k6-stable.list
sudo apt-get update
sudo apt-get install k6
```

### Docker
```bash
docker run -i grafana/k6 run - <smoke-test.js
```

## Requirements

- **k6**: Latest version (v0.50+)
- **Running API**: The API must be running and accessible at the specified BASE_URL
- **Database**: For meaningful tests, ensure the database has proper seed data

## Environment Variables

- `BASE_URL`: The base URL of the API (default: `http://localhost:5000`)

## Execution Examples

### Run smoke test against local API
```bash
k6 run smoke-test.js
```

### Run load test against production API
```bash
k6 run -e BASE_URL=https://api.production.com load-test.js
```

### Run with custom output file
```bash
k6 run --out csv=results.csv smoke-test.js
```

### Run in cloud (Grafana Cloud)
```bash
k6 cloud smoke-test.js
```

## Interpreting Results

### Key Metrics
- **http_req_duration**: Time taken to complete HTTP request (p95, p99 percentiles shown)
- **http_req_failed**: Percentage of failed requests
- **http_reqs**: Total number of requests made
- **vus**: Virtual users (concurrent connections)
- **group_duration**: Time to complete a logical group of requests

### Example Output
```
✓ status is 200
✓ response time < 500ms
✓ has campaigns data

check.........................: 100% ✓ 6 ✗ 0
http_req_blocked...............: avg=5.23ms  min=0s      max=10.45ms  p(90)=8.34ms  p(95)=9.39ms 
http_req_connecting............: avg=1.02ms  min=0s      max=2.15ms   p(90)=1.95ms  p(95)=2.05ms 
http_req_duration..............: avg=123.45ms min=50ms    max=245ms    p(90)=200ms   p(95)=220ms 
http_req_failed................: 0%      ✓ 0 ✗ 6
http_req_sending..............: avg=1.23ms  min=0.5ms   max=3.45ms   p(90)=2.3ms   p(95)=2.8ms  
http_req_tls_handshaking.......: avg=0s      min=0s      max=0s       p(90)=0s      p(95)=0s     
http_req_waiting..............: avg=121.1ms min=48ms    max=242ms    p(90)=197ms   p(95)=217ms
```

## CI/CD Integration

Performance tests are run in the GitHub Actions pipeline:
- **Smoke tests**: Run on every commit (quick validation)
- **Load tests**: Run on main branch commits (comprehensive validation)
- **Results**: Published as artifacts and tracked over time

## Performance Baselines

Current performance targets:
- **p99 Response Time**: < 1500ms for smoke tests
- **p99 Response Time**: < 5000ms for load tests
- **Error Rate**: < 10% for smoke tests, < 5% for load tests
- **GET /api/campaigns**: < 1000ms p95

## Troubleshooting

### Connection Refused
```
Error: Get "http://localhost:5000/api/campaigns": dial: connection refused
```
**Solution**: Ensure the API is running and accessible at the specified BASE_URL

### High Response Times
- Check API server resources (CPU, memory)
- Look for database bottlenecks
- Review application logs for errors

### High Error Rates
- Verify database has proper seed data
- Check API configuration
- Review request payloads (especially required fields)

## Resources

- [k6 Documentation](https://k6.io/docs/)
- [k6 Best Practices](https://k6.io/docs/misc/best-practices/)
- [Grafana Cloud k6](https://grafana.com/products/cloud/k6/)
