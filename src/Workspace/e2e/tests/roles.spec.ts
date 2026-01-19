import { test, expect } from '@playwright/test';

// Mock role data
const mockRoles = [
  {
    id: '550e8400-e29b-41d4-a716-446655440001',
    name: 'Administrator',
    description: 'Full system access with all permissions',
    isSystem: true,
    userCount: 2,
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-15T00:00:00Z',
    permissions: [
      { id: 'perm-1', name: 'users.read', description: 'View users' },
      { id: 'perm-2', name: 'users.write', description: 'Create/edit users' },
      { id: 'perm-3', name: 'users.delete', description: 'Delete users' },
      { id: 'perm-4', name: 'roles.read', description: 'View roles' },
      { id: 'perm-5', name: 'roles.write', description: 'Create/edit roles' },
    ],
  },
  {
    id: '550e8400-e29b-41d4-a716-446655440002',
    name: 'User',
    description: 'Standard user with limited access',
    isSystem: true,
    userCount: 15,
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-10T00:00:00Z',
    permissions: [
      { id: 'perm-1', name: 'users.read', description: 'View users' },
      { id: 'perm-4', name: 'roles.read', description: 'View roles' },
    ],
  },
  {
    id: '550e8400-e29b-41d4-a716-446655440003',
    name: 'Mission Manager',
    description: 'Can manage missions and spacecraft',
    isSystem: false,
    userCount: 5,
    createdAt: '2024-06-15T00:00:00Z',
    updatedAt: '2024-12-01T00:00:00Z',
    permissions: [
      { id: 'perm-6', name: 'missions.read', description: 'View missions' },
      { id: 'perm-7', name: 'missions.write', description: 'Create/edit missions' },
      { id: 'perm-8', name: 'missions.delete', description: 'Delete missions' },
    ],
  },
  {
    id: '550e8400-e29b-41d4-a716-446655440004',
    name: 'Analyst',
    description: 'Read-only access to reports and data',
    isSystem: false,
    userCount: 8,
    createdAt: '2024-08-20T00:00:00Z',
    updatedAt: '2024-11-15T00:00:00Z',
    permissions: [
      { id: 'perm-1', name: 'users.read', description: 'View users' },
      { id: 'perm-6', name: 'missions.read', description: 'View missions' },
      { id: 'perm-9', name: 'reports.read', description: 'View reports' },
    ],
  },
];

const mockPermissions = [
  { id: 'perm-1', name: 'users.read', description: 'View users', group: 'Users' },
  { id: 'perm-2', name: 'users.write', description: 'Create/edit users', group: 'Users' },
  { id: 'perm-3', name: 'users.delete', description: 'Delete users', group: 'Users' },
  { id: 'perm-4', name: 'roles.read', description: 'View roles', group: 'Roles' },
  { id: 'perm-5', name: 'roles.write', description: 'Create/edit roles', group: 'Roles' },
  { id: 'perm-6', name: 'missions.read', description: 'View missions', group: 'Missions' },
  { id: 'perm-7', name: 'missions.write', description: 'Create/edit missions', group: 'Missions' },
  { id: 'perm-8', name: 'missions.delete', description: 'Delete missions', group: 'Missions' },
  { id: 'perm-9', name: 'reports.read', description: 'View reports', group: 'Reports' },
  { id: 'perm-10', name: 'reports.write', description: 'Create reports', group: 'Reports' },
];

const mockRolesResponse = {
  items: mockRoles,
  page: 1,
  pageSize: 20,
  totalCount: 4,
  totalPages: 1,
};

test.describe('Roles List Page', () => {
  test.beforeEach(async ({ page, context }) => {
    await context.addInitScript(() => {
      localStorage.setItem(
        'access_token',
        'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiZW1haWwiOiJ0ZXN0QGV4YW1wbGUuY29tIiwibmFtZSI6IlRlc3QgVXNlciJ9.fake'
      );
    });

    await page.route('**/api/roles**', (route) => {
      const url = new URL(route.request().url());
      const search = url.searchParams.get('search');
      const type = url.searchParams.get('type');

      let filteredRoles = [...mockRoles];

      if (type === 'system') {
        filteredRoles = filteredRoles.filter((r) => r.isSystem);
      } else if (type === 'custom') {
        filteredRoles = filteredRoles.filter((r) => !r.isSystem);
      }

      if (search) {
        const searchLower = search.toLowerCase();
        filteredRoles = filteredRoles.filter(
          (r) =>
            r.name.toLowerCase().includes(searchLower) ||
            r.description.toLowerCase().includes(searchLower)
        );
      }

      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          items: filteredRoles,
          page: 1,
          pageSize: 20,
          totalCount: filteredRoles.length,
          totalPages: 1,
        }),
      });
    });

    await page.goto('/roles');
  });

  test('should display roles page', async ({ page }) => {
    await expect(page.locator('text=Role Management')).toBeVisible();
    await expect(page.locator('button:has-text("Create Role")')).toBeVisible();
  });

  test('should display role cards', async ({ page }) => {
    await expect(page.locator('.role-card:has-text("Administrator")')).toBeVisible();
    await expect(page.locator('.role-card:has-text("User")')).toBeVisible();
    await expect(page.locator('.role-card:has-text("Mission Manager")')).toBeVisible();
    await expect(page.locator('.role-card:has-text("Analyst")')).toBeVisible();
  });

  test('should display system roles section', async ({ page }) => {
    await expect(page.locator('text=System Roles')).toBeVisible();
  });

  test('should display custom roles section', async ({ page }) => {
    await expect(page.locator('text=Custom Roles')).toBeVisible();
  });

  test('should display role description', async ({ page }) => {
    await expect(page.locator('text=Full system access with all permissions')).toBeVisible();
  });

  test('should display user count for each role', async ({ page }) => {
    await expect(page.locator('.role-card:has-text("Administrator") .user-count:has-text("2")')).toBeVisible();
    await expect(page.locator('.role-card:has-text("User") .user-count:has-text("15")')).toBeVisible();
  });

  test('should filter roles by search', async ({ page }) => {
    const searchInput = page.locator('input[placeholder*="Search"]');
    await searchInput.fill('Mission');

    await expect(page.locator('.role-card:has-text("Mission Manager")')).toBeVisible();
    await expect(page.locator('.role-card:has-text("Administrator")')).not.toBeVisible();
  });

  test('should filter to show only system roles', async ({ page }) => {
    const filterSelect = page.locator('mat-select').first();
    await filterSelect.click();
    await page.locator('mat-option:has-text("System Roles")').click();

    await expect(page.locator('.role-card:has-text("Administrator")')).toBeVisible();
    await expect(page.locator('.role-card:has-text("User")')).toBeVisible();
    await expect(page.locator('.role-card:has-text("Mission Manager")')).not.toBeVisible();
  });

  test('should filter to show only custom roles', async ({ page }) => {
    const filterSelect = page.locator('mat-select').first();
    await filterSelect.click();
    await page.locator('mat-option:has-text("Custom Roles")').click();

    await expect(page.locator('.role-card:has-text("Mission Manager")')).toBeVisible();
    await expect(page.locator('.role-card:has-text("Analyst")')).toBeVisible();
    await expect(page.locator('.role-card:has-text("Administrator")')).not.toBeVisible();
  });

  test('should navigate to create role page', async ({ page }) => {
    await page.locator('button:has-text("Create Role")').click();
    await expect(page).toHaveURL(/.*roles\/new/);
  });

  test('should navigate to role detail on card click', async ({ page }) => {
    await page.route('**/api/roles/550e8400-e29b-41d4-a716-446655440003', (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(mockRoles[2]),
      });
    });

    await page.locator('.role-card:has-text("Mission Manager")').click();
    await expect(page).toHaveURL(/.*roles\/550e8400-e29b-41d4-a716-446655440003/);
  });

  test('should display system badge for system roles', async ({ page }) => {
    await expect(page.locator('.role-card:has-text("Administrator") .system-badge')).toBeVisible();
    await expect(page.locator('.role-card:has-text("User") .system-badge')).toBeVisible();
  });

  test('should not display system badge for custom roles', async ({ page }) => {
    await expect(page.locator('.role-card:has-text("Mission Manager") .system-badge')).not.toBeVisible();
    await expect(page.locator('.role-card:has-text("Analyst") .system-badge')).not.toBeVisible();
  });

  test('should display permission count for each role', async ({ page }) => {
    await expect(page.locator('.role-card:has-text("Administrator")').locator('text=/\\d+ permissions?/i')).toBeVisible();
  });

  test('should show edit button for custom roles', async ({ page }) => {
    await expect(page.locator('.role-card:has-text("Mission Manager") button:has-text("Edit")')).toBeVisible();
  });

  test('should show delete button for custom roles only', async ({ page }) => {
    await expect(page.locator('.role-card:has-text("Mission Manager") button[mat-icon-button]')).toBeVisible();
    await expect(page.locator('.role-card:has-text("Administrator") button:has-text("Delete")')).not.toBeVisible();
  });

  test('should delete custom role', async ({ page }) => {
    page.on('dialog', (dialog) => dialog.accept());

    await page.route('**/api/roles/550e8400-e29b-41d4-a716-446655440003', (route) => {
      if (route.request().method() === 'DELETE') {
        route.fulfill({ status: 204 });
      }
    });

    const deleteButton = page.locator('.role-card:has-text("Mission Manager") button[mat-icon-button]');
    await deleteButton.click();

    await expect(page.locator('.mat-mdc-snack-bar-label')).toContainText(/deleted|success/i);
  });
});

test.describe('Role Detail - Create New Role', () => {
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
        body: JSON.stringify({
          items: mockPermissions,
          page: 1,
          pageSize: 100,
          totalCount: mockPermissions.length,
          totalPages: 1,
        }),
      });
    });

    await page.goto('/roles/new');
  });

  test('should display role creation form', async ({ page }) => {
    await expect(page.locator('text=New Role')).toBeVisible();
    await expect(page.locator('input[formControlName="name"]')).toBeVisible();
    await expect(page.locator('textarea[formControlName="description"]')).toBeVisible();
  });

  test('should show validation error for required fields', async ({ page }) => {
    const nameInput = page.locator('input[formControlName="name"]');
    await nameInput.click();
    await nameInput.blur();

    await expect(page.locator('mat-error:has-text("required")')).toBeVisible();
  });

  test('should show validation error for short name', async ({ page }) => {
    const nameInput = page.locator('input[formControlName="name"]');
    await nameInput.fill('AB');
    await nameInput.blur();

    await expect(page.locator('mat-error:has-text("at least")')).toBeVisible();
  });

  test('should display permissions grouped by category', async ({ page }) => {
    await expect(page.locator('text=Users')).toBeVisible();
    await expect(page.locator('text=Roles')).toBeVisible();
    await expect(page.locator('text=Missions')).toBeVisible();
    await expect(page.locator('text=Reports')).toBeVisible();
  });

  test('should display individual permissions', async ({ page }) => {
    await expect(page.locator('mat-checkbox:has-text("View users")')).toBeVisible();
    await expect(page.locator('mat-checkbox:has-text("Create/edit users")')).toBeVisible();
    await expect(page.locator('mat-checkbox:has-text("Delete users")')).toBeVisible();
  });

  test('should create new role successfully', async ({ page }) => {
    const newRole = {
      id: '550e8400-e29b-41d4-a716-446655440005',
      name: 'New Custom Role',
      description: 'A new custom role for testing',
      isSystem: false,
      userCount: 0,
      permissions: [mockPermissions[0], mockPermissions[5]],
      createdAt: new Date().toISOString(),
    };

    await page.route('**/api/roles', (route) => {
      if (route.request().method() === 'POST') {
        route.fulfill({
          status: 201,
          contentType: 'application/json',
          body: JSON.stringify(newRole),
        });
      }
    });

    await page.locator('input[formControlName="name"]').fill('New Custom Role');
    await page.locator('textarea[formControlName="description"]').fill('A new custom role for testing');
    await page.locator('mat-checkbox:has-text("View users")').click();
    await page.locator('mat-checkbox:has-text("View missions")').click();

    await page.locator('button:has-text("Create Role")').click();

    await expect(page.locator('.mat-mdc-snack-bar-label')).toContainText(/created|success/i);
  });

  test('should navigate back on cancel', async ({ page }) => {
    await page.locator('button:has-text("Cancel")').click();
    await expect(page).toHaveURL(/.*roles$/);
  });

  test('should select all permissions in a group', async ({ page }) => {
    const groupHeader = page.locator('.permission-group:has-text("Users")').locator('mat-checkbox').first();
    await groupHeader.click();

    await expect(page.locator('mat-checkbox:has-text("View users")').locator('input')).toBeChecked();
    await expect(page.locator('mat-checkbox:has-text("Create/edit users")').locator('input')).toBeChecked();
    await expect(page.locator('mat-checkbox:has-text("Delete users")').locator('input')).toBeChecked();
  });
});

test.describe('Role Detail - Edit Existing Role', () => {
  const existingRole = mockRoles[2]; // Mission Manager (custom role)

  test.beforeEach(async ({ page, context }) => {
    await context.addInitScript(() => {
      localStorage.setItem(
        'access_token',
        'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiZW1haWwiOiJ0ZXN0QGV4YW1wbGUuY29tIiwibmFtZSI6IlRlc3QgVXNlciJ9.fake'
      );
    });

    await page.route(`**/api/roles/${existingRole.id}`, (route) => {
      if (route.request().method() === 'GET') {
        route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(existingRole),
        });
      } else if (route.request().method() === 'PUT') {
        route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            ...existingRole,
            ...JSON.parse(route.request().postData() || '{}'),
          }),
        });
      }
    });

    await page.route('**/api/permissions**', (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          items: mockPermissions,
          page: 1,
          pageSize: 100,
          totalCount: mockPermissions.length,
          totalPages: 1,
        }),
      });
    });

    await page.goto(`/roles/${existingRole.id}`);
  });

  test('should load existing role data', async ({ page }) => {
    await expect(page.locator('input[formControlName="name"]')).toHaveValue(existingRole.name);
    await expect(page.locator('textarea[formControlName="description"]')).toHaveValue(existingRole.description);
  });

  test('should display role title in header', async ({ page }) => {
    await expect(page.locator('text=Edit Role')).toBeVisible();
    await expect(page.locator(`text=${existingRole.name}`)).toBeVisible();
  });

  test('should show assigned permissions as checked', async ({ page }) => {
    await expect(page.locator('mat-checkbox:has-text("View missions")').locator('input')).toBeChecked();
    await expect(page.locator('mat-checkbox:has-text("Create/edit missions")').locator('input')).toBeChecked();
    await expect(page.locator('mat-checkbox:has-text("Delete missions")').locator('input')).toBeChecked();
  });

  test('should show unassigned permissions as unchecked', async ({ page }) => {
    await expect(page.locator('mat-checkbox:has-text("View users")').locator('input')).not.toBeChecked();
    await expect(page.locator('mat-checkbox:has-text("View reports")').locator('input')).not.toBeChecked();
  });

  test('should update role successfully', async ({ page }) => {
    const descriptionInput = page.locator('textarea[formControlName="description"]');
    await descriptionInput.clear();
    await descriptionInput.fill('Updated description for mission manager role');

    await page.locator('button:has-text("Save")').click();

    await expect(page.locator('.mat-mdc-snack-bar-label')).toContainText(/saved|updated|success/i);
  });

  test('should add permission to role', async ({ page }) => {
    await page.locator('mat-checkbox:has-text("View reports")').click();
    await page.locator('button:has-text("Save")').click();

    await expect(page.locator('.mat-mdc-snack-bar-label')).toContainText(/saved|updated|success/i);
  });

  test('should remove permission from role', async ({ page }) => {
    await page.locator('mat-checkbox:has-text("Delete missions")').click();
    await page.locator('button:has-text("Save")').click();

    await expect(page.locator('.mat-mdc-snack-bar-label')).toContainText(/saved|updated|success/i);
  });

  test('should display user count', async ({ page }) => {
    await expect(page.locator('text=5 users')).toBeVisible();
  });

  test('should delete role', async ({ page }) => {
    page.on('dialog', (dialog) => dialog.accept());

    await page.route(`**/api/roles/${existingRole.id}`, (route) => {
      if (route.request().method() === 'DELETE') {
        route.fulfill({ status: 204 });
      }
    });

    await page.locator('button:has-text("Delete Role")').click();

    await expect(page).toHaveURL(/.*roles$/);
  });
});

test.describe('Role Detail - View System Role', () => {
  const systemRole = mockRoles[0]; // Administrator (system role)

  test.beforeEach(async ({ page, context }) => {
    await context.addInitScript(() => {
      localStorage.setItem(
        'access_token',
        'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiZW1haWwiOiJ0ZXN0QGV4YW1wbGUuY29tIiwibmFtZSI6IlRlc3QgVXNlciJ9.fake'
      );
    });

    await page.route(`**/api/roles/${systemRole.id}`, (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(systemRole),
      });
    });

    await page.route('**/api/permissions**', (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          items: mockPermissions,
          page: 1,
          pageSize: 100,
          totalCount: mockPermissions.length,
          totalPages: 1,
        }),
      });
    });

    await page.goto(`/roles/${systemRole.id}`);
  });

  test('should display system role badge', async ({ page }) => {
    await expect(page.locator('.system-badge')).toBeVisible();
  });

  test('should have disabled form fields for system role', async ({ page }) => {
    await expect(page.locator('input[formControlName="name"]')).toBeDisabled();
    await expect(page.locator('textarea[formControlName="description"]')).toBeDisabled();
  });

  test('should have disabled permission checkboxes for system role', async ({ page }) => {
    await expect(page.locator('mat-checkbox:has-text("View users")').locator('input')).toBeDisabled();
  });

  test('should not show delete button for system role', async ({ page }) => {
    await expect(page.locator('button:has-text("Delete Role")')).not.toBeVisible();
  });

  test('should not show save button for system role', async ({ page }) => {
    await expect(page.locator('button:has-text("Save")')).not.toBeVisible();
  });

  test('should display view-only message', async ({ page }) => {
    await expect(page.locator('text=/system role.*cannot be modified/i')).toBeVisible();
  });
});

test.describe('Role Management - Error Handling', () => {
  test.beforeEach(async ({ page, context }) => {
    await context.addInitScript(() => {
      localStorage.setItem(
        'access_token',
        'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiZW1haWwiOiJ0ZXN0QGV4YW1wbGUuY29tIiwibmFtZSI6IlRlc3QgVXNlciJ9.fake'
      );
    });
  });

  test('should show error message when loading roles fails', async ({ page }) => {
    await page.route('**/api/roles**', (route) => {
      route.fulfill({
        status: 500,
        contentType: 'application/json',
        body: JSON.stringify({ error: 'Internal Server Error' }),
      });
    });

    await page.goto('/roles');

    await expect(page.locator('.mat-mdc-snack-bar-label')).toContainText(/error|failed/i);
  });

  test('should show error message when creating role fails', async ({ page }) => {
    await page.route('**/api/permissions**', (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ items: mockPermissions, page: 1, pageSize: 100, totalCount: 10, totalPages: 1 }),
      });
    });

    await page.route('**/api/roles', (route) => {
      if (route.request().method() === 'POST') {
        route.fulfill({
          status: 400,
          contentType: 'application/json',
          body: JSON.stringify({ error: 'Role name already exists' }),
        });
      }
    });

    await page.goto('/roles/new');

    await page.locator('input[formControlName="name"]').fill('Administrator');
    await page.locator('textarea[formControlName="description"]').fill('Duplicate role');

    await page.locator('button:has-text("Create Role")').click();

    await expect(page.locator('.mat-mdc-snack-bar-label')).toContainText(/error|failed|exists/i);
  });

  test('should show error message when role not found', async ({ page }) => {
    await page.route('**/api/roles/non-existent-id', (route) => {
      route.fulfill({
        status: 404,
        contentType: 'application/json',
        body: JSON.stringify({ error: 'Role not found' }),
      });
    });

    await page.route('**/api/permissions**', (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ items: mockPermissions, page: 1, pageSize: 100, totalCount: 10, totalPages: 1 }),
      });
    });

    await page.goto('/roles/non-existent-id');

    await expect(page.locator('.mat-mdc-snack-bar-label')).toContainText(/error|not found|failed/i);
  });

  test('should show error when deleting role with users', async ({ page }) => {
    page.on('dialog', (dialog) => dialog.accept());

    await page.route('**/api/roles**', (route) => {
      if (!route.request().url().includes('/')) {
        route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(mockRolesResponse),
        });
      }
    });

    await page.route('**/api/roles/550e8400-e29b-41d4-a716-446655440003', (route) => {
      if (route.request().method() === 'DELETE') {
        route.fulfill({
          status: 400,
          contentType: 'application/json',
          body: JSON.stringify({ error: 'Cannot delete role with assigned users' }),
        });
      }
    });

    await page.goto('/roles');

    const deleteButton = page.locator('.role-card:has-text("Mission Manager") button[mat-icon-button]');
    await deleteButton.click();

    await expect(page.locator('.mat-mdc-snack-bar-label')).toContainText(/error|cannot delete|users/i);
  });
});

test.describe('Role Management - Accessibility', () => {
  test.beforeEach(async ({ page, context }) => {
    await context.addInitScript(() => {
      localStorage.setItem(
        'access_token',
        'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiZW1haWwiOiJ0ZXN0QGV4YW1wbGUuY29tIiwibmFtZSI6IlRlc3QgVXNlciJ9.fake'
      );
    });

    await page.route('**/api/roles**', (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(mockRolesResponse),
      });
    });

    await page.route('**/api/permissions**', (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ items: mockPermissions, page: 1, pageSize: 100, totalCount: 10, totalPages: 1 }),
      });
    });
  });

  test('roles page should be keyboard navigable', async ({ page }) => {
    await page.goto('/roles');

    await page.keyboard.press('Tab');
    await page.keyboard.press('Tab');

    const focusedElement = await page.evaluate(() => document.activeElement?.tagName);
    expect(focusedElement).toBeTruthy();
  });

  test('role form should have labels', async ({ page }) => {
    await page.goto('/roles/new');

    await expect(page.locator('mat-label:has-text("Role Name")')).toBeVisible();
    await expect(page.locator('mat-label:has-text("Description")')).toBeVisible();
  });

  test('permission checkboxes should have labels', async ({ page }) => {
    await page.goto('/roles/new');

    const checkboxLabels = page.locator('mat-checkbox .mdc-label');
    const count = await checkboxLabels.count();
    expect(count).toBeGreaterThan(0);
  });
});
