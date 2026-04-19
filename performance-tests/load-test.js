import http from 'k6/http';
import { check, group, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '2m', target: 100 },   // Ramp up to 100 VUs over 2 minutes
    { duration: '5m', target: 100 },   // Stay at 100 VUs for 5 minutes
    { duration: '2m', target: 200 },   // Ramp up to 200 VUs over 2 minutes
    { duration: '5m', target: 200 },   // Stay at 200 VUs for 5 minutes
    { duration: '2m', target: 0 },     // Ramp down to 0 VUs over 2 minutes
  ],
  thresholds: {
    http_req_duration: ['p(95)<2000', 'p(99)<5000'], 
    http_req_failed: ['rate<0.1'],
    'group_duration{group:::get_campaigns}': ['p(95)<1000'],
  },
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000';

export default function () {
  group('Heavy Load Test - GET Operations', () => {
    // Heavily test GET endpoints
    const getCampaignsResponse = http.get(`${BASE_URL}/api/campaigns`);
    check(getCampaignsResponse, {
      'get campaigns status is 200': (r) => r.status === 200,
    });

    const getCampaignResponse = http.get(`${BASE_URL}/api/campaigns/1`);
    check(getCampaignResponse, {
      'get campaign status is 200 or 404': (r) => r.status === 200 || r.status === 404,
    });

    const getStatsResponse = http.get(`${BASE_URL}/api/campaigns/1/stats`);
    check(getStatsResponse, {
      'get stats status is 200 or 404': (r) => r.status === 200 || r.status === 404,
    });

    sleep(0.5);
  });

  group('Heavy Load Test - POST Operations', () => {
    // Test donation creation under load
    const payload = {
      donorName: `LoadTest_${Date.now()}_${Math.random()}`,
      donorEmail: `load${Date.now()}@example.com`,
      amount: 50,
      message: 'Load test donation',
      isAnonymous: Math.random() > 0.5,
    };

    const params = {
      headers: {
        'Content-Type': 'application/json',
      },
    };

    const donateResponse = http.post(
      `${BASE_URL}/api/campaigns/1/donate`,
      JSON.stringify(payload),
      params
    );

    check(donateResponse, {
      'donate status is 201 or 400 or 404': (r) => 
        r.status === 201 || r.status === 400 || r.status === 404,
    });

    sleep(0.5);
  });
}
