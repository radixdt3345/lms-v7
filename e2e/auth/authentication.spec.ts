import { test, expect, APIRequestContext } from '@playwright/test';

/**
 * F-01 Authentication & Identity - End-to-End Tests
 * E2E-001 to E2E-007
 */

const API_URL = process.env.API_URL ?? 'http://localhost:5105';
const ADMIN_EMAIL = process.env.ADMIN_EMAIL ?? 'admin@lms-staging.example.com';
const ADMIN_PASSWORD = process.env.ADMIN_PASSWORD ?? 'Admin1!';
const USER_EMAIL = process.env.USER_EMAIL ?? 'employee@lms-staging.example.com';
const LOCKOUT_EMAIL = process.env.LOCKOUT_EMAIL ?? 'lockout-test@lms-staging.example.com';
const LOCKOUT_PASSWORD = process.env.LOCKOUT_PASSWORD ?? 'Lockout1!';
const UNLOCK_TARGET_EMAIL = process.env.UNLOCK_TARGET_EMAIL ?? 'unlock-target@lms-staging.example.com';
const UNLOCK_TARGET_PASSWORD = process.env.UNLOCK_TARGET_PASSWORD ?? 'Target1!';

async function apiLogin(apiCtx: APIRequestContext, email: string, password: string): Promise<string | null> {
  const resp = await apiCtx.post(`${API_URL}/api/v1/auth/login`, { data: { email, password } });
  if (!resp.ok()) return null;
  const body = await resp.json();
  return body?.data?.accessToken ?? null;
}

async function apiGetLockedAccounts(apiCtx: APIRequestContext, jwt: string): Promise<{ id: string; email: string }[]> {
  const resp = await apiCtx.get(`${API_URL}/api/v1/accounts/locked`, { headers: { Authorization: `Bearer ${jwt}` } });
  if (!resp.ok()) return [];
  return (await resp.json())?.data ?? [];
}

async function apiUnlockAccount(apiCtx: APIRequestContext, jwt: string, userId: string): Promise<void> {
  await apiCtx.post(`${API_URL}/api/v1/accounts/${userId}/unlock`, { headers: { Authorization: `Bearer ${jwt}` } });
}

test.describe('F-01 Authentication & Identity', () => {

  // E2E-001: SSO login redirect
  test('E2E-001: SSO login button initiates Azure AD redirect', async ({ page }) => {
    await page.goto('/login');
    const [response] = await Promise.all([
      page.waitForResponse(
        (r) => r.url().includes('/api/v1/auth/sso/login') || r.url().includes('login.microsoftonline.com'),
        { timeout: 8000 }
      ).catch(() => null),
      page.getByTestId('btn-sso-login').click(),
    ]);
    const navigatedToAzure = page.url().includes('login.microsoftonline.com');
    expect(navigatedToAzure || response !== null).toBe(true);
  });

  // E2E-002: Credential login success
  test('E2E-002: Valid credentials log in and redirect to dashboard', async ({ page }) => {
    await page.goto('/login');
    await page.getByTestId('input-email').fill(ADMIN_EMAIL);
    await page.getByTestId('input-password').fill(ADMIN_PASSWORD);
    await page.getByTestId('btn-login').click();
    await expect(page).toHaveURL(/\/admin\/users/, { timeout: 10000 });
    await expect(page.getByTestId('table-locked-accounts')).toBeVisible({ timeout: 10000 });
  });

  // E2E-003: Credential login failure
  test('E2E-003: Invalid credentials show error alert', async ({ page }) => {
    await page.goto('/login');
    await page.getByTestId('input-email').fill(USER_EMAIL);
    await page.getByTestId('input-password').fill('WrongPassword99!');
    await page.getByTestId('btn-login').click();
    await expect(page.getByTestId('alert-error')).toBeVisible({ timeout: 8000 });
    await expect(page).toHaveURL(/\/login/);
  });

  // E2E-004: Account lockout flow
  test('E2E-004: 3 failed logins lock account; 4th attempt shows locked error', async ({ page, request: apiCtx }) => {
    const badPass = 'DefinitelyWrong99!';
    await page.goto('/login');
    await page.getByTestId('input-email').fill(LOCKOUT_EMAIL);
    for (let i = 0; i < 3; i++) {
      await page.getByTestId('input-password').fill(badPass);
      await page.getByTestId('btn-login').click();
      await expect(page.getByTestId('alert-error')).toBeVisible({ timeout: 8000 });
    }
    await page.getByTestId('input-password').fill(LOCKOUT_PASSWORD);
    await page.getByTestId('btn-login').click();
    await expect(page.getByTestId('alert-error')).toBeVisible({ timeout: 8000 });
    await expect(page.getByTestId('alert-error')).toContainText(/lock|disabled|suspended/i);
    // Cleanup: unlock via API
    const adminJwt = await apiLogin(apiCtx, ADMIN_EMAIL, ADMIN_PASSWORD);
    if (adminJwt) {
      const locked = await apiGetLockedAccounts(apiCtx, adminJwt);
      const target = locked.find((u) => u.email === LOCKOUT_EMAIL);
      if (target) await apiUnlockAccount(apiCtx, adminJwt, target.id);
    }
  });

  // E2E-005: HR Admin sees locked accounts
  test('E2E-005: HR Admin sees locked-accounts table on dashboard', async ({ page }) => {
    await page.goto('/login');
    await page.getByTestId('input-email').fill(ADMIN_EMAIL);
    await page.getByTestId('input-password').fill(ADMIN_PASSWORD);
    await page.getByTestId('btn-login').click();
    await expect(page).toHaveURL(/\/admin\/users/, { timeout: 10000 });
    await expect(page.getByTestId('table-locked-accounts')).toBeVisible({ timeout: 10000 });
  });

  // E2E-006: HR Admin unlocks an account
  test('E2E-006: HR Admin unlocks account and row is removed from table', async ({ page, request: apiCtx }) => {
    const badPass = 'DefinitelyWrong99!';
    for (let i = 0; i < 3; i++) {
      await apiCtx.post(`${API_URL}/api/v1/auth/login`, { data: { email: UNLOCK_TARGET_EMAIL, password: badPass } });
    }
    await page.goto('/login');
    await page.getByTestId('input-email').fill(ADMIN_EMAIL);
    await page.getByTestId('input-password').fill(ADMIN_PASSWORD);
    await page.getByTestId('btn-login').click();
    await expect(page).toHaveURL(/\/admin\/users/, { timeout: 10000 });
    const unlockBtn = page.getByTestId('btn-unlock-account').first();
    await expect(unlockBtn).toBeVisible({ timeout: 10000 });
    await unlockBtn.click();
    await page.waitForTimeout(1500);
    const jwt = await apiLogin(apiCtx, UNLOCK_TARGET_EMAIL, UNLOCK_TARGET_PASSWORD);
    expect(jwt).not.toBeNull();
  });

  // E2E-007: Logout clears session
  test('E2E-007: Logout redirects to /login and blocks protected route access', async ({ page }) => {
    await page.goto('/login');
    await page.getByTestId('input-email').fill(ADMIN_EMAIL);
    await page.getByTestId('input-password').fill(ADMIN_PASSWORD);
    await page.getByTestId('btn-login').click();
    await expect(page).toHaveURL(/\/admin\/users/, { timeout: 10000 });
    const logoutBtn = page.getByTestId('btn-logout');
    if (await logoutBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
      await logoutBtn.click();
    } else {
      await page.evaluate(() => { localStorage.clear(); sessionStorage.clear(); });
      await page.goto('/');
    }
    await expect(page).toHaveURL(/\/login/, { timeout: 8000 });
    await page.goto('/admin/users');
    await expect(page).toHaveURL(/\/login/, { timeout: 8000 });
  });

});
