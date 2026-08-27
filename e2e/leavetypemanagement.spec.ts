import { test, expect, APIRequestContext } from '@playwright/test';

/**
 * F-04 Leave Type Management — End-to-End Tests
 *
 * E2E-019: HR Admin views the leave type list
 * E2E-020: HR Admin creates a new leave type
 * E2E-021: HR Admin deactivates a leave type
 * E2E-022: Employee cannot access leave type management (redirected to login)
 *
 * Prerequisites:
 *   BASE_URL        - frontend origin (default: http://localhost:5173)
 *   API_URL         - backend API origin (default: http://localhost:5105)
 *   ADMIN_EMAIL     - seeded HR_ADMIN user email
 *   ADMIN_PASSWORD  - seeded HR_ADMIN user password
 *   USER_EMAIL      - seeded EMPLOYEE user email
 *   USER_PASSWORD   - seeded EMPLOYEE user password (default: Employee1!)
 *
 * Trigger: workflow_dispatch only — never wired to push/pull_request
 * Run: npx playwright test e2e/leavetypemanagement.spec.ts
 */

const BASE_URL = process.env.BASE_URL ?? 'http://localhost:5173';
const API_URL = process.env.API_URL ?? 'http://localhost:5105';
const ADMIN_EMAIL = process.env.ADMIN_EMAIL ?? 'admin@lms-staging.example.com';
const ADMIN_PASSWORD = process.env.ADMIN_PASSWORD ?? 'Admin1!';
const USER_EMAIL = process.env.USER_EMAIL ?? 'employee@lms-staging.example.com';
const USER_PASSWORD = process.env.USER_PASSWORD ?? 'Employee1!';

// ── API helpers ────────────────────────────────────────────────────────────────

async function apiLogin(
  apiCtx: APIRequestContext,
  email: string,
  password: string
): Promise<string | null> {
  const resp = await apiCtx.post(`${API_URL}/api/v1/auth/login`, {
    data: { email, password },
  });
  if (!resp.ok()) return null;
  const body = await resp.json();
  return body?.data?.accessToken ?? null;
}

async function apiDeleteLeaveTypeByName(
  apiCtx: APIRequestContext,
  jwt: string,
  name: string
): Promise<void> {
  const listResp = await apiCtx.get(`${API_URL}/api/v1/leave-types`, {
    headers: { Authorization: `Bearer ${jwt}` },
  });
  if (!listResp.ok()) return;
  const body = await listResp.json();
  const types: { id: string; name: string }[] = body?.data ?? [];
  const target = types.find((t) => t.name === name);
  if (target) {
    await apiCtx.delete(`${API_URL}/api/v1/leave-types/${target.id}`, {
      headers: { Authorization: `Bearer ${jwt}` },
    });
  }
}

// ── E2E-019: HR Admin views leave type list ────────────────────────────────────

test('E2E-019: HR Admin views leave type list with seeded types', async ({ page, request }) => {
  const jwt = await apiLogin(request, ADMIN_EMAIL, ADMIN_PASSWORD);
  expect(jwt).not.toBeNull();

  await page.goto(`${BASE_URL}/login`);
  await page.getByLabel(/email/i).fill(ADMIN_EMAIL);
  await page.getByLabel(/password/i).fill(ADMIN_PASSWORD);
  await page.getByRole('button', { name: /login|sign in/i }).click();
  await page.waitForURL(/\/admin|\/dashboard/, { timeout: 10_000 });

  await page.goto(`${BASE_URL}/admin/leave-types`);
  await page.waitForSelector('[data-testid="leave-type-table"]', { timeout: 10_000 });

  // Verify at least the 5 seeded leave types are visible
  await expect(page.getByText('Casual Leave')).toBeVisible({ timeout: 8_000 });
  await expect(page.getByText('Sick Leave')).toBeVisible();
  await expect(page.getByText('Earned Leave')).toBeVisible();
});

// ── E2E-020: HR Admin creates a new leave type ────────────────────────────────

test('E2E-020: HR Admin creates a new leave type', async ({ page, request }) => {
  const jwt = await apiLogin(request, ADMIN_EMAIL, ADMIN_PASSWORD);
  expect(jwt).not.toBeNull();

  const newName = `Study Leave ${Date.now()}`;

  // Cleanup after test
  test.afterEach(async () => {
    await apiDeleteLeaveTypeByName(request, jwt!, newName);
  });

  await page.goto(`${BASE_URL}/login`);
  await page.getByLabel(/email/i).fill(ADMIN_EMAIL);
  await page.getByLabel(/password/i).fill(ADMIN_PASSWORD);
  await page.getByRole('button', { name: /login|sign in/i }).click();
  await page.waitForURL(/\/admin|\/dashboard/, { timeout: 10_000 });

  await page.goto(`${BASE_URL}/admin/leave-types`);
  await page.waitForSelector('[data-testid="leave-type-table"]', { timeout: 10_000 });

  // Open create dialog
  await page.getByTestId('create-leave-type-btn').click();
  await page.waitForSelector('[data-testid="leave-type-dialog"]', { timeout: 5_000 });

  // Fill form
  await page.getByTestId('leave-type-name-input').fill(newName);
  await page.getByTestId('leave-type-code-input').fill('STL');
  await page.getByTestId('leave-type-annual-days-input').fill('10');

  await page.getByTestId('leave-type-submit-btn').click();

  // Success snackbar
  await expect(page.getByTestId('success-snackbar')).toBeVisible({ timeout: 8_000 });

  // New type appears in table
  await expect(page.getByText(newName)).toBeVisible({ timeout: 8_000 });
});

// ── E2E-021: HR Admin deactivates a leave type ────────────────────────────────

test('E2E-021: HR Admin deactivates a leave type', async ({ page, request }) => {
  const jwt = await apiLogin(request, ADMIN_EMAIL, ADMIN_PASSWORD);
  expect(jwt).not.toBeNull();

  // Pre-create a type via API so we have something to deactivate
  const deactivateName = `Temp Leave ${Date.now()}`;
  await request.post(`${API_URL}/api/v1/leave-types`, {
    data: {
      name: deactivateName,
      code: `TL${Date.now()}`.slice(0, 8).toUpperCase(),
      annualDays: 1,
      requiresAttachment: false,
      requiresHrApproval: false,
    },
    headers: { Authorization: `Bearer ${jwt}` },
  });

  await page.goto(`${BASE_URL}/login`);
  await page.getByLabel(/email/i).fill(ADMIN_EMAIL);
  await page.getByLabel(/password/i).fill(ADMIN_PASSWORD);
  await page.getByRole('button', { name: /login|sign in/i }).click();
  await page.waitForURL(/\/admin|\/dashboard/, { timeout: 10_000 });

  await page.goto(`${BASE_URL}/admin/leave-types`);
  await page.waitForSelector('[data-testid="leave-type-table"]', { timeout: 10_000 });
  await expect(page.getByText(deactivateName)).toBeVisible({ timeout: 8_000 });

  // Click the deactivate button for that row
  const row = page.locator('.MuiDataGrid-row').filter({ hasText: deactivateName });
  await row.getByTestId('deactivate-leave-type-btn').click();

  // Success confirmation
  await expect(page.getByTestId('success-snackbar')).toBeVisible({ timeout: 8_000 });
});

// ── E2E-022: Employee cannot access leave type management ─────────────────────

test('E2E-022: Employee cannot access leave type management page', async ({ page }) => {
  await page.goto(`${BASE_URL}/login`);
  await page.getByLabel(/email/i).fill(USER_EMAIL);
  await page.getByLabel(/password/i).fill(USER_PASSWORD);
  await page.getByRole('button', { name: /login|sign in/i }).click();
  await page.waitForURL(/\/dashboard|\/profile|\/team/, { timeout: 10_000 });

  // Employees are not linked to /admin/leave-types — navigating redirects away
  await page.goto(`${BASE_URL}/admin/leave-types`);

  // Should be redirected to login (ProtectedRoute) or show no admin UI
  await expect(page).not.toHaveURL(/leave-types/, { timeout: 5_000 });
});
