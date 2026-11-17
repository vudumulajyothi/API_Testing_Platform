import { test, expect } from '@playwright/test';

test.describe('TestPilot Platform End-to-End Tests', () => {

  test('Unauthenticated user should be redirected to login', async ({ page }) => {
    await page.goto('/');
    await expect(page).toHaveURL(/.*\/auth\/login/);
    await expect(page.locator('h2')).toContainText('Sign In to TestPilot');
  });

  test('User can navigate to registration page', async ({ page }) => {
    await page.goto('/auth/login');
    await page.click('text=Create one');
    await expect(page).toHaveURL(/.*\/auth\/register/);
    await expect(page.locator('h2')).toContainText('Create an Account');
  });

  test('Request Builder UI executes and renders response inspect pane', async ({ page }) => {
    await page.addInitScript(() => {
      localStorage.setItem('testpilot_token', 'mock-valid-jwt-token');
      localStorage.setItem('testpilot_user', JSON.stringify({ userId: '123', name: 'Tester', email: 'test@example.com' }));
    });

    await page.goto('/request-builder');
    await expect(page.locator('.request-bar')).toBeVisible();

    const urlInput = page.locator('.url-input');
    await expect(urlInput).toBeVisible();

    const sendBtn = page.locator('button.send-btn');
    await expect(sendBtn).toBeVisible();
    await sendBtn.click();

    const responsePane = page.locator('.response-pane');
    await expect(responsePane).toBeVisible();
  });

  test('Test Runner executes assertions and updates status badges', async ({ page }) => {
    await page.addInitScript(() => {
      localStorage.setItem('testpilot_token', 'mock-valid-jwt-token');
      localStorage.setItem('testpilot_user', JSON.stringify({ userId: '123', name: 'Tester', email: 'test@example.com' }));
    });

    await page.goto('/test-runner');
    await expect(page.locator('h1')).toContainText('Automated Test Runner');

    const runBtn = page.locator('button:has-text("Run Test Suite")');
    await expect(runBtn).toBeVisible();
    await runBtn.click();

    const firstStatusBadge = page.locator('.test-table .badge-status').first();
    await expect(firstStatusBadge).not.toHaveText('PENDING', { timeout: 10000 });
  });

});
