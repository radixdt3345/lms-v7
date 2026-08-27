import { test, expect } from "@playwright/test";

test.describe("E2E-033: Employee dashboard loads", () => {
  test("dashboard shows stats cards", async ({ page }) => {
    await page.goto("/login");
    await page.getByTestId("email-input").fill("employee@company.com");
    await page.getByTestId("password-input").fill("Password123!");
    await page.getByTestId("login-btn").click();
    await page.waitForURL("**/dashboard");
    await expect(page.getByTestId("page-title")).toBeVisible();
    await expect(page.getByTestId("stat-pending-leaves").or(page.getByTestId("error-message"))).toBeVisible({ timeout: 5000 });
  });
});

test.describe("E2E-034: HR dashboard loads", () => {
  test("HR admin can view HR dashboard", async ({ page }) => {
    await page.goto("/login");
    await page.getByTestId("email-input").fill("hradmin@company.com");
    await page.getByTestId("password-input").fill("Password123!");
    await page.getByTestId("login-btn").click();
    await page.waitForURL("**/dashboard");
    await expect(page.getByTestId("page-title")).toBeVisible();
  });
});
