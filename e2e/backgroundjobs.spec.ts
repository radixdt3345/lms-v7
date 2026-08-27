import { test, expect } from "@playwright/test";

// E2E-037: HR Admin can see background jobs page
test("E2E-037: HR Admin sees background jobs page with trigger buttons", async ({ page }) => {
  await page.goto("/login");
  await page.getByTestId("email-input").fill("hradmin@company.com");
  await page.getByTestId("password-input").fill("Test@1234");
  await page.getByTestId("login-btn").click();
  await page.goto("/admin/jobs");
  await expect(page.getByTestId("page-title")).toBeVisible();
  await expect(page.getByTestId("trigger-expire-compoff")).toBeVisible();
  await expect(page.getByTestId("trigger-reset-balances")).toBeVisible();
  await expect(page.getByTestId("trigger-send-reminders")).toBeVisible();
});

// E2E-038: Triggering a job shows success message
test("E2E-038: Triggering expire comp-off shows success", async ({ page }) => {
  await page.goto("/login");
  await page.getByTestId("email-input").fill("hradmin@company.com");
  await page.getByTestId("password-input").fill("Test@1234");
  await page.getByTestId("login-btn").click();
  await page.goto("/admin/jobs");
  await page.getByTestId("trigger-expire-compoff").click();
  await expect(page.getByTestId("success-message")).toBeVisible({ timeout: 10000 });
});
