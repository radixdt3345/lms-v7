import { test, expect } from '@playwright/test';

test.describe('E2E-043: System Health Endpoint', () => {
  test('E2E-043-01: Health endpoint returns healthy', async ({ request }) => {
    const response = await request.get('/api/v1/system/health');
    expect(response.status()).toBe(200);
    const body = await response.json();
    expect(body).toHaveProperty('data');
    expect(body.data.status).toBe('Healthy');
  });
});

test.describe('E2E-044: System Info Endpoint', () => {
  test('E2E-044-01: Info endpoint returns version', async ({ request }) => {
    const response = await request.get('/api/v1/system/info');
    expect(response.status()).toBe(200);
    const body = await response.json();
    expect(body).toHaveProperty('data');
    expect(body.data.version).toBe('1.0.0');
  });
});

test.describe('E2E-045: Seed Data Verification', () => {
  test('E2E-045-01: Super admin can login', async ({ request }) => {
    const response = await request.post('/api/v1/auth/login', {
      data: { email: 'superadmin@company.com', password: 'Admin@123' },
    });
    expect(response.status()).toBe(200);
    const body = await response.json();
    expect(body).toHaveProperty('data');
  });

  test('E2E-045-02: AppFooter shows health link', async ({ page }) => {
    await page.goto('/');
    const footer = page.getByTestId('app-footer');
    if (await footer.isVisible()) {
      await expect(page.getByTestId('footer-health-link')).toBeVisible();
    }
  });
});
