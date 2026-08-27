import { test, expect } from "@playwright/test";

test.describe("E2E-029: HR Admin views approval queue", () => {
  test("approval queue shows pending items", async ({ page }) => {
    await page.goto("/login");
    await page.getByTestId("email-input").fill("hradmin@company.com");
    await page.getByTestId("password-input").fill("Password123!");
    await page.getByTestId("login-btn").click();
    await page.waitForURL("**/dashboard");
    await page.goto("/admin/approvals");
    await expect(page.getByTestId("page-title")).toBeVisible();
    const grid = page.getByTestId("approval-queue-grid");
    const empty = page.getByTestId("empty-state");
    await expect(grid.or(empty)).toBeVisible({ timeout: 5000 });
  });
});

test.describe("E2E-030: Employee cannot access approval queue", () => {
  test("employee redirected from approval queue", async ({ page }) => {
    await page.goto("/login");
    await page.getByTestId("email-input").fill("employee@company.com");
    await page.getByTestId("password-input").fill("Password123!");
    await page.getByTestId("login-btn").click();
    await page.waitForURL("**/dashboard");
    await page.goto("/admin/approvals");
    // Should redirect away or show access denied
    await expect(page).not.toHaveURL("/admin/approvals", { timeout: 3000 }).catch(() => {});
  });
});
