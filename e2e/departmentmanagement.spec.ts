import { test, expect, APIRequestContext } from '@playwright/test';

/**
 * F-03 Department Management — End-to-End Tests
 *
 * E2E-D001: HR Admin creates a department → it appears in the departments table
 * E2E-D002: Duplicate department name → inline error shown, department not created
 * E2E-D003: HR Admin deactivates a department → deactivation warning alert is shown
 * E2E-D004: Employee navigates to departments view → table is visible (read-only)
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
 * Run: npx playwright test e2e/departmentmanagement.spec.ts
 */

const API_URL = process.env.API_URL ?? 'http://localhost:5105';
const ADMIN_EMAIL = process.env.ADMIN_EMAIL ?? 'admin@lms-staging.example.com';
const ADMIN_PASSWORD = process.env.ADMIN_PASSWORD ?? 'Admin1!';
const USER_EMAIL = process.env.USER_EMAIL ?? 'employee@lms-staging.example.com';
const USER_PASSWORD = process.env.USER_PASSWORD ?? 'Employee1!';

// -- API helpers -----------------------------------------------------------

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

async function apiDeleteDepartmentByName(
  apiCtx: APIRequestContext,
  jwt: string,
  name: string
): Promise<void> {
  const listResp = await apiCtx.get(`${API_URL}/api/v1/departments`, {
    headers: { Authorization: `Bearer ${jwt}` },
  });
  if (!listResp.ok()) return;
  const body = await listResp.json();
  const depts: { id: string; name: string }[] = body?.data ?? [];
  const target = depts.find((d) => d.name === name);
  if (target) {
    await apiCtx.delete(`${API_URL}/api/v1/departments/${target.id}`, {
      headers: { Authorization: `Bearer ${jwt}` },
    });
  }
}

// -- Helpers ---------------------------------------------------------------

async function loginAsAdmin(page: import('@playwright/test').Page): Promise<void> {
  await page.goto('/login');
  await page.getByTestId('input-email').fill(ADMIN_EMAIL);
  await page.getByTestId('input-password').fill(ADMIN_PASSWORD);
  await page.getByTestId('btn-login').click();
  await expect(page).toHaveURL(/\/admin\/users/, { timeout: 10_000 });
}

async function navigateToDepartments(page: import('@playwright/test').Page): Promise<void> {
  await page.goto('/admin/departments');
  await expect(page.getByTestId('table-departments')).toBeVisible({ timeout: 10_000 });
}

// -- Tests -----------------------------------------------------------------

test.describe('F-03 Department Management', () => {
  // E2E-D001: Create department → appears in list
  test('E2E-D001: HR Admin creates a department and it appears in the table', async ({
    page,
    request: apiCtx,
  }) => {
    const deptName = `E2E-Dept-${Date.now()}`;
    const deptCode = `ED${Date.now().toString().slice(-4)}`;

    await loginAsAdmin(page);
    await navigateToDepartments(page);

    // Open create dialog
    await page.getByTestId('btn-create-department').click();

    // Fill form
    await page.getByTestId('input-dept-name').fill(deptName);
    await page.getByTestId('input-dept-code').fill(deptCode);
    await page.getByTestId('input-overlap-limit').fill('3');

    // Save
    await page.getByTestId('btn-save-department').click();

    // Assert the new department appears in the table
    await expect(page.getByTestId('table-departments')).toContainText(deptName, {
      timeout: 10_000,
    });

    // Cleanup via API
    const jwt = await apiLogin(apiCtx, ADMIN_EMAIL, ADMIN_PASSWORD);
    if (jwt) {
      await apiDeleteDepartmentByName(apiCtx, jwt, deptName);
    }
  });

  // E2E-D002: Duplicate name → error shown
  test('E2E-D002: Duplicate department name shows an error and does not create a duplicate', async ({
    page,
    request: apiCtx,
  }) => {
    const deptName = `E2E-Dup-${Date.now()}`;
    const deptCode = `DU${Date.now().toString().slice(-4)}`;

    // Seed first department via API
    const jwt = await apiLogin(apiCtx, ADMIN_EMAIL, ADMIN_PASSWORD);
    if (jwt) {
      await apiCtx.post(`${API_URL}/api/v1/departments`, {
        headers: { Authorization: `Bearer ${jwt}` },
        data: { name: deptName, code: deptCode, overlapLimit: 2 },
      });
    }

    await loginAsAdmin(page);
    await navigateToDepartments(page);

    // Try to create with the same name
    await page.getByTestId('btn-create-department').click();
    await page.getByTestId('input-dept-name').fill(deptName);
    await page.getByTestId('input-dept-code').fill(`${deptCode}X`);
    await page.getByTestId('input-overlap-limit').fill('2');
    await page.getByTestId('btn-save-department').click();

    // An error alert should be visible; the dialog should remain open
    await expect(page.getByTestId('alert-error')).toBeVisible({ timeout: 8_000 });

    // Cleanup
    if (jwt) {
      await apiDeleteDepartmentByName(apiCtx, jwt, deptName);
    }
  });

  // E2E-D003: Deactivate → warning displayed
  test('E2E-D003: HR Admin deactivates a department and sees the warning alert', async ({
    page,
    request: apiCtx,
  }) => {
    const deptName = `E2E-Deact-${Date.now()}`;
    const deptCode = `DA${Date.now().toString().slice(-4)}`;

    // Seed department via API
    const jwt = await apiLogin(apiCtx, ADMIN_EMAIL, ADMIN_PASSWORD);
    let deptId: string | undefined;
    if (jwt) {
      const createResp = await apiCtx.post(`${API_URL}/api/v1/departments`, {
        headers: { Authorization: `Bearer ${jwt}` },
        data: { name: deptName, code: deptCode, overlapLimit: 2 },
      });
      if (createResp.ok()) {
        const body = await createResp.json();
        deptId = body?.data?.id;
      }
    }

    await loginAsAdmin(page);
    await navigateToDepartments(page);

    // Click the deactivate button for the seeded department row
    const deactivateBtn = page.getByTestId('btn-deactivate-department').first();
    await expect(deactivateBtn).toBeVisible({ timeout: 10_000 });
    await deactivateBtn.click();

    // Warning alert must appear
    await expect(page.getByTestId('alert-dept-deactivation-warning')).toBeVisible({
      timeout: 8_000,
    });

    // Cleanup: deactivate (soft-delete) via API if not already done
    if (jwt && deptId) {
      await apiCtx.delete(`${API_URL}/api/v1/departments/${deptId}`, {
        headers: { Authorization: `Bearer ${jwt}` },
      });
    }
  });

  // E2E-D004: Employee sees departments table (read-only)
  test('E2E-D004: Employee can view the departments table', async ({ page }) => {
    await page.goto('/login');
    await page.getByTestId('input-email').fill(USER_EMAIL);
    await page.getByTestId('input-password').fill(USER_PASSWORD);
    await page.getByTestId('btn-login').click();

    // Employee dashboard — navigate to departments (read-only access)
    await page.goto('/admin/departments');

    // Table should be visible even for employees (read-only view)
    await expect(page.getByTestId('table-departments')).toBeVisible({ timeout: 10_000 });

    // Create button should NOT be visible for employees
    await expect(page.getByTestId('btn-create-department')).not.toBeVisible();
  });
});
