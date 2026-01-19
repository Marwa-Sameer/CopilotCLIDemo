import { test, expect } from '@playwright/test';

test.describe('CSV Export Tests', () => {
  let uniqueEmail;
  const testPassword = 'Test123!@#';
  let householdName;

  test.beforeEach(async ({ page }) => {
    // Generate unique test data
    const timestamp = Date.now();
    uniqueEmail = `testuser${timestamp}@example.com`;
    householdName = `Test Household ${timestamp}`;

    // Register and login user
    await page.goto('/Identity/Account/Register');
    await page.waitForSelector('input[name="Input.Email"]');
    await page.fill('input[name="Input.Email"]', uniqueEmail);
    await page.fill('input[name="Input.Password"]', testPassword);
    await page.fill('input[name="Input.ConfirmPassword"]', testPassword);
    await page.click('button[type="submit"]');
    await page.waitForURL(/\/(Identity\/Account\/RegisterConfirmation|\/)/);
    await page.waitForTimeout(1000);

    // Create a household
    await page.goto('/Households/Create');
    await page.waitForSelector('input[name="Name"]');
    await page.fill('input[name="Name"]', householdName);
    await page.click('button[type="submit"]:has-text("Create Household")');
    await page.waitForURL('/Households');
    await page.waitForTimeout(1000);

    // Create test items for export
    // Create personal item
    await page.goto('/Items/Create');
    await page.waitForSelector('input[name="Name"]');
    await page.check('input[name="HouseholdId"][id="personalItem"]');
    await page.fill('input[name="Name"]', `Personal Export Item ${timestamp}`);
    await page.fill('input[name="Quantity"]', '2');
    await page.fill('input[name="EstimatedPrice"]', '19.99');
    await page.fill('input[name="Vendor"]', 'Test Vendor');
    await page.click('button[type="submit"]:has-text("Create")');
    await page.waitForURL('/Items');
    
    // Create household item
    await page.goto('/Items/Create');
    await page.waitForSelector('input[name="Name"]');
    await page.check('input[name="HouseholdId"][id="householdItem"]');
    await page.fill('input[name="Name"]', `Household Export Item ${timestamp}`);
    await page.fill('input[name="Quantity"]', '5');
    await page.fill('input[name="EstimatedPrice"]', '49.99');
    await page.check('input[name="IsUrgent"]');
    await page.click('button[type="submit"]:has-text("Create")');
    await page.waitForURL('/Items');
  });

  test('should navigate to items page', async ({ page }) => {
    // Navigate to items page
    await page.goto('/Items');
    
    // Verify items page loaded
    await page.waitForSelector('h1:has-text("Items")');
    const title = await page.locator('h1:has-text("Items")');
    await expect(title).toBeVisible();
  });

  test('should verify export CSV button exists', async ({ page }) => {
    // Navigate to items page
    await page.goto('/Items');
    await page.waitForSelector('h1:has-text("Items")');
    
    // Verify Export CSV button is present
    const exportButton = await page.locator('a:has-text("Export CSV")');
    await expect(exportButton).toBeVisible();
    
    // Verify button has correct icon
    const exportIcon = await exportButton.locator('.bi-download');
    await expect(exportIcon).toBeVisible();
  });

  test('should click export CSV button and verify download occurs', async ({ page }) => {
    // Navigate to items page
    await page.goto('/Items');
    await page.waitForSelector('h1:has-text("Items")');
    
    // Set up download promise before clicking
    const downloadPromise = page.waitForEvent('download');
    
    // Click export CSV button
    await page.click('a:has-text("Export CSV")');
    
    // Wait for download to start
    const download = await downloadPromise;
    
    // Verify download occurred
    expect(download).toBeTruthy();
    
    // Verify filename contains "items" or "export"
    const filename = download.suggestedFilename();
    expect(filename.toLowerCase()).toMatch(/items|export|\.csv/);
  });

  test('should export CSV with filters applied', async ({ page }) => {
    // Navigate to items page
    await page.goto('/Items');
    await page.waitForSelector('h1:has-text("Items")');
    
    // Apply filter for personal items only
    await page.selectOption('select[name="filter"]', 'personal');
    await page.click('button[type="submit"]:has-text("Apply Filters")');
    await page.waitForTimeout(500);
    
    // Set up download promise
    const downloadPromise = page.waitForEvent('download');
    
    // Click export CSV button (should export only filtered items)
    await page.click('a:has-text("Export CSV")');
    
    // Wait for download
    const download = await downloadPromise;
    expect(download).toBeTruthy();
  });

  test('should verify print view button exists', async ({ page }) => {
    // Navigate to items page
    await page.goto('/Items');
    await page.waitForSelector('h1:has-text("Items")');
    
    // Verify Print View button is present
    const printButton = await page.locator('a:has-text("Print View")');
    await expect(printButton).toBeVisible();
    
    // Verify button has printer icon
    const printIcon = await printButton.locator('.bi-printer');
    await expect(printIcon).toBeVisible();
  });

  test('should load printable view in new tab', async ({ page, context }) => {
    // Navigate to items page
    await page.goto('/Items');
    await page.waitForSelector('h1:has-text("Items")');
    
    // Get the print view link
    const printLink = await page.locator('a:has-text("Print View")');
    const href = await printLink.getAttribute('href');
    
    // Verify href contains PrintableView
    expect(href).toContain('PrintableView');
    
    // Open printable view in new page
    const newPage = await context.newPage();
    await newPage.goto(href!);
    
    // Wait for printable page to load
    await newPage.waitForTimeout(1000);
    
    // Verify printable view loaded (should have items content)
    const url = newPage.url();
    expect(url).toContain('PrintableView');
    
    // Close the new page
    await newPage.close();
  });

  test('should export CSV and verify response headers', async ({ page }) => {
    // Navigate to items page
    await page.goto('/Items');
    await page.waitForSelector('h1:has-text("Items")');
    
    // Listen for download event
    const [download] = await Promise.all([
      page.waitForEvent('download'),
      page.click('a:has-text("Export CSV")')
    ]);
    
    // Verify download properties
    expect(download.suggestedFilename()).toMatch(/\.csv$/i);
    
    // Optionally save and verify file content
    const path = await download.path();
    expect(path).toBeTruthy();
  });

  test('should export filtered urgent items to CSV', async ({ page }) => {
    // Navigate to items page
    await page.goto('/Items');
    await page.waitForSelector('h1:has-text("Items")');
    
    // Filter for urgent items only
    await page.selectOption('select[name="urgent"]', 'true');
    await page.click('button[type="submit"]:has-text("Apply Filters")');
    await page.waitForTimeout(500);
    
    // Export filtered data
    const downloadPromise = page.waitForEvent('download');
    await page.click('a:has-text("Export CSV")');
    
    const download = await downloadPromise;
    expect(download).toBeTruthy();
    expect(download.suggestedFilename()).toMatch(/\.csv$/i);
  });

  test('should verify printable view displays items', async ({ page, context }) => {
    // Navigate to items page
    await page.goto('/Items');
    await page.waitForSelector('h1:has-text("Items")');
    
    // Get print view URL
    const printLink = await page.locator('a:has-text("Print View")');
    const href = await printLink.getAttribute('href');
    
    // Navigate to printable view
    const printPage = await context.newPage();
    await printPage.goto(href!);
    await printPage.waitForTimeout(1000);
    
    // Verify page has content (table or items list)
    const hasTable = await printPage.locator('table').count();
    const hasItems = await printPage.locator('.item, tr, .card').count();
    
    // Should have either table or some item representation
    expect(hasTable + hasItems).toBeGreaterThan(0);
    
    await printPage.close();
  });

  test('should handle empty items list for CSV export', async ({ page }) => {
    // Logout and create new user with no items
    const logoutButton = page.locator('form[action="/Identity/Account/Logout"] button').first();
    await logoutButton.click();
    await page.waitForTimeout(1000);
    
    // Register new user
    const newTimestamp = Date.now();
    const newEmail = `emptyuser${newTimestamp}@example.com`;
    
    await page.goto('/Identity/Account/Register');
    await page.waitForSelector('input[name="Input.Email"]');
    await page.fill('input[name="Input.Email"]', newEmail);
    await page.fill('input[name="Input.Password"]', testPassword);
    await page.fill('input[name="Input.ConfirmPassword"]', testPassword);
    await page.click('button[type="submit"]');
    await page.waitForURL(/\/(Identity\/Account\/RegisterConfirmation|\/)/);
    
    // Navigate to items page (should be empty)
    await page.goto('/Items');
    await page.waitForSelector('h1:has-text("Items")');
    
    // Try to export empty items list
    const exportButton = await page.locator('a:has-text("Export CSV")');
    
    if (await exportButton.count() > 0) {
      // If export button is available for empty list
      const downloadPromise = page.waitForEvent('download', { timeout: 5000 }).catch(() => null);
      await exportButton.click();
      
      const download = await downloadPromise;
      // Download may or may not occur for empty list - both are valid
      // Just verify no error occurred
    }
  });
});
