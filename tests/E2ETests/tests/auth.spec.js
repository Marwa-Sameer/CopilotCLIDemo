import { test, expect } from '@playwright/test';

test.describe('Authentication Tests', () => {
  let uniqueEmail;
  const testPassword = 'Test123!@#';

  test.beforeEach(() => {
    // Generate unique email using timestamp to avoid conflicts
    const timestamp = Date.now();
    uniqueEmail = `testuser${timestamp}@example.com`;
  });

  test('should register a new user', async ({ page }) => {
    // Navigate to registration page
    await page.goto('/Identity/Account/Register');
    
    // Wait for the page to load
    await page.waitForSelector('input[name="Input.Email"]');
    
    // Fill in registration form
    await page.fill('input[name="Input.Email"]', uniqueEmail);
    await page.fill('input[name="Input.Password"]', testPassword);
    await page.fill('input[name="Input.ConfirmPassword"]', testPassword);
    
    // Submit the form
    await page.click('button[type="submit"]');
    
    // Wait for navigation after successful registration
    await page.waitForURL(/\/(Identity\/Account\/RegisterConfirmation|\/)/);
    
    // Verify successful registration (user should be redirected)
    const url = page.url();
    expect(url).toMatch(/\/(Identity\/Account\/RegisterConfirmation|\/)/);
  });

  test('should login with created user', async ({ page }) => {
    // First, register a new user
    await page.goto('/Identity/Account/Register');
    await page.waitForSelector('input[name="Input.Email"]');
    await page.fill('input[name="Input.Email"]', uniqueEmail);
    await page.fill('input[name="Input.Password"]', testPassword);
    await page.fill('input[name="Input.ConfirmPassword"]', testPassword);
    await page.click('button[type="submit"]');
    await page.waitForURL(/\/(Identity\/Account\/RegisterConfirmation|\/)/);
    
    // Logout if auto-logged in
    const logoutButton = page.locator('form[action="/Identity/Account/Logout"] button, a:has-text("Logout")');
    if (await logoutButton.count() > 0) {
      await logoutButton.first().click();
      await page.waitForTimeout(1000);
    }
    
    // Navigate to login page
    await page.goto('/Identity/Account/Login');
    await page.waitForSelector('input[name="Input.Email"]');
    
    // Fill in login form
    await page.fill('input[name="Input.Email"]', uniqueEmail);
    await page.fill('input[name="Input.Password"]', testPassword);
    
    // Submit login form
    await page.click('button[type="submit"]');
    
    // Wait for successful login redirect
    await page.waitForURL('/');
    
    // Verify user is logged in by checking for email or logout button
    const userEmail = await page.locator(`text=${uniqueEmail}`).count();
    const logoutForm = await page.locator('form[action="/Identity/Account/Logout"]').count();
    expect(userEmail + logoutForm).toBeGreaterThan(0);
  });

  test('should logout successfully', async ({ page }) => {
    // Register and login first
    await page.goto('/Identity/Account/Register');
    await page.waitForSelector('input[name="Input.Email"]');
    await page.fill('input[name="Input.Email"]', uniqueEmail);
    await page.fill('input[name="Input.Password"]', testPassword);
    await page.fill('input[name="Input.ConfirmPassword"]', testPassword);
    await page.click('button[type="submit"]');
    await page.waitForURL(/\/(Identity\/Account\/RegisterConfirmation|\/)/);
    
    // Find and click logout button
    const logoutButton = page.locator('form[action="/Identity/Account/Logout"] button').first();
    await logoutButton.click();
    
    // Wait for logout to complete
    await page.waitForTimeout(1000);
    
    // Verify logout by checking for Login/Register links
    await page.goto('/');
    const loginLink = await page.locator('a:has-text("Login"), a[href*="Login"]').count();
    const registerLink = await page.locator('a:has-text("Register"), a[href*="Register"]').count();
    expect(loginLink + registerLink).toBeGreaterThan(0);
  });

  test('should fail login with invalid credentials', async ({ page }) => {
    // Navigate to login page
    await page.goto('/Identity/Account/Login');
    await page.waitForSelector('input[name="Input.Email"]');
    
    // Fill in invalid credentials
    await page.fill('input[name="Input.Email"]', 'invalid@example.com');
    await page.fill('input[name="Input.Password"]', 'WrongPassword123!');
    
    // Submit login form
    await page.click('button[type="submit"]');
    
    // Wait a moment for validation
    await page.waitForTimeout(1000);
    
    // Verify error message appears or still on login page
    const url = page.url();
    expect(url).toContain('/Identity/Account/Login');
    
    // Check for validation error
    const errorMessage = await page.locator('.text-danger, .validation-summary-errors, [role="alert"]').count();
    expect(errorMessage).toBeGreaterThan(0);
  });
});
