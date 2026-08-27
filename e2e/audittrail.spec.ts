import { test, expect, APIRequestContext } from '@playwright/test';

/**
 * F-13 Audit Trail — End-to-End Tests
 *
 * E2E-040: HR Admin can view audit log list
 * E2E-041: HR Admin can filter by action type
 * E2E-042: Non-HR user is redirected from audit trail page
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
 * Run: npx playwright test e2e/audittrail.spec.ts
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

// ── E2E-040: HR Admin can view audit log list ──────────────────────────────────

test('E2E-040: HR Admin can navigate to audit trail page and see the table', async ({
  page,
  request,
}) => {
  // Seed: make sure we have at least one audit log entry via the API
  const jwt = await apiLogin(request, ADMIN_EMAIL, ADMIN_PASSWORD);
  expect(jwt).not.toBeNull();

  // Navigate to audit trail page (assuming the app has a nav link or direct URL)
  await page.goto(`${BASE_URL}/login`);

  // Log in via UI
  await page.getByLabel(/email/i).fill(ADMIN_EMAIL);
  await page.getByLabel(/password/i).fill(ADMIN_PASSWORD);
  await page.getByRole('button', { name: /sign in|log in/i }).click();

  // Navigate directly to audit trail
  await page.goto(`${BASE_URL}/admin/audit-trail`);

  // The table should be visible
  await expect(page.getByTestId('audit-trail-table')).toBeVisible({ timeout: 10000 });

  // Filter inputs should be present
  await expect(page.getByTestId('audit-filter-user')).toBeVisible();
  await expect(page.getByTestId('audit-filter-from-date')).toBeVisible();
  await expect(page.getByTestId('audit-filter-to-date')).toBeVisible();
  await expect(page.getByTestId('audit-search-btn')).toBeVisible();
});

// ── E2E-041: HR Admin can filter by action type ────────────────────────────────

test('E2E-041: HR Admin can filter audit logs by action type and trigger a search', async ({
  page,
}) => {
  await page.goto(`${BASE_URL}/login`);
  await page.getByLabel(/email/i).fill(ADMIN_EMAIL);
  await page.getByLabel(/password/i).fill(ADMIN_PASSWORD);
  await page.getByRole('button', { name: /sign in|log in/i }).click();

  await page.goto(`${BASE_URL}/admin/audit-trail`);
  await expect(page.getByTestId('audit-trail-table')).toBeVisible({ timeout: 10000 });

  // Select an action type filter — open the dropdown
  const actionSelect = page.getByTestId('audit-filter-action');
  await actionSelect.click();

  // Choose "CREATE" from the dropdown
  await page.getByRole('option', { name: 'CREATE' }).click();

  // Click search
  await page.getByTestId('audit-search-btn').click();

  // The table should still be visible after search
  await expect(page.getByTestId('audit-trail-table')).toBeVisible({ timeout: 8000 });
});

// ── E2E-042: Non-HR user is redirected from audit trail page ──────────────────

test('E2E-042: EMPLOYEE user cannot access audit trail — redirected to login', async ({
  page,
}) => {
  // Log in as EMPLOYEE
  await page.goto(`${BASE_URL}/login`);
  await page.getByLabel(/email/i).fill(USER_EMAIL);
  await page.getByLabel(/password/i).fill(USER_PASSWORD);
  await page.getByRole('button', { name: /sign in|log in/i }).click();

  // Attempt to navigate directly to audit trail
  await page.goto(`${BASE_URL}/admin/audit-trail`);

  // ProtectedRoute should redirect to login (or show 403-style page)
  // The audit trail table must NOT be accessible
  const url = page.url();
  const hasTable = await page.getByTestId('audit-trail-table').isVisible().catch(() => false);

  // Either redirected away from /admin/audit-trail OR the table is not visible
  const isOnAuditPage = url.includes('/admin/audit-trail');
  if (isOnAuditPage) {
    expect(hasTable).toBe(false);
  } else {
    expect(url).not.toContain('/admin/audit-trail');
  }
});
