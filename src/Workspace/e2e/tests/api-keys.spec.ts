import { test, expect } from '@playwright/test';

// Mock API key data
const mockApiKeys = [
  {
    id: '550e8400-e29b-41d4-a716-446655440001',
    name: 'Production API Key',
    description: 'Main production API access key',
    keyPrefix: 'ngmat_prod_abc123',
    scopes: ['missions.read', 'missions.write', 'spacecraft.read'],
    isActive: true,
    expiresAt: '2026-01-15T00:00:00Z',
    lastUsedAt: '2025-01-18T14:30:00Z',
    createdAt: '2024-01-15T10:00:00Z',
    createdBy: 'John Doe',
  },
  {
    id: '550e8400-e29b-41d4-a716-446655440002',
    name: 'Development API Key',
    description: 'Development and testing access',
    keyPrefix: 'ngmat_dev_xyz789',
    scopes: ['missions.read', 'spacecraft.read', 'reports.read'],
    isActive: true,
    expiresAt: '2025-06-30T00:00:00Z',
    lastUsedAt: '2025-01-17T09:15:00Z',
    createdAt: '2024-06-01T08:00:00Z',
    createdBy: 'Jane Smith',
  },
  {
    id: '550e8400-e29b-41d4-a716-446655440003',
    name: 'CI/CD Pipeline Key',
    description: 'Automated deployment access',
    keyPrefix: 'ngmat_ci_def456',
    scopes: ['missions.read', 'missions.write', 'missions.execute'],
    isActive: true,
    expiresAt: null, // No expiration
    lastUsedAt: '2025-01-19T02:00:00Z',
    createdAt: '2024-09-15T12:00:00Z',
    createdBy: 'John Doe',
  },
  {
    id: '550e8400-e29b-41d4-a716-446655440004',
    name: 'Revoked API Key',
    description: 'Previously active key that was revoked',
    keyPrefix: 'ngmat_old_ghi012',
    scopes: ['missions.read'],
    isActive: false,
    expiresAt: '2025-12-31T00:00:00Z',
    lastUsedAt: '2024-10-15T16:45:00Z',
    createdAt: '2024-03-01T10:00:00Z',
    createdBy: 'Bob Wilson',
    revokedAt: '2024-11-01T12:00:00Z',
    revokedBy: 'Admin',
  },
];

const mockScopes = [
  { id: 'scope-1', name: 'missions.read', description: 'Read access to missions' },
  { id: 'scope-2', name: 'missions.write', description: 'Write access to missions' },
  { id: 'scope-3', name: 'missions.execute', description: 'Execute mission simulations' },
  { id: 'scope-4', name: 'spacecraft.read', description: 'Read access to spacecraft' },
  { id: 'scope-5', name: 'spacecraft.write', description: 'Write access to spacecraft' },
  { id: 'scope-6', name: 'reports.read', description: 'Read access to reports' },
  { id: 'scope-7', name: 'reports.write', description: 'Write access to reports' },
  { id: 'scope-8', name: 'users.read', description: 'Read access to users' },
];

const mockApiKeysResponse = {
  items: mockApiKeys,
  page: 1,
  pageSize: 20,
  totalCount: 4,
  totalPages: 1,
};

test.describe('API Keys List Page', () => {
  test.beforeEach(async ({ page, context }) => {
    await context.addInitScript(() => {
      localStorage.setItem(
        'access_token',
        'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiZW1haWwiOiJ0ZXN0QGV4YW1wbGUuY29tIiwibmFtZSI6IlRlc3QgVXNlciJ9.fake'
      );
    });

    await page.route('**/api/apikeys**', (route) => {
      const url = new URL(route.request().url());
      const status = url.searchParams.get('status');
      const search = url.searchParams.get('search');

      let filteredKeys = [...mockApiKeys];

      if (status === 'active') {
        filteredKeys = filteredKeys.filter((k) => k.isActive);
      } else if (status === 'revoked') {
        filteredKeys = filteredKeys.filter((k) => !k.isActive);
      }

      if (search) {
        const searchLower = search.toLowerCase();
        filteredKeys = filteredKeys.filter(
          (k) =>
            k.name.toLowerCase().includes(searchLower) ||
            k.description.toLowerCase().includes(searchLower) ||
            k.keyPrefix.toLowerCase().includes(searchLower)
        );
      }

      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          items: filteredKeys,
          page: 1,
          pageSize: 20,
          totalCount: filteredKeys.length,
          totalPages: 1,
        }),
      });
    });

    await page.goto('/api-keys');
  });

  test('should display API keys page', async ({ page }) => {
    await expect(page.locator('text=API Key Management')).toBeVisible();
    await expect(page.locator('button:has-text("Create API Key")')).toBeVisible();
  });

  test('should display API keys in the table', async ({ page }) => {
    await expect(page.locator('text=Production API Key')).toBeVisible();
    await expect(page.locator('text=Development API Key')).toBeVisible();
    await expect(page.locator('text=CI/CD Pipeline Key')).toBeVisible();
    await expect(page.locator('text=Revoked API Key')).toBeVisible();
  });

  test('should display key prefix', async ({ page }) => {
    await expect(page.locator('text=ngmat_prod_abc123')).toBeVisible();
    await expect(page.locator('text=ngmat_dev_xyz789')).toBeVisible();
  });

  test('should display active status for active keys', async ({ page }) => {
    await expect(page.locator('tr:has-text("Production API Key") .status-chip--active')).toBeVisible();
    await expect(page.locator('tr:has-text("Development API Key") .status-chip--active')).toBeVisible();
  });

  test('should display revoked status for revoked keys', async ({ page }) => {
    await expect(page.locator('tr:has-text("Revoked API Key") .status-chip--revoked')).toBeVisible();
  });

  test('should display expiration date', async ({ page }) => {
    await expect(page.locator('tr:has-text("Production API Key")').locator('text=/2026/i')).toBeVisible();
  });

  test('should display "Never" for keys without expiration', async ({ page }) => {
    await expect(page.locator('tr:has-text("CI/CD Pipeline Key")').locator('text=Never')).toBeVisible();
  });

  test('should display last used date', async ({ page }) => {
    await expect(page.locator('tr:has-text("Production API Key")').locator('text=/Jan/i')).toBeVisible();
  });

  test('should filter keys by status - active', async ({ page }) => {
    const statusFilter = page.locator('mat-select').first();
    await statusFilter.click();
    await page.locator('mat-option:has-text("Active")').click();

    await expect(page.locator('text=Production API Key')).toBeVisible();
    await expect(page.locator('text=Development API Key')).toBeVisible();
    await expect(page.locator('text=Revoked API Key')).not.toBeVisible();
  });

  test('should filter keys by status - revoked', async ({ page }) => {
    const statusFilter = page.locator('mat-select').first();
    await statusFilter.click();
    await page.locator('mat-option:has-text("Revoked")').click();

    await expect(page.locator('text=Revoked API Key')).toBeVisible();
    await expect(page.locator('text=Production API Key')).not.toBeVisible();
  });

  test('should search keys by name', async ({ page }) => {
    const searchInput = page.locator('input[placeholder*="Search"]');
    await searchInput.fill('Production');

    await expect(page.locator('text=Production API Key')).toBeVisible();
    await expect(page.locator('text=Development API Key')).not.toBeVisible();
  });

  test('should search keys by prefix', async ({ page }) => {
    const searchInput = page.locator('input[placeholder*="Search"]');
    await searchInput.fill('ngmat_ci');

    await expect(page.locator('text=CI/CD Pipeline Key')).toBeVisible();
    await expect(page.locator('text=Production API Key')).not.toBeVisible();
  });

  test('should display scopes for each key', async ({ page }) => {
    await expect(page.locator('tr:has-text("Production API Key")').locator('text=/missions\\.read/i')).toBeVisible();
  });

  test('should display scope count when many scopes', async ({ page }) => {
    await expect(page.locator('tr:has-text("Production API Key")').locator('text=/\\+\\d+ more|3 scopes?/i')).toBeVisible();
  });

  test('should open create API key dialog', async ({ page }) => {
    await page.locator('button:has-text("Create API Key")').click();
    await expect(page.locator('text=Create New API Key')).toBeVisible();
  });

  test('should show action menu for active key', async ({ page }) => {
    const actionButton = page.locator('tr:has-text("Production API Key") button[mat-icon-button]');
    await actionButton.click();

    await expect(page.locator('button:has-text("Copy Key Prefix")')).toBeVisible();
    await expect(page.locator('button:has-text("Revoke")')).toBeVisible();
  });

  test('should not show revoke option for already revoked key', async ({ page }) => {
    const actionButton = page.locator('tr:has-text("Revoked API Key") button[mat-icon-button]');
    await actionButton.click();

    await expect(page.locator('button:has-text("Revoke")')).not.toBeVisible();
  });

  test('should revoke active key', async ({ page }) => {
    page.on('dialog', (dialog) => dialog.accept());

    await page.route('**/api/apikeys/550e8400-e29b-41d4-a716-446655440001/revoke', (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          ...mockApiKeys[0],
          isActive: false,
          revokedAt: new Date().toISOString(),
          revokedBy: 'Test User',
        }),
      });
    });

    const actionButton = page.locator('tr:has-text("Production API Key") button[mat-icon-button]');
    await actionButton.click();
    await page.locator('button:has-text("Revoke")').click();

    await expect(page.locator('.mat-mdc-snack-bar-label')).toContainText(/revoked|success/i);
  });

  test('should delete revoked key', async ({ page }) => {
    page.on('dialog', (dialog) => dialog.accept());

    await page.route('**/api/apikeys/550e8400-e29b-41d4-a716-446655440004', (route) => {
      if (route.request().method() === 'DELETE') {
        route.fulfill({ status: 204 });
      }
    });

    const actionButton = page.locator('tr:has-text("Revoked API Key") button[mat-icon-button]');
    await actionButton.click();
    await page.locator('button:has-text("Delete")').click();

    await expect(page.locator('.mat-mdc-snack-bar-label')).toContainText(/deleted|success/i);
  });

  test('should display warning for keys expiring soon', async ({ page }) => {
    const expiringKey = {
      ...mockApiKeys[1],
      expiresAt: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString(), // 7 days from now
    };

    await page.route('**/api/apikeys**', (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          items: [expiringKey, ...mockApiKeys.slice(2)],
          page: 1,
          pageSize: 20,
          totalCount: 3,
          totalPages: 1,
        }),
      });
    });

    await page.reload();

    await expect(page.locator('tr:has-text("Development API Key") .expiring-soon')).toBeVisible();
  });
});

test.describe('API Keys - Create New Key', () => {
  test.beforeEach(async ({ page, context }) => {
    await context.addInitScript(() => {
      localStorage.setItem(
        'access_token',
        'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiZW1haWwiOiJ0ZXN0QGV4YW1wbGUuY29tIiwibmFtZSI6IlRlc3QgVXNlciJ9.fake'
      );
    });

    await page.route('**/api/apikeys**', (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(mockApiKeysResponse),
      });
    });

    await page.route('**/api/apikeys/scopes', (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ items: mockScopes }),
      });
    });

    await page.goto('/api-keys');
    await page.locator('button:has-text("Create API Key")').click();
  });

  test('should display create API key dialog', async ({ page }) => {
    await expect(page.locator('text=Create New API Key')).toBeVisible();
    await expect(page.locator('input[formControlName="name"]')).toBeVisible();
    await expect(page.locator('textarea[formControlName="description"]')).toBeVisible();
  });

  test('should display scope selection', async ({ page }) => {
    await expect(page.locator('text=Select Scopes')).toBeVisible();
    await expect(page.locator('mat-checkbox:has-text("missions.read")')).toBeVisible();
    await expect(page.locator('mat-checkbox:has-text("spacecraft.read")')).toBeVisible();
  });

  test('should display expiration options', async ({ page }) => {
    await expect(page.locator('text=Expiration')).toBeVisible();
    await expect(page.locator('mat-radio-button:has-text("30 days")')).toBeVisible();
    await expect(page.locator('mat-radio-button:has-text("90 days")')).toBeVisible();
    await expect(page.locator('mat-radio-button:has-text("1 year")')).toBeVisible();
    await expect(page.locator('mat-radio-button:has-text("Never")')).toBeVisible();
  });

  test('should show validation error for required name', async ({ page }) => {
    const nameInput = page.locator('input[formControlName="name"]');
    await nameInput.click();
    await nameInput.blur();

    await expect(page.locator('mat-error:has-text("required")')).toBeVisible();
  });

  test('should require at least one scope', async ({ page }) => {
    await page.locator('input[formControlName="name"]').fill('Test API Key');
    await page.locator('button:has-text("Create")').click();

    await expect(page.locator('text=/select.*scope|at least one/i')).toBeVisible();
  });

  test('should create new API key successfully', async ({ page }) => {
    const newApiKey = {
      id: '550e8400-e29b-41d4-a716-446655440005',
      name: 'New Test API Key',
      description: 'A new test API key',
      keyPrefix: 'ngmat_test_new123',
      key: 'ngmat_test_new123_full_key_here_abc123xyz789', // Full key shown once
      scopes: ['missions.read', 'spacecraft.read'],
      isActive: true,
      expiresAt: new Date(Date.now() + 90 * 24 * 60 * 60 * 1000).toISOString(),
      createdAt: new Date().toISOString(),
      createdBy: 'Test User',
    };

    await page.route('**/api/apikeys', (route) => {
      if (route.request().method() === 'POST') {
        route.fulfill({
          status: 201,
          contentType: 'application/json',
          body: JSON.stringify(newApiKey),
        });
      }
    });

    await page.locator('input[formControlName="name"]').fill('New Test API Key');
    await page.locator('textarea[formControlName="description"]').fill('A new test API key');
    await page.locator('mat-checkbox:has-text("missions.read")').click();
    await page.locator('mat-checkbox:has-text("spacecraft.read")').click();
    await page.locator('mat-radio-button:has-text("90 days")').click();

    await page.locator('button:has-text("Create")').click();

    await expect(page.locator('.mat-mdc-snack-bar-label')).toContainText(/created|success/i);
  });

  test('should display full API key after creation for copying', async ({ page }) => {
    const newApiKey = {
      id: '550e8400-e29b-41d4-a716-446655440005',
      name: 'New Test API Key',
      keyPrefix: 'ngmat_test_new123',
      key: 'ngmat_test_new123_full_key_here_abc123xyz789',
      scopes: ['missions.read'],
      isActive: true,
      createdAt: new Date().toISOString(),
    };

    await page.route('**/api/apikeys', (route) => {
      if (route.request().method() === 'POST') {
        route.fulfill({
          status: 201,
          contentType: 'application/json',
          body: JSON.stringify(newApiKey),
        });
      }
    });

    await page.locator('input[formControlName="name"]').fill('New Test API Key');
    await page.locator('mat-checkbox:has-text("missions.read")').click();
    await page.locator('button:has-text("Create")').click();

    await expect(page.locator('text=ngmat_test_new123_full_key_here_abc123xyz789')).toBeVisible();
    await expect(page.locator('text=/copy.*key|save.*key/i')).toBeVisible();
  });

  test('should close dialog on cancel', async ({ page }) => {
    await page.locator('button:has-text("Cancel")').click();
    await expect(page.locator('text=Create New API Key')).not.toBeVisible();
  });

  test('should select custom expiration date', async ({ page }) => {
    await page.locator('mat-radio-button:has-text("Custom")').click();
    await expect(page.locator('input[type="date"]')).toBeVisible();
  });
});

test.describe('API Keys - Error Handling', () => {
  test.beforeEach(async ({ page, context }) => {
    await context.addInitScript(() => {
      localStorage.setItem(
        'access_token',
        'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiZW1haWwiOiJ0ZXN0QGV4YW1wbGUuY29tIiwibmFtZSI6IlRlc3QgVXNlciJ9.fake'
      );
    });
  });

  test('should show error message when loading API keys fails', async ({ page }) => {
    await page.route('**/api/apikeys**', (route) => {
      route.fulfill({
        status: 500,
        contentType: 'application/json',
        body: JSON.stringify({ error: 'Internal Server Error' }),
      });
    });

    await page.goto('/api-keys');

    await expect(page.locator('.mat-mdc-snack-bar-label')).toContainText(/error|failed/i);
  });

  test('should show error message when creating API key fails', async ({ page }) => {
    await page.route('**/api/apikeys**', (route) => {
      if (route.request().method() === 'GET') {
        route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(mockApiKeysResponse),
        });
      } else if (route.request().method() === 'POST') {
        route.fulfill({
          status: 400,
          contentType: 'application/json',
          body: JSON.stringify({ error: 'API key name already exists' }),
        });
      }
    });

    await page.route('**/api/apikeys/scopes', (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ items: mockScopes }),
      });
    });

    await page.goto('/api-keys');
    await page.locator('button:has-text("Create API Key")').click();

    await page.locator('input[formControlName="name"]').fill('Production API Key');
    await page.locator('mat-checkbox:has-text("missions.read")').click();
    await page.locator('button:has-text("Create")').click();

    await expect(page.locator('.mat-mdc-snack-bar-label')).toContainText(/error|failed|exists/i);
  });

  test('should show error message when revoking API key fails', async ({ page }) => {
    page.on('dialog', (dialog) => dialog.accept());

    await page.route('**/api/apikeys**', (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(mockApiKeysResponse),
      });
    });

    await page.route('**/api/apikeys/550e8400-e29b-41d4-a716-446655440001/revoke', (route) => {
      route.fulfill({
        status: 500,
        contentType: 'application/json',
        body: JSON.stringify({ error: 'Failed to revoke API key' }),
      });
    });

    await page.goto('/api-keys');

    const actionButton = page.locator('tr:has-text("Production API Key") button[mat-icon-button]');
    await actionButton.click();
    await page.locator('button:has-text("Revoke")').click();

    await expect(page.locator('.mat-mdc-snack-bar-label')).toContainText(/error|failed/i);
  });
});

test.describe('API Keys - Accessibility', () => {
  test.beforeEach(async ({ page, context }) => {
    await context.addInitScript(() => {
      localStorage.setItem(
        'access_token',
        'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiZW1haWwiOiJ0ZXN0QGV4YW1wbGUuY29tIiwibmFtZSI6IlRlc3QgVXNlciJ9.fake'
      );
    });

    await page.route('**/api/apikeys**', (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(mockApiKeysResponse),
      });
    });

    await page.route('**/api/apikeys/scopes', (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ items: mockScopes }),
      });
    });
  });

  test('API keys page should be keyboard navigable', async ({ page }) => {
    await page.goto('/api-keys');

    await page.keyboard.press('Tab');
    await page.keyboard.press('Tab');

    const focusedElement = await page.evaluate(() => document.activeElement?.tagName);
    expect(focusedElement).toBeTruthy();
  });

  test('create dialog should have form labels', async ({ page }) => {
    await page.goto('/api-keys');
    await page.locator('button:has-text("Create API Key")').click();

    await expect(page.locator('mat-label:has-text("Name")')).toBeVisible();
    await expect(page.locator('mat-label:has-text("Description")')).toBeVisible();
  });

  test('action buttons should have accessible labels', async ({ page }) => {
    await page.goto('/api-keys');

    const actionButtons = page.locator('button[mat-icon-button][aria-label]');
    const count = await actionButtons.count();
    expect(count).toBeGreaterThan(0);
  });
});

test.describe('API Keys - Security', () => {
  test.beforeEach(async ({ page, context }) => {
    await context.addInitScript(() => {
      localStorage.setItem(
        'access_token',
        'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiZW1haWwiOiJ0ZXN0QGV4YW1wbGUuY29tIiwibmFtZSI6IlRlc3QgVXNlciJ9.fake'
      );
    });

    await page.route('**/api/apikeys**', (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(mockApiKeysResponse),
      });
    });
  });

  test('should only show key prefix, not full key', async ({ page }) => {
    await page.goto('/api-keys');

    // Key prefix should be visible
    await expect(page.locator('text=ngmat_prod_abc123')).toBeVisible();

    // Full key should not be displayed in the list
    const pageContent = await page.content();
    expect(pageContent).not.toContain('full_key_here');
  });

  test('should require confirmation before revoking key', async ({ page }) => {
    let dialogShown = false;
    page.on('dialog', (dialog) => {
      dialogShown = true;
      dialog.dismiss();
    });

    await page.goto('/api-keys');

    const actionButton = page.locator('tr:has-text("Production API Key") button[mat-icon-button]');
    await actionButton.click();
    await page.locator('button:has-text("Revoke")').click();

    expect(dialogShown).toBe(true);
  });

  test('should show warning when creating key without expiration', async ({ page }) => {
    await page.route('**/api/apikeys/scopes', (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ items: mockScopes }),
      });
    });

    await page.goto('/api-keys');
    await page.locator('button:has-text("Create API Key")').click();

    await page.locator('mat-radio-button:has-text("Never")').click();

    await expect(page.locator('text=/warning|not recommended|security/i')).toBeVisible();
  });
});
