import { test, expect } from '@playwright/test';

test.describe('Item CRUD Operations Tests', () => {
  let uniqueEmail;
  const testPassword = 'Test123!@#';
  let householdName;
  let personalItemName;
  let householdItemName;

  test.beforeEach(async ({ page }) => {
    // Generate unique test data
    const timestamp = Date.now();
    uniqueEmail = `testuser${timestamp}@example.com`;
    householdName = `Test Household ${timestamp}`;
    personalItemName = `Personal Item ${timestamp}`;
    householdItemName = `Household Item ${timestamp}`;

    // Register and login user
    await page.goto('/Identity/Account/Register');
    await page.waitForSelector('input[name="Input.Email"]');
    await page.fill('input[name="Input.Email"]', uniqueEmail);
    await page.fill('input[name="Input.Password"]', testPassword);
    await page.fill('input[name="Input.ConfirmPassword"]', testPassword);
    await page.click('button[type="submit"]');
    await page.waitForURL(/\/(Identity\/Account\/RegisterConfirmation|\/)/);
    await page.waitForTimeout(1000);

    // Create a household for household items
    await page.goto('/Households/Create');
    await page.waitForSelector('input[name="Name"]');
    await page.fill('input[name="Name"]', householdName);
    await page.click('button[type="submit"]:has-text("Create Household")');
    await page.waitForURL('/Households');
    await page.waitForTimeout(1000);
  });

  test('should create a personal item', async ({ page }) => {
    // Navigate to create item page
    await page.goto('/Items/Create');
    await page.waitForSelector('input[name="Name"]');
    
    // Select personal item radio button
    await page.check('input[name="HouseholdId"][id="personalItem"]');
    
    // Fill in item details
    await page.fill('input[name="Name"]', personalItemName);
    await page.fill('input[name="Quantity"]', '2');
    await page.fill('input[name="Vendor"]', 'Test Vendor');
    await page.fill('textarea[name="Notes"]', 'Test notes for personal item');
    
    // Submit form
    await page.click('button[type="submit"]:has-text("Create")');
    
    // Wait for redirect to items index
    await page.waitForURL('/Items');
    
    // Verify item appears in the list
    const itemRow = await page.locator(`tr:has-text("${personalItemName}")`);
    await expect(itemRow).toBeVisible();
    
    // Verify it's marked as Personal
    const personalBadge = await itemRow.locator('.badge:has-text("Personal")');
    await expect(personalBadge).toBeVisible();
  });

  test('should create a household item', async ({ page }) => {
    // Navigate to create item page
    await page.goto('/Items/Create');
    await page.waitForSelector('input[name="Name"]');
    
    // Select household item radio button
    await page.check('input[name="HouseholdId"][id="householdItem"]');
    
    // Fill in item details
    await page.fill('input[name="Name"]', householdItemName);
    await page.fill('input[name="Quantity"]', '5');
    await page.fill('input[name="EstimatedPrice"]', '29.99');
    await page.fill('input[name="Vendor"]', 'Household Vendor');
    
    // Submit form
    await page.click('button[type="submit"]:has-text("Create")');
    
    // Wait for redirect to items index
    await page.waitForURL('/Items');
    
    // Verify item appears in the list
    const itemRow = await page.locator(`tr:has-text("${householdItemName}")`);
    await expect(itemRow).toBeVisible();
    
    // Verify it's marked as Household
    const householdBadge = await itemRow.locator('.badge:has-text("Household")');
    await expect(householdBadge).toBeVisible();
  });

  test('should mark item as urgent', async ({ page }) => {
    // Navigate to create item page
    await page.goto('/Items/Create');
    await page.waitForSelector('input[name="Name"]');
    
    // Select personal item
    await page.check('input[name="HouseholdId"][id="personalItem"]');
    
    // Fill in item details and mark as urgent
    await page.fill('input[name="Name"]', personalItemName);
    await page.fill('input[name="Quantity"]', '1');
    await page.check('input[name="IsUrgent"]');
    
    // Submit form
    await page.click('button[type="submit"]:has-text("Create")');
    await page.waitForURL('/Items');
    
    // Verify urgent badge is displayed
    const itemRow = await page.locator(`tr:has-text("${personalItemName}")`);
    const urgentBadge = await itemRow.locator('.badge:has-text("Yes")');
    await expect(urgentBadge).toBeVisible();
    
    // Verify urgent icon is displayed
    const urgentIcon = await itemRow.locator('.bi-exclamation-triangle-fill');
    await expect(urgentIcon).toBeVisible();
  });

  test('should edit an item', async ({ page }) => {
    // Create an item first
    await page.goto('/Items/Create');
    await page.waitForSelector('input[name="Name"]');
    await page.check('input[name="HouseholdId"][id="personalItem"]');
    await page.fill('input[name="Name"]', personalItemName);
    await page.fill('input[name="Quantity"]', '1');
    await page.click('button[type="submit"]:has-text("Create")');
    await page.waitForURL('/Items');
    
    // Find the item row and click edit button
    const itemRow = page.locator(`tr:has-text("${personalItemName}")`);
    const editButton = itemRow.locator('a[title="Edit"]');
    await editButton.click();
    
    // Wait for edit page to load
    await page.waitForURL(/\/Items\/Edit\/\d+/);
    await page.waitForSelector('input[name="Name"]');
    
    // Update item details
    const updatedName = `${personalItemName} - Updated`;
    await page.fill('input[name="Name"]', updatedName);
    await page.fill('input[name="Quantity"]', '3');
    await page.check('input[name="IsUrgent"]');
    
    // Submit the edit form
    await page.click('button[type="submit"]');
    
    // Wait for redirect
    await page.waitForURL('/Items');
    
    // Verify updated item appears
    const updatedItemRow = await page.locator(`tr:has-text("${updatedName}")`);
    await expect(updatedItemRow).toBeVisible();
    
    // Verify quantity is updated
    const quantityCell = await updatedItemRow.locator('td:nth-child(2)');
    await expect(quantityCell).toHaveText('3');
  });

  test('should delete an item', async ({ page }) => {
    // Create an item first
    await page.goto('/Items/Create');
    await page.waitForSelector('input[name="Name"]');
    await page.check('input[name="HouseholdId"][id="personalItem"]');
    await page.fill('input[name="Name"]', personalItemName);
    await page.fill('input[name="Quantity"]', '1');
    await page.click('button[type="submit"]:has-text("Create")');
    await page.waitForURL('/Items');
    
    // Find the item row and click delete button
    const itemRow = page.locator(`tr:has-text("${personalItemName}")`);
    const deleteButton = itemRow.locator('a[title="Delete"]');
    await deleteButton.click();
    
    // Wait for delete confirmation page
    await page.waitForURL(/\/Items\/Delete\/\d+/);
    
    // Confirm deletion
    const confirmButton = await page.locator('button[type="submit"]:has-text("Delete")');
    if (await confirmButton.count() > 0) {
      await confirmButton.click();
    } else {
      // Try alternative selector
      await page.click('input[type="submit"][value="Delete"]');
    }
    
    // Wait for redirect
    await page.waitForURL('/Items');
    
    // Verify item is no longer in the list
    const deletedItemRow = page.locator(`tr:has-text("${personalItemName}")`);
    await expect(deletedItemRow).not.toBeVisible();
  });

  test('should filter items correctly - personal vs household', async ({ page }) => {
    // Create both personal and household items
    // Create personal item
    await page.goto('/Items/Create');
    await page.waitForSelector('input[name="Name"]');
    await page.check('input[name="HouseholdId"][id="personalItem"]');
    await page.fill('input[name="Name"]', personalItemName);
    await page.fill('input[name="Quantity"]', '1');
    await page.click('button[type="submit"]:has-text("Create")');
    await page.waitForURL('/Items');
    
    // Create household item
    await page.goto('/Items/Create');
    await page.waitForSelector('input[name="Name"]');
    await page.check('input[name="HouseholdId"][id="householdItem"]');
    await page.fill('input[name="Name"]', householdItemName);
    await page.fill('input[name="Quantity"]', '2');
    await page.click('button[type="submit"]:has-text("Create")');
    await page.waitForURL('/Items');
    
    // Test filtering by personal items
    await page.selectOption('select[name="filter"]', 'personal');
    await page.click('button[type="submit"]:has-text("Apply Filters")');
    await page.waitForTimeout(500);
    
    // Verify only personal item is shown
    const personalItem = await page.locator(`tr:has-text("${personalItemName}")`);
    await expect(personalItem).toBeVisible();
    
    // Household item should not be visible
    const householdItemVisible = await page.locator(`tr:has-text("${householdItemName}")`).count();
    expect(householdItemVisible).toBe(0);
    
    // Test filtering by household items
    await page.selectOption('select[name="filter"]', 'household');
    await page.click('button[type="submit"]:has-text("Apply Filters")');
    await page.waitForTimeout(500);
    
    // Verify only household item is shown
    const householdItem = await page.locator(`tr:has-text("${householdItemName}")`);
    await expect(householdItem).toBeVisible();
    
    // Personal item should not be visible
    const personalItemVisible = await page.locator(`tr:has-text("${personalItemName}")`).count();
    expect(personalItemVisible).toBe(0);
    
    // Clear filters - both items should be visible
    await page.click('a:has-text("Clear Filters")');
    await page.waitForTimeout(500);
    
    const bothPersonalItem = await page.locator(`tr:has-text("${personalItemName}")`);
    const bothHouseholdItem = await page.locator(`tr:has-text("${householdItemName}")`);
    await expect(bothPersonalItem).toBeVisible();
    await expect(bothHouseholdItem).toBeVisible();
  });
});
