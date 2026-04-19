import http from 'k6/http';
import { check, group, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '5s', target: 5 },   // Ramp up to 5 VUs
    { duration: '10s', target: 5 },  // Stay at 5 VUs
    { duration: '5s', target: 0 },   // Ramp down to 0 VUs
  ],
  thresholds: {
    http_req_duration: ['p(99)<1500'], // 99% of requests should be below 1500ms
    http_req_failed: ['rate<0.1'],     // error rate should be below 10%
  },
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000';

export default function () {
  group('Campaigns API - Smoke Tests', () => {
    // Test 1: Get all campaigns
    group('GET /api/campaigns', () => {
      const response = http.get(`${BASE_URL}/api/campaigns`);
      check(response, {
        'status is 200': (r) => r.status === 200,
        'response time < 500ms': (r) => r.timings.duration < 500,
        'has campaigns data': (r) => r.body.includes('id'),
      });
    });
    sleep(1);

    // Test 2: Create a campaign
    group('POST /api/campaigns', () => {
      const payload = {
        title: `Campaign ${Date.now()}`,
        description: 'Performance test campaign',
        goalAmount: 10000,
        startDate: new Date().toISOString(),
        endDate: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString(),
        organizerId: 1,
      };

      const params = {
        headers: {
          'Content-Type': 'application/json',
        },
      };

      const response = http.post(
        `${BASE_URL}/api/campaigns`,
        JSON.stringify(payload),
        params
      );

      check(response, {
        'status is 201': (r) => r.status === 201,
        'response time < 1000ms': (r) => r.timings.duration < 1000,
      });
    });
    sleep(1);

    // Test 3: Get specific campaign (using ID 1 for demo)
    group('GET /api/campaigns/:id', () => {
      const response = http.get(`${BASE_URL}/api/campaigns/1`);
      check(response, {
        'status is 200 or 404': (r) => r.status === 200 || r.status === 404,
        'response time < 300ms': (r) => r.timings.duration < 300,
      });
    });
    sleep(1);

    // Test 4: Make a donation
    group('POST /api/campaigns/:id/donate', () => {
      const payload = {
        donorName: `Donor ${Date.now()}`,
        donorEmail: `donor${Date.now()}@example.com`,
        amount: 100,
        message: 'Performance test donation',
        isAnonymous: false,
      };

      const params = {
        headers: {
          'Content-Type': 'application/json',
        },
      };

      const response = http.post(
        `${BASE_URL}/api/campaigns/1/donate`,
        JSON.stringify(payload),
        params
      );

      check(response, {
        'status is 201 or 400 or 404': (r) => 
          r.status === 201 || r.status === 400 || r.status === 404,
        'response time < 1000ms': (r) => r.timings.duration < 1000,
      });
    });
    sleep(1);

    // Test 5: Get campaign stats
    group('GET /api/campaigns/:id/stats', () => {
      const response = http.get(`${BASE_URL}/api/campaigns/1/stats`);
      check(response, {
        'status is 200 or 404': (r) => r.status === 200 || r.status === 404,
        'response time < 300ms': (r) => r.timings.duration < 300,
      });
    });
    sleep(1);

    // Test 6: Get donations
    group('GET /api/campaigns/:id/donations', () => {
      const response = http.get(`${BASE_URL}/api/campaigns/1/donations`);
      check(response, {
        'status is 200 or 404': (r) => r.status === 200 || r.status === 404,
        'response time < 300ms': (r) => r.timings.duration < 300,
      });
    });
    sleep(1);
  });
}
