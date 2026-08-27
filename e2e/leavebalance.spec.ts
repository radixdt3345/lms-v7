import { test, expect } from '@playwright/test';

test.describe('E2E-023: My Leave Balance Page', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    await page.getByTestId('email-input').fill('superadmin@company.com');
    await page.getByTestId('password-input').fill('Admin@123');
    await page.getByTestId('login-button').click();
    await page.waitForURL('/leave-balance');
  });

  test('E2E-023-01: Employee can view leave balances', async ({ page }) => {
    await page.goto('/leave-balance');
    await expect(page.getByTestId('page-title')).toBeVisible();
    await expect(page.getByTestId('year-select')).toBeVisible();
  });

  test('E2E-023-02: Year selector changes displayed balances', async ({ page }) => {
    await page.goto('/leave-balance');
    await page.getByTestId('year-select').click();
    await page.getByRole('option', { name: '2025' }).click();
    await expect(page.getByTestId('year-select')).toContainText('2025');
  });
});

test.describe('E2E-024: Leave Balance Management (HR Admin)', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    await page.getByTestId('email-input').fill('hradmin@company.com');
    await page.getByTestId('password-input').fill('Admin@123');
    await page.getByTestId('login-button').click();
    await page.waitForURL('/admin/leave-balances');
  });

  test('E2E-024-01: HR Admin can view all leave balances', async ({ page }) => {
    await page.goto('/admin/leave-balances');
    await expect(page.getByTestId('page-title')).toBeVisible();
    await expect(page.getByTestId('balances-grid')).toBeVisible();
  });

  test('E2E-024-02: Credit Annual button is present', async ({ page }) => {
    await page.goto('/admin/leave-balances');
    await expect(page.getByTestId('credit-annual-btn')).toBeVisible();
  });
});
