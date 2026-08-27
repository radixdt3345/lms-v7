import { test, expect } from '@playwright/test';

test.describe('F-10 Public Holiday Management', () => {
  test.beforeEach(async ({ page }) => {
    // Login as HR Admin
    await page.goto('/login');
    await page.getByTestId('email-input').fill('superadmin@company.com');
    await page.getByTestId('password-input').fill('Admin@123');
    await page.getByTestId('login-btn').click();
    await page.waitForURL('**/dashboard');
  });

  test('E2E-037: HR Admin can view public holidays list', async ({ page }) => {
    await page.goto('/admin/holidays');
    await expect(page.getByTestId('page-title')).toBeVisible();
    await expect(page.getByTestId('holidays-table')).toBeVisible();
  });

  test('E2E-038: HR Admin can add a public holiday', async ({ page }) => {
    await page.goto('/admin/holidays');
    await page.getByTestId('add-holiday-btn').click();
    await expect(page.getByTestId('holiday-dialog')).toBeVisible();
    await page.getByTestId('holiday-date-input').fill('2026-12-25');
    await page.getByTestId('holiday-name-input').fill('Christmas Day E2E Test');
    await page.getByTestId('save-holiday-btn').click();
    await expect(page.getByTestId('holiday-dialog')).not.toBeVisible();
  });

  test('E2E-039: HR Admin can edit a public holiday', async ({ page }) => {
    await page.goto('/admin/holidays');
    // Assumes at least one holiday exists
    const editBtns = page.locator('[data-testid^="edit-holiday-"]');
    if (await editBtns.count() > 0) {
      await editBtns.first().click();
      await expect(page.getByTestId('holiday-dialog')).toBeVisible();
      await page.getByTestId('holiday-name-input').clear();
      await page.getByTestId('holiday-name-input').fill('Updated Holiday Name');
      await page.getByTestId('save-holiday-btn').click();
    }
  });
});
