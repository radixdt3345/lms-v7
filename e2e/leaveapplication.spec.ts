import { test, expect } from '@playwright/test';

test.describe('E2E-025: My Leave Applications', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    await page.getByTestId('email-input').fill('superadmin@company.com');
    await page.getByTestId('password-input').fill('Admin@123');
    await page.getByTestId('login-button').click();
  });

  test('E2E-025-01: Page renders with apply button', async ({ page }) => {
    await page.goto('/leave-applications');
    await expect(page.getByTestId('page-title')).toBeVisible();
    await expect(page.getByTestId('apply-leave-btn')).toBeVisible();
  });

  test('E2E-025-02: Apply leave dialog opens', async ({ page }) => {
    await page.goto('/leave-applications');
    await page.getByTestId('apply-leave-btn').click();
    await expect(page.getByTestId('apply-leave-dialog')).toBeVisible();
  });
});

test.describe('E2E-026: Leave Application Management (HR)', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    await page.getByTestId('email-input').fill('hradmin@company.com');
    await page.getByTestId('password-input').fill('Admin@123');
    await page.getByTestId('login-button').click();
  });

  test('E2E-026-01: HR admin sees all applications grid', async ({ page }) => {
    await page.goto('/admin/leave-applications');
    await expect(page.getByTestId('all-applications-grid')).toBeVisible();
  });
});
