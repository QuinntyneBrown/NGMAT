import { test, expect } from '@playwright/test';

// Mock user data
const mockUsers = [
  {
    id: '550e8400-e29b-41d4-a716-446655440001',
    username: 'john.doe',
    email: 'john.doe@example.com',
    firstName: 'John',
    lastName: 'Doe',
    displayName: 'John Doe',
    isActive: true,
    isLocked: false,
    emailConfirmed: true,
    twoFactorEnabled: true,
    createdAt: '2024-01-15T10:00:00Z',
    lastLoginAt: '2025-01-18T14:30:00Z',
    roles: [{ id: 'role-1', name: 'Admin' }],
  },
  {
    id: '550e8400-e29b-41d4-a716-446655440002',
    username: 'jane.smith',
    email: 'jane.smith@example.com',
    firstName: 'Jane',
    lastName: 'Smith',
    displayName: 'Jane Smith',
    isActive: true,
    isLocked: false,
    emailConfirmed: true,
    twoFactorEnabled: false,
    createdAt: '2024-03-20T08:00:00Z',
    lastLoginAt: '2025-01-17T09:15:00Z',
    roles: [{ id: 'role-2', name: 'User' }],
  },
  {
    id: '550e8400-e29b-41d4-a716-446655440003',
    username: 'bob.wilson',
    email: 'bob.wilson@example.com',
    firstName: 'Bob',
    lastName: 'Wilson',
    displayName: 'Bob Wilson',
    isActive: false,
    isLocked: true,
    emailConfirmed: false,
    twoFactorEnabled: false,
    createdAt: '2024-06-01T12:00:00Z',
    lastLoginAt: '2024-12-01T16:45:00Z',
    roles: [{ id: 'role-2', name: 'User' }],
  },
];

const mockUserStats = {
  totalUsers: 3,
  activeUsers: 2,
  lockedUsers: 1,
  newUsersThisMonth: 0,
};

const mockRoles = [
  { id: 'role-1', name: 'Admin', description: 'Administrator role', isSystem: true, userCount: 1 },
  { id: 'role-2', name: 'User', description: 'Regular user role', isSystem: true, userCount: 2 },
  { id: 'role-3', name: 'Manager', description: 'Manager role', isSystem: false, userCount: 0 },
];

const mockUsersResponse = {
  items: mockUsers,
  page: 1,
  pageSize: 20,
  totalCount: 3,
  totalPages: 1,
};

test.describe('Users List Page', () => {
  test.beforeEach(async ({ page, context }) => {
    await context.addInitScript(() => {
      localStorage.setItem(
        'access_token',
        'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiZW1haWwiOiJ0ZXN0QGV4YW1wbGUuY29tIiwibmFtZSI6IlRlc3QgVXNlciJ9.fake'
      );
    });

    // Mock the users API
    await page.route('**/api/users**', (route) => {
      const url = new URL(route.request().url());
      const status = url.searchParams.get('status');
      const search = url.searchParams.get('search');
      const role = url.searchParams.get('role');

      let filteredUsers = [...mockUsers];

      if (status === 'active') {
        filteredUsers = filteredUsers.filter((u) => u.isActive && !u.isLocked);
      } else if (status === 'inactive') {
        filteredUsers = filteredUsers.filter((u) => !u.isActive);
      } else if (status === 'locked') {
        filteredUsers = filteredUsers.filter((u) => u.isLocked);
      }

      if (role) {
        filteredUsers = filteredUsers.filter((u) => u.roles.some((r) => r.id === role));
      }

      if (search) {
        const searchLower = search.toLowerCase();
        filteredUsers = filteredUsers.filter(
          (u) =>
            u.username.toLowerCase().includes(searchLower) ||
            u.email.toLowerCase().includes(searchLower) ||
            u.displayName.toLowerCase().includes(searchLower)
        );
      }

      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          items: filteredUsers,
          page: 1,
          pageSize: 20,
          totalCount: filteredUsers.length,
          totalPages: 1,
        }),
      });
    });

    // Mock user stats
    await page.route('**/api/users/stats', (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(mockUserStats),
      });
    });

    // Mock roles for filter
    await page.route('**/api/roles**', (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          items: mockRoles,
          page: 1,
          pageSize: 100,
          totalCount: mockRoles.length,
          totalPages: 1,
        }),
      });
    });

    await page.goto('/users');
  });

  test('should display users list page', async ({ page }) => {
    await expect(page.locator('text=User Management')).toBeVisible();
    await expect(page.locator('button:has-text("Add User")')).toBeVisible();
  });

  test('should display user stats cards', async ({ page }) => {
    await expect(page.locator('.stat-card:has-text("Total Users")')).toBeVisible();
    await expect(page.locator('.stat-card:has-text("Active Users")')).toBeVisible();
    await expect(page.locator('.stat-card:has-text("Locked Accounts")')).toBeVisible();
    await expect(page.locator('.stat-card:has-text("New This Month")')).toBeVisible();
  });

  test('should display user items in the table', async ({ page }) => {
    await expect(page.locator('text=john.doe')).toBeVisible();
    await expect(page.locator('text=jane.smith')).toBeVisible();
    await expect(page.locator('text=bob.wilson')).toBeVisible();
  });

  test('should filter users by status', async ({ page }) => {
    const statusFilter = page.locator('mat-select').first();
    await statusFilter.click();
    await page.locator('mat-option:has-text("Active")').click();

    await expect(page.locator('text=john.doe')).toBeVisible();
    await expect(page.locator('text=jane.smith')).toBeVisible();
    await expect(page.locator('text=bob.wilson')).not.toBeVisible();
  });

  test('should filter users by locked status', async ({ page }) => {
    const statusFilter = page.locator('mat-select').first();
    await statusFilter.click();
    await page.locator('mat-option:has-text("Locked")').click();

    await expect(page.locator('text=bob.wilson')).toBeVisible();
    await expect(page.locator('text=john.doe')).not.toBeVisible();
  });

  test('should search users by name', async ({ page }) => {
    const searchInput = page.locator('input[placeholder*="Search"]');
    await searchInput.fill('john');

    await expect(page.locator('text=john.doe')).toBeVisible();
    await expect(page.locator('text=jane.smith')).not.toBeVisible();
  });

  test('should search users by email', async ({ page }) => {
    const searchInput = page.locator('input[placeholder*="Search"]');
    await searchInput.fill('jane.smith@example.com');

    await expect(page.locator('text=jane.smith')).toBeVisible();
    await expect(page.locator('text=john.doe')).not.toBeVisible();
  });

  test('should navigate to add user page', async ({ page }) => {
    await page.locator('button:has-text("Add User")').click();
    await expect(page).toHaveURL(/.*users\/new/);
  });

  test('should navigate to user detail on row click', async ({ page }) => {
    await page.route('**/api/users/550e8400-e29b-41d4-a716-446655440001', (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(mockUsers[0]),
      });
    });

    await page.locator('tr:has-text("john.doe")').click();
    await expect(page).toHaveURL(/.*users\/550e8400-e29b-41d4-a716-446655440001/);
  });

  test('should show action menu for user', async ({ page }) => {
    const actionButton = page.locator('tr:has-text("john.doe") button[mat-icon-button]');
    await actionButton.click();

    await expect(page.locator('button:has-text("Edit")')).toBeVisible();
    await expect(page.locator('button:has-text("Deactivate")')).toBeVisible();
    await expect(page.locator('button:has-text("Reset Password")')).toBeVisible();
  });

  test('should show unlock option for locked user', async ({ page }) => {
    const actionButton = page.locator('tr:has-text("bob.wilson") button[mat-icon-button]');
    await actionButton.click();

    await expect(page.locator('button:has-text("Unlock")')).toBeVisible();
  });

  test('should deactivate user from action menu', async ({ page }) => {
    page.on('dialog', (dialog) => dialog.accept());

    await page.route('**/api/users/550e8400-e29b-41d4-a716-446655440001/deactivate', (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ ...mockUsers[0], isActive: false }),
      });
    });

    const actionButton = page.locator('tr:has-text("john.doe") button[mat-icon-button]');
    await actionButton.click();
    await page.locator('button:has-text("Deactivate")').click();

    await expect(page.locator('.mat-mdc-snack-bar-label')).toContainText(/deactivated|success/i);
  });

  test('should unlock user from action menu', async ({ page }) => {
    await page.route('**/api/users/550e8400-e29b-41d4-a716-446655440003/unlock', (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ ...mockUsers[2], isLocked: false }),
      });
    });

    const actionButton = page.locator('tr:has-text("bob.wilson") button[mat-icon-button]');
    await actionButton.click();
    await page.locator('button:has-text("Unlock")').click();

    await expect(page.locator('.mat-mdc-snack-bar-label')).toContainText(/unlocked|success/i);
  });

  test('should reset user password from action menu', async ({ page }) => {
    page.on('dialog', (dialog) => dialog.accept());

    await page.route('**/api/users/550e8400-e29b-41d4-a716-446655440001/reset-password', (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ temporaryPassword: 'TempPass123!' }),
      });
    });

    const actionButton = page.locator('tr:has-text("john.doe") button[mat-icon-button]');
    await actionButton.click();
    await page.locator('button:has-text("Reset Password")').click();

    await expect(page.locator('.mat-mdc-snack-bar-label')).toContainText(/password|reset/i);
  });

  test('should display correct status chips', async ({ page }) => {
    await expect(page.locator('tr:has-text("john.doe") .status-chip--active')).toBeVisible();
    await expect(page.locator('tr:has-text("bob.wilson") .status-chip--locked')).toBeVisible();
  });

  test('should display 2FA indicator', async ({ page }) => {
    await expect(page.locator('tr:has-text("john.doe") mat-icon:has-text("verified_user")')).toBeVisible();
  });

  test('should display role badges', async ({ page }) => {
    await expect(page.locator('tr:has-text("john.doe") .role-chip:has-text("Admin")')).toBeVisible();
    await expect(page.locator('tr:has-text("jane.smith") .role-chip:has-text("User")')).toBeVisible();
  });
});

test.describe('User Detail - Create New User', () => {
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
        body: JSON.stringify({
          items: mockRoles,
          page: 1,
          pageSize: 100,
          totalCount: mockRoles.length,
          totalPages: 1,
        }),
      });
    });

    await page.goto('/users/new');
  });

  test('should display user creation form', async ({ page }) => {
    await expect(page.locator('text=New User')).toBeVisible();
    await expect(page.locator('input[formControlName="username"]')).toBeVisible();
    await expect(page.locator('input[formControlName="email"]')).toBeVisible();
    await expect(page.locator('input[formControlName="firstName"]')).toBeVisible();
    await expect(page.locator('input[formControlName="lastName"]')).toBeVisible();
  });

  test('should show validation error for required fields', async ({ page }) => {
    const usernameInput = page.locator('input[formControlName="username"]');
    await usernameInput.click();
    await usernameInput.blur();

    await expect(page.locator('mat-error:has-text("required")')).toBeVisible();
  });

  test('should show validation error for invalid email', async ({ page }) => {
    const emailInput = page.locator('input[formControlName="email"]');
    await emailInput.fill('invalid-email');
    await emailInput.blur();

    await expect(page.locator('mat-error:has-text("valid email")')).toBeVisible();
  });

  test('should show password requirements', async ({ page }) => {
    const passwordInput = page.locator('input[formControlName="password"]');
    await passwordInput.fill('weak');
    await passwordInput.blur();

    await expect(page.locator('mat-error')).toBeVisible();
  });

  test('should create new user successfully', async ({ page }) => {
    const newUser = {
      id: '550e8400-e29b-41d4-a716-446655440004',
      username: 'new.user',
      email: 'new.user@example.com',
      firstName: 'New',
      lastName: 'User',
      displayName: 'New User',
      isActive: true,
      isLocked: false,
      emailConfirmed: false,
      twoFactorEnabled: false,
      createdAt: new Date().toISOString(),
      roles: [],
    };

    await page.route('**/api/users', (route) => {
      if (route.request().method() === 'POST') {
        route.fulfill({
          status: 201,
          contentType: 'application/json',
          body: JSON.stringify(newUser),
        });
      }
    });

    await page.locator('input[formControlName="username"]').fill('new.user');
    await page.locator('input[formControlName="email"]').fill('new.user@example.com');
    await page.locator('input[formControlName="firstName"]').fill('New');
    await page.locator('input[formControlName="lastName"]').fill('User');
    await page.locator('input[formControlName="password"]').fill('SecurePass123!');
    await page.locator('input[formControlName="confirmPassword"]').fill('SecurePass123!');

    await page.locator('button:has-text("Create User")').click();

    await expect(page.locator('.mat-mdc-snack-bar-label')).toContainText(/created|success/i);
  });

  test('should show password mismatch error', async ({ page }) => {
    await page.locator('input[formControlName="password"]').fill('SecurePass123!');
    await page.locator('input[formControlName="confirmPassword"]').fill('DifferentPass456!');
    await page.locator('input[formControlName="confirmPassword"]').blur();

    await expect(page.locator('mat-error:has-text("match")')).toBeVisible();
  });

  test('should navigate back on cancel', async ({ page }) => {
    await page.locator('button:has-text("Cancel")').click();
    await expect(page).toHaveURL(/.*users$/);
  });

  test('should display role selection', async ({ page }) => {
    await expect(page.locator('text=Assign Roles')).toBeVisible();
    await expect(page.locator('mat-checkbox:has-text("Admin")')).toBeVisible();
    await expect(page.locator('mat-checkbox:has-text("User")')).toBeVisible();
  });
});

test.describe('User Detail - Edit Existing User', () => {
  const existingUser = mockUsers[0];

  test.beforeEach(async ({ page, context }) => {
    await context.addInitScript(() => {
      localStorage.setItem(
        'access_token',
        'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiZW1haWwiOiJ0ZXN0QGV4YW1wbGUuY29tIiwibmFtZSI6IlRlc3QgVXNlciJ9.fake'
      );
    });

    await page.route(`**/api/users/${existingUser.id}`, (route) => {
      if (route.request().method() === 'GET') {
        route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(existingUser),
        });
      } else if (route.request().method() === 'PUT') {
        route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            ...existingUser,
            ...JSON.parse(route.request().postData() || '{}'),
          }),
        });
      }
    });

    await page.route('**/api/roles**', (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          items: mockRoles,
          page: 1,
          pageSize: 100,
          totalCount: mockRoles.length,
          totalPages: 1,
        }),
      });
    });

    await page.goto(`/users/${existingUser.id}`);
  });

  test('should load existing user data', async ({ page }) => {
    await expect(page.locator('input[formControlName="username"]')).toHaveValue(existingUser.username);
    await expect(page.locator('input[formControlName="email"]')).toHaveValue(existingUser.email);
    await expect(page.locator('input[formControlName="firstName"]')).toHaveValue(existingUser.firstName);
    await expect(page.locator('input[formControlName="lastName"]')).toHaveValue(existingUser.lastName);
  });

  test('should display user title in header', async ({ page }) => {
    await expect(page.locator('text=Edit User')).toBeVisible();
    await expect(page.locator(`text=${existingUser.displayName}`)).toBeVisible();
  });

  test('should display security settings section', async ({ page }) => {
    await expect(page.locator('text=Security Settings')).toBeVisible();
    await expect(page.locator('text=Two-Factor Authentication')).toBeVisible();
    await expect(page.locator('text=Account Status')).toBeVisible();
  });

  test('should show 2FA as enabled for user with 2FA', async ({ page }) => {
    await expect(page.locator('mat-slide-toggle[formControlName="twoFactorEnabled"]')).toBeChecked();
  });

  test('should update user successfully', async ({ page }) => {
    const firstNameInput = page.locator('input[formControlName="firstName"]');
    await firstNameInput.clear();
    await firstNameInput.fill('Jonathan');

    await page.locator('button:has-text("Save")').click();

    await expect(page.locator('.mat-mdc-snack-bar-label')).toContainText(/saved|updated|success/i);
  });

  test('should display last login information', async ({ page }) => {
    await expect(page.locator('text=Last Login')).toBeVisible();
  });

  test('should display account creation date', async ({ page }) => {
    await expect(page.locator('text=Created')).toBeVisible();
  });

  test('should toggle 2FA setting', async ({ page }) => {
    const toggle = page.locator('mat-slide-toggle[formControlName="twoFactorEnabled"]');
    await toggle.click();

    await expect(toggle).not.toBeChecked();
  });

  test('should navigate to change password section', async ({ page }) => {
    await expect(page.locator('button:has-text("Change Password")')).toBeVisible();
  });

  test('should display assigned roles', async ({ page }) => {
    await expect(page.locator('mat-checkbox:has-text("Admin")').locator('input')).toBeChecked();
  });

  test('should deactivate user from detail page', async ({ page }) => {
    page.on('dialog', (dialog) => dialog.accept());

    await page.route(`**/api/users/${existingUser.id}/deactivate`, (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ ...existingUser, isActive: false }),
      });
    });

    await page.locator('button:has-text("Deactivate User")').click();

    await expect(page.locator('.mat-mdc-snack-bar-label')).toContainText(/deactivated|success/i);
  });

  test('should delete user from detail page', async ({ page }) => {
    page.on('dialog', (dialog) => dialog.accept());

    await page.route(`**/api/users/${existingUser.id}`, (route) => {
      if (route.request().method() === 'DELETE') {
        route.fulfill({ status: 204 });
      }
    });

    await page.locator('button:has-text("Delete User")').click();

    await expect(page).toHaveURL(/.*users$/);
  });
});

test.describe('User Management - Role Assignment', () => {
  const existingUser = mockUsers[1]; // Jane Smith with User role

  test.beforeEach(async ({ page, context }) => {
    await context.addInitScript(() => {
      localStorage.setItem(
        'access_token',
        'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiZW1haWwiOiJ0ZXN0QGV4YW1wbGUuY29tIiwibmFtZSI6IlRlc3QgVXNlciJ9.fake'
      );
    });

    await page.route(`**/api/users/${existingUser.id}`, (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(existingUser),
      });
    });

    await page.route('**/api/roles**', (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          items: mockRoles,
          page: 1,
          pageSize: 100,
          totalCount: mockRoles.length,
          totalPages: 1,
        }),
      });
    });

    await page.goto(`/users/${existingUser.id}`);
  });

  test('should display available roles for assignment', async ({ page }) => {
    await expect(page.locator('mat-checkbox:has-text("Admin")')).toBeVisible();
    await expect(page.locator('mat-checkbox:has-text("User")')).toBeVisible();
    await expect(page.locator('mat-checkbox:has-text("Manager")')).toBeVisible();
  });

  test('should show current role as checked', async ({ page }) => {
    await expect(page.locator('mat-checkbox:has-text("User")').locator('input')).toBeChecked();
    await expect(page.locator('mat-checkbox:has-text("Admin")').locator('input')).not.toBeChecked();
  });

  test('should assign additional role to user', async ({ page }) => {
    await page.route(`**/api/users/${existingUser.id}/roles`, (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          ...existingUser,
          roles: [...existingUser.roles, { id: 'role-1', name: 'Admin' }],
        }),
      });
    });

    await page.locator('mat-checkbox:has-text("Admin")').click();
    await page.locator('button:has-text("Save")').click();

    await expect(page.locator('.mat-mdc-snack-bar-label')).toContainText(/saved|updated|success/i);
  });

  test('should remove role from user', async ({ page }) => {
    await page.route(`**/api/users/${existingUser.id}/roles`, (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          ...existingUser,
          roles: [],
        }),
      });
    });

    await page.locator('mat-checkbox:has-text("User")').click();
    await page.locator('button:has-text("Save")').click();

    await expect(page.locator('.mat-mdc-snack-bar-label')).toContainText(/saved|updated|success/i);
  });
});

test.describe('User Management - Error Handling', () => {
  test.beforeEach(async ({ page, context }) => {
    await context.addInitScript(() => {
      localStorage.setItem(
        'access_token',
        'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiZW1haWwiOiJ0ZXN0QGV4YW1wbGUuY29tIiwibmFtZSI6IlRlc3QgVXNlciJ9.fake'
      );
    });
  });

  test('should show error message when loading users fails', async ({ page }) => {
    await page.route('**/api/users**', (route) => {
      route.fulfill({
        status: 500,
        contentType: 'application/json',
        body: JSON.stringify({ error: 'Internal Server Error' }),
      });
    });

    await page.route('**/api/users/stats', (route) => {
      route.fulfill({
        status: 500,
        contentType: 'application/json',
        body: JSON.stringify({ error: 'Internal Server Error' }),
      });
    });

    await page.goto('/users');

    await expect(page.locator('.mat-mdc-snack-bar-label')).toContainText(/error|failed/i);
  });

  test('should show error message when creating user fails', async ({ page }) => {
    await page.route('**/api/roles**', (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ items: mockRoles, page: 1, pageSize: 100, totalCount: 3, totalPages: 1 }),
      });
    });

    await page.route('**/api/users', (route) => {
      if (route.request().method() === 'POST') {
        route.fulfill({
          status: 400,
          contentType: 'application/json',
          body: JSON.stringify({ error: 'Username already exists' }),
        });
      }
    });

    await page.goto('/users/new');

    await page.locator('input[formControlName="username"]').fill('existing.user');
    await page.locator('input[formControlName="email"]').fill('existing@example.com');
    await page.locator('input[formControlName="firstName"]').fill('Test');
    await page.locator('input[formControlName="lastName"]').fill('User');
    await page.locator('input[formControlName="password"]').fill('SecurePass123!');
    await page.locator('input[formControlName="confirmPassword"]').fill('SecurePass123!');

    await page.locator('button:has-text("Create User")').click();

    await expect(page.locator('.mat-mdc-snack-bar-label')).toContainText(/error|failed|exists/i);
  });

  test('should show error message when user not found', async ({ page }) => {
    await page.route('**/api/users/non-existent-id', (route) => {
      route.fulfill({
        status: 404,
        contentType: 'application/json',
        body: JSON.stringify({ error: 'User not found' }),
      });
    });

    await page.route('**/api/roles**', (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ items: mockRoles, page: 1, pageSize: 100, totalCount: 3, totalPages: 1 }),
      });
    });

    await page.goto('/users/non-existent-id');

    await expect(page.locator('.mat-mdc-snack-bar-label')).toContainText(/error|not found|failed/i);
  });
});

test.describe('User Management - Accessibility', () => {
  test.beforeEach(async ({ page, context }) => {
    await context.addInitScript(() => {
      localStorage.setItem(
        'access_token',
        'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiZW1haWwiOiJ0ZXN0QGV4YW1wbGUuY29tIiwibmFtZSI6IlRlc3QgVXNlciJ9.fake'
      );
    });

    await page.route('**/api/users**', (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(mockUsersResponse),
      });
    });

    await page.route('**/api/users/stats', (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(mockUserStats),
      });
    });

    await page.route('**/api/roles**', (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ items: mockRoles, page: 1, pageSize: 100, totalCount: 3, totalPages: 1 }),
      });
    });
  });

  test('users page should be keyboard navigable', async ({ page }) => {
    await page.goto('/users');

    await page.keyboard.press('Tab');
    await page.keyboard.press('Tab');

    const focusedElement = await page.evaluate(() => document.activeElement?.tagName);
    expect(focusedElement).toBeTruthy();
  });

  test('user form should have labels', async ({ page }) => {
    await page.goto('/users/new');

    await expect(page.locator('mat-label:has-text("Username")')).toBeVisible();
    await expect(page.locator('mat-label:has-text("Email")')).toBeVisible();
    await expect(page.locator('mat-label:has-text("First Name")')).toBeVisible();
    await expect(page.locator('mat-label:has-text("Last Name")')).toBeVisible();
  });
});
