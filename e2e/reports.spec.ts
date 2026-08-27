import { test, expect } from "@playwright/test";

// E2E-035: HR Admin can see reports page
test("E2E-035: HR Admin sees reports page with download buttons", async ({ page }) => {
  await page.goto("/login");
  await page.getByTestId("email-input").fill("hradmin@company.com");
  await page.getByTestId("password-input").fill("Test@1234");
  await page.getByTestId("login-btn").click();
  await page.goto("/admin/reports");
  await expect(page.getByTestId("page-title")).toBeVisible();
  await expect(page.getByTestId("download-leave-report")).toBeVisible();
  await expect(page.getByTestId("download-compoff-report")).toBeVisible();
  await expect(page.getByTestId("download-balance-report")).toBeVisible();
});

// E2E-036: Download leave report triggers file download
test("E2E-036: Download leave report triggers CSV download", async ({ page }) => {
  await page.goto("/login");
  await page.getByTestId("email-input").fill("hradmin@company.com");
  await page.getByTestId("password-input").fill("Test@1234");
  await page.getByTestId("login-btn").click();
  await page.goto("/admin/reports");
  const [download] = await Promise.all([
    page.waitForEvent("download"),
    page.getByTestId("download-leave-report").click(),
  ]);
  expect(download.suggestedFilename()).toContain("leave-report");
});
