# Playwright E2E Tests for Household Needs Manager

This directory contains comprehensive end-to-end tests for the Household Needs Manager application using Playwright.

## Test Files

### 1. `auth.spec.js` - Authentication Tests
Tests user registration, login, logout functionality:
- ✅ Register new user (with unique email using timestamp)
- ✅ Login with created user credentials
- ✅ Logout functionality
- ✅ Login with invalid credentials (failure test)

### 2. `households.spec.js` - Household Management Tests
Tests household CRUD operations and membership:
- ✅ Create new household
- ✅ Verify household appears in list
- ✅ Switch to created household (set as active)
- ✅ Join another household
- ✅ Leave household (as non-owner)

### 3. `items.spec.js` - Item CRUD Tests
Tests complete item lifecycle:
- ✅ Create personal item
- ✅ Create household item
- ✅ Mark item as urgent
- ✅ Edit item details
- ✅ Delete item
- ✅ Filter items (personal vs household)

### 4. `dashboard.spec.js` - Dashboard Display Tests
Tests dashboard statistics and visualizations:
- ✅ Navigate to dashboard
- ✅ Verify dashboard loads with items
- ✅ Check statistics cards are present
- ✅ Verify charts are rendered (canvas elements)
- ✅ Display active household information
- ✅ Show historical data tables

### 5. `export.spec.js` - CSV Export Tests
Tests data export functionality:
- ✅ Navigate to items page
- ✅ Verify export CSV button exists
- ✅ Click export and verify download
- ✅ Export with filters applied
- ✅ Test printable view loads
- ✅ Verify download response headers
- ✅ Handle empty items list

## Prerequisites

- Node.js (v16 or higher)
- .NET 7.0 SDK
- Playwright browsers installed

## Installation

```bash
cd tests/E2ETests
npm install
npx playwright install
```

## Running Tests

### Run all tests
```bash
npm test
```

### Run tests in headed mode (see browser)
```bash
npm run test:headed
```

### Run tests in debug mode
```bash
npm run test:debug
```

### Run specific test file
```bash
npx playwright test tests/auth.spec.js
```

### Run tests in specific browser
```bash
npx playwright test --project=chromium
npx playwright test --project=firefox
npx playwright test --project=webkit
```

### View test report
```bash
npm run test:report
```

## Configuration

The tests are configured in `playwright.config.js`:

- **Base URL**: `http://localhost:5000` (configurable via `BASE_URL` env variable)
- **Test timeout**: Default Playwright timeout
- **Retries**: 0 locally, 2 in CI
- **Workers**: 1 (sequential execution to avoid database conflicts)
- **Web Server**: Automatically starts the .NET application before tests

## Environment Variables

- `BASE_URL`: Override the default base URL (default: `http://localhost:5000`)
- `CI`: Set to `true` in CI environments for additional retries

## Test Features

### Unique Test Data
All tests generate unique data using timestamps to avoid conflicts:
```javascript
const timestamp = Date.now();
const uniqueEmail = `testuser${timestamp}@example.com`;
```

### Proper Selectors
Tests use best practices for selectors:
1. Name attributes (e.g., `input[name="Input.Email"]`)
2. Text content (e.g., `:has-text("Create Household")`)
3. Role-based selectors where appropriate
4. CSS selectors for specific elements

### Async/Await Pattern
All operations use modern async/await:
```javascript
await page.goto('/Items');
await page.waitForSelector('h1:has-text("Items")');
await page.click('button[type="submit"]');
```

### Assertions
Uses Playwright's built-in expect assertions:
```javascript
await expect(element).toBeVisible();
await expect(element).toHaveText('Expected Text');
expect(value).toBeGreaterThan(0);
```

## Test Data Cleanup

Tests create temporary data (users, households, items) that remains in the database. Consider:
- Using a separate test database
- Implementing cleanup scripts
- Using database transactions (if supported)

## CI/CD Integration

To run in CI/CD pipeline:

```yaml
- name: Install dependencies
  run: |
    cd tests/E2ETests
    npm ci
    npx playwright install --with-deps

- name: Run E2E tests
  run: |
    cd tests/E2ETests
    npm test
  env:
    BASE_URL: http://localhost:5000
    CI: true
```

## Troubleshooting

### Tests fail with "baseURL is not set"
Make sure the application is running on `http://localhost:5000` or set the `BASE_URL` environment variable.

### Tests timeout
Increase timeout in `playwright.config.js` or check if the application server is running.

### Browser not found
Run `npx playwright install` to install browser binaries.

### Database conflicts
Tests run sequentially (workers: 1) to minimize conflicts, but consider using a test database.

## Coverage

Total: **28 test cases** covering:
- Authentication flow (4 tests)
- Household management (5 tests)
- Item CRUD operations (6 tests)
- Dashboard displays (6 tests)
- Export functionality (10 tests)

## Future Improvements

- Add visual regression testing
- Add API testing alongside E2E
- Implement test data factories
- Add performance testing
- Add accessibility testing (a11y)
- Add mobile viewport testing
