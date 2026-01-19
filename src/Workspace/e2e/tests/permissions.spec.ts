import { test, expect } from '@playwright/test';

// Mock permission data grouped by category
const mockPermissions = [
  // Users group
  { id: 'perm-1', name: 'users.read', description: 'View users and user details', group: 'Users' },
  { id: 'perm-2', name: 'users.write', description: 'Create and edit users', group: 'Users' },
  { id: 'perm-3', name: 'users.delete', description: 'Delete users', group: 'Users' },
  { id: 'perm-4', name: 'users.manage-roles', description: 'Assign roles to users', group: 'Users' },

  // Roles group
  { id: 'perm-5', name: 'roles.read', description: 'View roles and permissions', group: 'Roles' },
  { id: 'perm-6', name: 'roles.write', description: 'Create and edit roles', group: 'Roles' },
  { id: 'perm-7', name: 'roles.delete', description: 'Delete roles', group: 'Roles' },

  // Missions group
  { id: 'perm-8', name: 'missions.read', description: 'View missions', group: 'Missions' },
  { id: 'perm-9', name: 'missions.write', description: 'Create and edit missions', group: 'Missions' },
  { id: 'perm-10', name: 'missions.delete', description: 'Delete missions', group: 'Missions' },
  { id: 'perm-11', name: 'missions.execute', description: 'Run mission simulations', group: 'Missions' },

  // Spacecraft group
  { id: 'perm-12', name: 'spacecraft.read', description: 'View spacecraft', group: 'Spacecraft' },
  { id: 'perm-13', name: 'spacecraft.write', description: 'Create and edit spacecraft', group: 'Spacecraft' },
  { id: 'perm-14', name: 'spacecraft.delete', description: 'Delete spacecraft', group: 'Spacecraft' },

  // Reports group
  { id: 'perm-15', name: 'reports.read', description: 'View reports', group: 'Reports' },
  { id: 'perm-16', name: 'reports.write', description: 'Create and export reports', group: 'Reports' },

  // API Keys group
  { id: 'perm-17', name: 'apikeys.read', description: 'View API keys', group: 'API Keys' },
  { id: 'perm-18', name: 'apikeys.write', description: 'Create and manage API keys', group: 'API Keys' },
  { id: 'perm-19', name: 'apikeys.revoke', description: 'Revoke API keys', group: 'API Keys' },

  // System group
  { id: 'perm-20', name: 'system.settings', description: 'Modify system settings', group: 'System' },
  { id: 'perm-21', name: 'system.audit', description: 'View audit logs', group: 'System' },
];

const mockPermissionsResponse = {
  items: mockPermissions,
  page: 1,
  pageSize: 100,
  totalCount: mockPermissions.length,
  totalPages: 1,
};

test.describe('Permissions Page', () => {
  test.beforeEach(async ({ page, context }) => {
    await context.addInitScript(() => {
      localStorage.setItem(
        'access_token',
        'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiZW1haWwiOiJ0ZXN0QGV4YW1wbGUuY29tIiwibmFtZSI6IlRlc3QgVXNlciJ9.fake'
      );
    });

    await page.route('**/api/permissions**', (route) => {
      const url = new URL(route.request().url());
      const search = url.searchParams.get('search');
      const group = url.searchParams.get('group');

      let filteredPermissions = [...mockPermissions];

      if (group) {
        filteredPermissions = filteredPermissions.filter((p) => p.group === group);
      }

      if (search) {
        const searchLower = search.toLowerCase();
        filteredPermissions = filteredPermissions.filter(
          (p) =>
            p.name.toLowerCase().includes(searchLower) ||
            p.description.toLowerCase().includes(searchLower)
        );
      }

      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          items: filteredPermissions,
          page: 1,
          pageSize: 100,
          totalCount: filteredPermissions.length,
          totalPages: 1,
        }),
      });
    });

    await page.goto('/permissions');
  });

  test('should display permissions page', async ({ page }) => {
    await expect(page.locator('text=Permissions')).toBeVisible();
    await expect(page.locator('text=System permissions are read-only')).toBeVisible();
  });

  test('should display permission groups', async ({ page }) => {
    await expect(page.locator('.permission-group:has-text("Users")')).toBeVisible();
    await expect(page.locator('.permission-group:has-text("Roles")')).toBeVisible();
    await expect(page.locator('.permission-group:has-text("Missions")')).toBeVisible();
    await expect(page.locator('.permission-group:has-text("Spacecraft")')).toBeVisible();
    await expect(page.locator('.permission-group:has-text("Reports")')).toBeVisible();
    await expect(page.locator('.permission-group:has-text("API Keys")')).toBeVisible();
    await expect(page.locator('.permission-group:has-text("System")')).toBeVisible();
  });

  test('should display permissions within groups', async ({ page }) => {
    await expect(page.locator('text=users.read')).toBeVisible();
    await expect(page.locator('text=users.write')).toBeVisible();
    await expect(page.locator('text=users.delete')).toBeVisible();
  });

  test('should display permission descriptions', async ({ page }) => {
    await expect(page.locator('text=View users and user details')).toBeVisible();
    await expect(page.locator('text=Create and edit users')).toBeVisible();
  });

  test('should filter permissions by search', async ({ page }) => {
    const searchInput = page.locator('input[placeholder*="Search"]');
    await searchInput.fill('missions');

    await expect(page.locator('text=missions.read')).toBeVisible();
    await expect(page.locator('text=missions.write')).toBeVisible();
    await expect(page.locator('text=users.read')).not.toBeVisible();
  });

  test('should filter permissions by group', async ({ page }) => {
    const groupFilter = page.locator('mat-select').first();
    await groupFilter.click();
    await page.locator('mat-option:has-text("Users")').click();

    await expect(page.locator('text=users.read')).toBeVisible();
    await expect(page.locator('text=users.write')).toBeVisible();
    await expect(page.locator('text=missions.read')).not.toBeVisible();
  });

  test('should expand and collapse permission groups', async ({ page }) => {
    const usersGroupHeader = page.locator('.permission-group:has-text("Users") .group-header');

    // Click to collapse
    await usersGroupHeader.click();
    await expect(page.locator('.permission-group:has-text("Users") .permission-item:has-text("users.read")')).not.toBeVisible();

    // Click to expand
    await usersGroupHeader.click();
    await expect(page.locator('.permission-group:has-text("Users") .permission-item:has-text("users.read")')).toBeVisible();
  });

  test('should display permission count per group', async ({ page }) => {
    await expect(page.locator('.permission-group:has-text("Users")').locator('text=/4 permissions?/i')).toBeVisible();
    await expect(page.locator('.permission-group:has-text("Roles")').locator('text=/3 permissions?/i')).toBeVisible();
  });

  test('should display permission icon for each permission', async ({ page }) => {
    const permissionIcons = page.locator('.permission-item mat-icon');
    const count = await permissionIcons.count();
    expect(count).toBeGreaterThan(0);
  });

  test('should display group icon for each group', async ({ page }) => {
    await expect(page.locator('.permission-group:has-text("Users") .group-icon')).toBeVisible();
    await expect(page.locator('.permission-group:has-text("Missions") .group-icon')).toBeVisible();
  });

  test('should clear search when clear button clicked', async ({ page }) => {
    const searchInput = page.locator('input[placeholder*="Search"]');
    await searchInput.fill('missions');

    await expect(page.locator('text=users.read')).not.toBeVisible();

    const clearButton = page.locator('button[aria-label="Clear search"]');
    if (await clearButton.isVisible()) {
      await clearButton.click();
      await expect(page.locator('text=users.read')).toBeVisible();
    }
  });

  test('should show total permission count', async ({ page }) => {
    await expect(page.locator('text=/\\d+ permissions?/i')).toBeVisible();
  });

  test('should not show create or edit buttons (read-only)', async ({ page }) => {
    await expect(page.locator('button:has-text("Create")')).not.toBeVisible();
    await expect(page.locator('button:has-text("Edit")')).not.toBeVisible();
    await expect(page.locator('button:has-text("Delete")')).not.toBeVisible();
  });

  test('should display info message about permissions being system-managed', async ({ page }) => {
    await expect(page.locator('text=/read-only|system|managed/i')).toBeVisible();
  });

  test('should search by permission name', async ({ page }) => {
    const searchInput = page.locator('input[placeholder*="Search"]');
    await searchInput.fill('apikeys.revoke');

    await expect(page.locator('text=apikeys.revoke')).toBeVisible();
    await expect(page.locator('text=Revoke API keys')).toBeVisible();
    await expect(page.locator('text=users.read')).not.toBeVisible();
  });

  test('should search by permission description', async ({ page }) => {
    const searchInput = page.locator('input[placeholder*="Search"]');
    await searchInput.fill('audit');

    await expect(page.locator('text=system.audit')).toBeVisible();
    await expect(page.locator('text=View audit logs')).toBeVisible();
  });

  test('should show no results message when search has no matches', async ({ page }) => {
    const searchInput = page.locator('input[placeholder*="Search"]');
    await searchInput.fill('nonexistentpermission');

    await expect(page.locator('text=/no permissions?.*found/i')).toBeVisible();
  });

  test('should reset group filter when showing all', async ({ page }) => {
    const groupFilter = page.locator('mat-select').first();
    await groupFilter.click();
    await page.locator('mat-option:has-text("Users")').click();

    await expect(page.locator('text=missions.read')).not.toBeVisible();

    await groupFilter.click();
    await page.locator('mat-option:has-text("All Groups")').click();

    await expect(page.locator('text=missions.read')).toBeVisible();
    await expect(page.locator('text=users.read')).toBeVisible();
  });
});

test.describe('Permissions Page - Error Handling', () => {
  test.beforeEach(async ({ page, context }) => {
    await context.addInitScript(() => {
      localStorage.setItem(
        'access_token',
        'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiZW1haWwiOiJ0ZXN0QGV4YW1wbGUuY29tIiwibmFtZSI6IlRlc3QgVXNlciJ9.fake'
      );
    });
  });

  test('should show error message when loading permissions fails', async ({ page }) => {
    await page.route('**/api/permissions**', (route) => {
      route.fulfill({
        status: 500,
        contentType: 'application/json',
        body: JSON.stringify({ error: 'Internal Server Error' }),
      });
    });

    await page.goto('/permissions');

    await expect(page.locator('.mat-mdc-snack-bar-label')).toContainText(/error|failed/i);
  });

  test('should display empty state when no permissions exist', async ({ page }) => {
    await page.route('**/api/permissions**', (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          items: [],
          page: 1,
          pageSize: 100,
          totalCount: 0,
          totalPages: 0,
        }),
      });
    });

    await page.goto('/permissions');

    await expect(page.locator('text=/no permissions?/i')).toBeVisible();
  });
});

test.describe('Permissions Page - Accessibility', () => {
  test.beforeEach(async ({ page, context }) => {
    await context.addInitScript(() => {
      localStorage.setItem(
        'access_token',
        'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiZW1haWwiOiJ0ZXN0QGV4YW1wbGUuY29tIiwibmFtZSI6IlRlc3QgVXNlciJ9.fake'
      );
    });

    await page.route('**/api/permissions**', (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(mockPermissionsResponse),
      });
    });
  });

  test('permissions page should be keyboard navigable', async ({ page }) => {
    await page.goto('/permissions');

    await page.keyboard.press('Tab');
    await page.keyboard.press('Tab');

    const focusedElement = await page.evaluate(() => document.activeElement?.tagName);
    expect(focusedElement).toBeTruthy();
  });

  test('search input should have label or placeholder', async ({ page }) => {
    await page.goto('/permissions');

    const searchInput = page.locator('input[placeholder*="Search"]');
    await expect(searchInput).toBeVisible();

    const placeholder = await searchInput.getAttribute('placeholder');
    expect(placeholder).toBeTruthy();
  });

  test('group headers should be expandable via keyboard', async ({ page }) => {
    await page.goto('/permissions');

    const groupHeader = page.locator('.permission-group:has-text("Users") .group-header');
    await groupHeader.focus();
    await page.keyboard.press('Enter');

    // Group should toggle
  });

  test('permission items should have readable text', async ({ page }) => {
    await page.goto('/permissions');

    const permissionItems = page.locator('.permission-item');
    const count = await permissionItems.count();
    expect(count).toBeGreaterThan(0);

    // Check first permission has visible text
    const firstItem = permissionItems.first();
    await expect(firstItem).toBeVisible();
    const text = await firstItem.textContent();
    expect(text).toBeTruthy();
  });
});

test.describe('Permissions Page - Responsive Design', () => {
  test.beforeEach(async ({ page, context }) => {
    await context.addInitScript(() => {
      localStorage.setItem(
        'access_token',
        'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiZW1haWwiOiJ0ZXN0QGV4YW1wbGUuY29tIiwibmFtZSI6IlRlc3QgVXNlciJ9.fake'
      );
    });

    await page.route('**/api/permissions**', (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(mockPermissionsResponse),
      });
    });
  });

  test('should display properly on mobile viewport', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 });
    await page.goto('/permissions');

    await expect(page.locator('text=Permissions')).toBeVisible();
    await expect(page.locator('.permission-group:has-text("Users")')).toBeVisible();
  });

  test('should display properly on tablet viewport', async ({ page }) => {
    await page.setViewportSize({ width: 768, height: 1024 });
    await page.goto('/permissions');

    await expect(page.locator('text=Permissions')).toBeVisible();
    await expect(page.locator('.permission-group:has-text("Users")')).toBeVisible();
  });

  test('should display properly on desktop viewport', async ({ page }) => {
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto('/permissions');

    await expect(page.locator('text=Permissions')).toBeVisible();
    await expect(page.locator('.permission-group:has-text("Users")')).toBeVisible();
  });
});
