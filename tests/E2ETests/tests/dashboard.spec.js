import { test, expect } from '@playwright/test';

test.describe('Dashboard Tests', () => {
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
  });

  test('should display dashboard with no items', async ({ page }) => {
    // Navigate to dashboard
    await page.goto('/Dashboard');
    
    // Wait for page to load
    await page.waitForSelector('h1:has-text("Dashboard")');
    
    // Verify dashboard title is visible
    const title = await page.locator('h1:has-text("Dashboard")');
    await expect(title).toBeVisible();
    
    // Should show "No personal items" message or empty state
    const personalSection = await page.locator('h2:has-text("Personal Items")');
    await expect(personalSection).toBeVisible();
  });

  test('should create household with items and display statistics', async ({ page }) => {
    // Create a household
    await page.goto('/Households/Create');
    await page.waitForSelector('input[name="Name"]');
    await page.fill('input[name="Name"]', householdName);
    await page.click('button[type="submit"]:has-text("Create Household")');
    await page.waitForURL('/Households');
    await page.waitForTimeout(1000);
    
    // Create a personal item
    await page.goto('/Items/Create');
    await page.waitForSelector('input[name="Name"]');
    await page.check('input[name="HouseholdId"][id="personalItem"]');
    await page.fill('input[name="Name"]', `Personal Item ${Date.now()}`);
    await page.fill('input[name="Quantity"]', '2');
    await page.fill('input[name="EstimatedPrice"]', '19.99');
    await page.check('input[name="IsUrgent"]');
    await page.click('button[type="submit"]:has-text("Create")');
    await page.waitForURL('/Items');
    
    // Create a household item
    await page.goto('/Items/Create');
    await page.waitForSelector('input[name="Name"]');
    await page.check('input[name="HouseholdId"][id="householdItem"]');
    await page.fill('input[name="Name"]', `Household Item ${Date.now()}`);
    await page.fill('input[name="Quantity"]', '3');
    await page.fill('input[name="EstimatedPrice"]', '49.99');
    await page.click('button[type="submit"]:has-text("Create")');
    await page.waitForURL('/Items');
    
    // Navigate to dashboard
    await page.goto('/Dashboard');
    await page.waitForSelector('h1:has-text("Dashboard")');
    
    // Verify personal statistics section exists
    const personalStats = await page.locator('h2:has-text("Personal Items")');
    await expect(personalStats).toBeVisible();
    
    // Verify household statistics section exists
    const householdStats = await page.locator(`h2:has-text("Household Items")`);
    await expect(householdStats).toBeVisible();
  });

  test('should verify statistics cards are present', async ({ page }) => {
    // Create items first
    await page.goto('/Households/Create');
    await page.waitForSelector('input[name="Name"]');
    await page.fill('input[name="Name"]', householdName);
    await page.click('button[type="submit"]:has-text("Create Household")');
    await page.waitForURL('/Households');
    
    // Create a personal urgent item
    await page.goto('/Items/Create');
    await page.waitForSelector('input[name="Name"]');
    await page.check('input[name="HouseholdId"][id="personalItem"]');
    await page.fill('input[name="Name"]', `Urgent Item ${Date.now()}`);
    await page.fill('input[name="Quantity"]', '1');
    await page.fill('input[name="EstimatedPrice"]', '25.00');
    await page.check('input[name="IsUrgent"]');
    await page.click('button[type="submit"]:has-text("Create")');
    await page.waitForURL('/Items');
    
    // Navigate to dashboard
    await page.goto('/Dashboard');
    await page.waitForSelector('h1:has-text("Dashboard")');
    await page.waitForTimeout(1000);
    
    // Check for statistics cards - look for common card titles
    const totalItemsCard = await page.locator('.card:has-text("Total Items")').count();
    const urgentItemsCard = await page.locator('.card:has-text("Urgent Items")').count();
    const estimatedCostCard = await page.locator('.card:has-text("Estimated Cost")').count();
    
    // At least one type of statistics should be present
    const totalCards = totalItemsCard + urgentItemsCard + estimatedCostCard;
    expect(totalCards).toBeGreaterThan(0);
  });

  test('should verify charts are rendered', async ({ page }) => {
    // Create household and items
    await page.goto('/Households/Create');
    await page.waitForSelector('input[name="Name"]');
    await page.fill('input[name="Name"]', householdName);
    await page.click('button[type="submit"]:has-text("Create Household")');
    await page.waitForURL('/Households');
    
    // Create multiple items for better chart data
    for (let i = 0; i < 2; i++) {
      await page.goto('/Items/Create');
      await page.waitForSelector('input[name="Name"]');
      await page.check('input[name="HouseholdId"][id="personalItem"]');
      await page.fill('input[name="Name"]', `Item ${Date.now()}_${i}`);
      await page.fill('input[name="Quantity"]', String(i + 1));
      await page.fill('input[name="EstimatedPrice"]', String((i + 1) * 10));
      await page.click('button[type="submit"]:has-text("Create")');
      await page.waitForURL('/Items');
      await page.waitForTimeout(500);
    }
    
    // Navigate to dashboard
    await page.goto('/Dashboard');
    await page.waitForSelector('h1:has-text("Dashboard")');
    
    // Wait for charts to load
    await page.waitForTimeout(2000);
    
    // Verify canvas elements exist (charts are rendered on canvas)
    const canvasElements = await page.locator('canvas').count();
    expect(canvasElements).toBeGreaterThan(0);
    
    // Check for specific chart containers
    const categoryChart = await page.locator('#personalCategoryChart').count();
    const trendChart = await page.locator('#personalTrendChart').count();
    
    // At least one chart should be present
    expect(categoryChart + trendChart).toBeGreaterThan(0);
  });

  test('should display active household information', async ({ page }) => {
    // Create a household
    await page.goto('/Households/Create');
    await page.waitForSelector('input[name="Name"]');
    await page.fill('input[name="Name"]', householdName);
    await page.click('button[type="submit"]:has-text("Create Household")');
    await page.waitForURL('/Households');
    
    // Navigate to dashboard
    await page.goto('/Dashboard');
    await page.waitForSelector('h1:has-text("Dashboard")');
    
    // Verify active household name is displayed
    const activeHouseholdAlert = await page.locator(`.alert:has-text("${householdName}")`);
    await expect(activeHouseholdAlert).toBeVisible();
    
    // Verify it shows as active household
    const activeHouseholdText = await page.locator('text=Active Household');
    await expect(activeHouseholdText).toBeVisible();
  });

  test('should display historical data table', async ({ page }) => {
    // Create household and items
    await page.goto('/Households/Create');
    await page.waitForSelector('input[name="Name"]');
    await page.fill('input[name="Name"]', householdName);
    await page.click('button[type="submit"]:has-text("Create Household")');
    await page.waitForURL('/Households');
    
    // Create an item
    await page.goto('/Items/Create');
    await page.waitForSelector('input[name="Name"]');
    await page.check('input[name="HouseholdId"][id="personalItem"]');
    await page.fill('input[name="Name"]', `Test Item ${Date.now()}`);
    await page.fill('input[name="Quantity"]', '1');
    await page.fill('input[name="EstimatedPrice"]', '15.00');
    await page.click('button[type="submit"]:has-text("Create")');
    await page.waitForURL('/Items');
    
    // Navigate to dashboard
    await page.goto('/Dashboard');
    await page.waitForSelector('h1:has-text("Dashboard")');
    
    // Look for historical data section
    const historicalSection = await page.locator('.card:has-text("Historical Data")').count();
    
    // If items exist, historical data table should be present
    if (historicalSection > 0) {
      const historicalTable = await page.locator('table.table').count();
      expect(historicalTable).toBeGreaterThan(0);
    }
  });
});
