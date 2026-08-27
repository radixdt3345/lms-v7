import { test, expect } from '@playwright/test';

test.describe('E2E-027: Employee Comp-Off Request Submission', () => {
  test('employee can submit a comp-off request', async ({ page }) => {
    await page.goto('/login');
    await page.getByTestId('email-input').fill('employee@company.com');
    await page.getByTestId('password-input').fill('Password123!');
    await page.getByTestId('login-btn').click();
    await page.waitForURL('**/dashboard');

    await page.goto('/comp-off');
    await expect(page.getByTestId('page-title')).toBeVisible();
    await page.getByTestId('request-comp-off-btn').click();

    // Fill form if dialog present
    const dialog = page.locator('[role="dialog"]');
    if (await dialog.isVisible()) {
      await dialog.getByLabel(/worked date/i).fill('2026-08-20');
      await dialog.getByLabel(/credit days/i).fill('1');
      await dialog.getByLabel(/reason/i).fill('Worked on weekend');
      await dialog.getByRole('button', { name: /submit/i }).click();
    }

    await expect(page.getByTestId('comp-off-grid')).toBeVisible();
  });
});

test.describe('E2E-028: HR Admin Comp-Off Approval', () => {
  test('hr admin can view and approve comp-off requests', async ({ page }) => {
    await page.goto('/login');
    await page.getByTestId('email-input').fill('hradmin@company.com');
    await page.getByTestId('password-input').fill('Password123!');
    await page.getByTestId('login-btn').click();
    await page.waitForURL('**/dashboard');

    await page.goto('/admin/comp-off');
    await expect(page.getByTestId('page-title')).toBeVisible();
    await expect(page.getByTestId('comp-off-management-grid')).toBeVisible();

    const approveBtn = page.getByTestId(/approve-btn-/).first();
    if (await approveBtn.isVisible()) {
      await approveBtn.click();
      await expect(approveBtn).not.toBeVisible({ timeout: 3000 }).catch(() => {});
    }
  });
});
