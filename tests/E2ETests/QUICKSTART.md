# Quick Start Guide - Playwright E2E Tests

## Installation (One-time setup)

```bash
cd tests/E2ETests
npm install
npx playwright install chromium
```

## Running Tests

### Quick Run (All Tests)
```bash
cd tests/E2ETests
npm test
```

### Run with Browser Visible (Headed Mode)
```bash
npm run test:headed
```

### Debug Mode (Step through tests)
```bash
npm run test:debug
```

### Run Specific Test File
```bash
npx playwright test tests/auth.spec.js
npx playwright test tests/items.spec.js
npx playwright test tests/dashboard.spec.js
```

### Run Specific Test by Name
```bash
npx playwright test -g "should register a new user"
npx playwright test -g "should create a personal item"
```

### View Test Report
```bash
npm run test:report
```

## Test Files Quick Reference

| File | Tests | Coverage |
|------|-------|----------|
| `auth.spec.js` | 4 | Registration, Login, Logout |
| `households.spec.js` | 5 | Create, Switch, Join, Leave |
| `items.spec.js` | 6 | CRUD, Urgent, Filtering |
| `dashboard.spec.js` | 6 | Statistics, Charts, History |
| `export.spec.js` | 10 | CSV Export, Print View |

## Before Running Tests

1. **Ensure .NET app is NOT running** (Playwright will start it automatically)
   - If running, stop it first: `Ctrl+C` in the terminal

2. **Database**: Tests will create data in the database
   - Consider using a test database
   - Or accept that test data will accumulate

3. **Port 5000**: Make sure it's available
   - Default: `http://localhost:5000`
   - Or set `BASE_URL` environment variable

## Common Commands

```bash
# Run only failed tests
npx playwright test --last-failed

# Run in specific browser
npx playwright test --project=chromium
npx playwright test --project=firefox

# Update snapshots (if any)
npx playwright test --update-snapshots

# Show trace viewer for debugging
npx playwright show-trace trace.zip

# List all tests without running
npx playwright test --list
```

## Environment Variables

```bash
# Custom base URL
BASE_URL=http://localhost:8080 npm test

# CI mode (more retries)
CI=true npm test
```

## Troubleshooting

### "Cannot find module '@playwright/test'"
```bash
npm install
```

### "Executable doesn't exist"
```bash
npx playwright install
```

### "Port already in use"
```bash
# Stop any running instances on port 5000
# Or set different port:
BASE_URL=http://localhost:5001 npm test
```

### Tests timing out
```bash
# Increase timeout in playwright.config.js
# Or run with more time:
npx playwright test --timeout=60000
```

## Test Output Locations

- **HTML Report**: `playwright-report/`
- **Screenshots**: `test-results/` (only on failure)
- **Videos**: `test-results/` (only on failure)
- **Traces**: `test-results/` (on retry)

## Tips

1. **First run** takes longer (downloads browsers)
2. **Sequential execution** prevents database conflicts
3. **Unique data** per test run (timestamp-based)
4. **Watch mode** not enabled by default (tests modify state)
5. **Clean database** periodically for better performance

## Example Test Run

```bash
$ npm test

> household-needs-e2e-tests@1.0.0 test
> playwright test

Running 31 tests using 1 worker

  ✓ auth.spec.js:13:3 › should register a new user (2s)
  ✓ auth.spec.js:36:3 › should login with created user (3s)
  ✓ auth.spec.js:72:3 › should logout successfully (2s)
  ✓ auth.spec.js:94:3 › should fail login with invalid credentials (1s)
  ...
  
  31 passed (2m 15s)

To open last HTML report run:
  npx playwright show-report
```

## Next Steps

After successful test run:
1. View HTML report: `npm run test:report`
2. Check screenshots/videos in `test-results/` if any failed
3. Integrate into CI/CD pipeline
4. Add more test cases as features are added

---

**Need help?** See full documentation in `README.md`
