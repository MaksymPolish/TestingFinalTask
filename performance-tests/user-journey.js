import http from 'k6/http';
import { check, group, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '1m', target: 50 },    // Ramp up to 50 VUs
    { duration: '3m', target: 50 },    // Stay for 3 minutes
    { duration: '1m', target: 0 },     // Ramp down
  ],
  thresholds: {
    http_req_duration: ['p(95)<1500'],
    http_req_failed: ['rate<0.05'],
  },
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000';

// Simulate realistic user journey
export default function () {
  // Step 1: User browses campaigns
  group('User Journey - Browse Campaigns', () => {
    const res = http.get(`${BASE_URL}/api/campaigns`);
    check(res, {
      'status is 200': (r) => r.status === 200,
      'response time < 800ms': (r) => r.timings.duration < 800,
    });
  });
  sleep(2); // User browsing time

  // Step 2: User views specific campaign
  group('User Journey - View Campaign Details', () => {
    const res = http.get(`${BASE_URL}/api/campaigns/1`);
    check(res, {
      'status is 200 or 404': (r) => r.status === 200 || r.status === 404,
      'response time < 500ms': (r) => r.timings.duration < 500,
    });
  });
  sleep(3); // User reading details

  // Step 3: User checks campaign progress
  group('User Journey - Check Campaign Stats', () => {
    const res = http.get(`${BASE_URL}/api/campaigns/1/stats`);
    check(res, {
      'status is 200 or 404': (r) => r.status === 200 || r.status === 404,
      'response time < 400ms': (r) => r.timings.duration < 400,
    });
  });
  sleep(1);

  // Step 4: User views donations
  group('User Journey - View Donations', () => {
    const res = http.get(`${BASE_URL}/api/campaigns/1/donations`);
    check(res, {
      'status is 200 or 404': (r) => r.status === 200 || r.status === 404,
      'response time < 500ms': (r) => r.timings.duration < 500,
    });
  });
  sleep(2); // User deciding to donate

  // Step 5: User makes donation
  group('User Journey - Make Donation', () => {
    const donationPayload = {
      donorName: `User_${Math.random().toString(36).substring(7)}`,
      donorEmail: `user${Date.now()}@example.com`,
      amount: 100,
      message: 'Supporting this great cause!',
      isAnonymous: false,
    };

    const params = {
      headers: {
        'Content-Type': 'application/json',
      },
    };

    const res = http.post(
      `${BASE_URL}/api/campaigns/1/donate`,
      JSON.stringify(donationPayload),
      params
    );

    check(res, {
      'donation succeeded or failed gracefully': (r) => 
        r.status === 201 || r.status === 400 || r.status === 404,
      'response time < 1000ms': (r) => r.timings.duration < 1000,
    });
  });
  sleep(5); // User waiting after donation
}
