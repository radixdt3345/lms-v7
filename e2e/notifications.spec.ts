import { test, expect } from "@playwright/test";

test.describe("E2E-031: Notification bell visible and functional", () => {
  test("notification bell shows for authenticated user", async ({ page }) => {
    await page.goto("/login");
    await page.getByTestId("email-input").fill("employee@company.com");
    await page.getByTestId("password-input").fill("Password123!");
    await page.getByTestId("login-btn").click();
    await page.waitForURL("**/dashboard");
    await expect(page.getByTestId("notification-bell")).toBeVisible();
  });
});

test.describe("E2E-032: Notification popover opens and shows content", () => {
  test("clicking bell opens notification popover", async ({ page }) => {
    await page.goto("/login");
    await page.getByTestId("email-input").fill("employee@company.com");
    await page.getByTestId("password-input").fill("Password123!");
    await page.getByTestId("login-btn").click();
    await page.waitForURL("**/dashboard");
    await page.getByTestId("notification-bell").click();
    const popover = page.getByTestId("notification-popover");
    await expect(popover).toBeVisible({ timeout: 3000 });
    const noNotif = page.getByTestId("no-notifications");
    const item = page.getByTestId("notification-item");
    await expect(noNotif.or(item)).toBeVisible({ timeout: 3000 });
  });
});
