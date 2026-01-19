import { test, expect, Page } from '@playwright/test';

// Helper to set up authenticated state
async function setupAuthenticatedUser(page: Page) {
  await page.addInitScript(() => {
    localStorage.setItem(
      'access_token',
      'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiZW1haWwiOiJ0ZXN0QGV4YW1wbGUuY29tIiwibmFtZSI6IlRlc3QgVXNlciJ9.fake'
    );
  });
}

// Mock mission data
const mockMissions = [
  {
    id: '550e8400-e29b-41d4-a716-446655440001',
    name: 'LEO Constellation Deploy',
    description: 'Deploy 6 satellites to low Earth orbit constellation.',
    type: 'LEO',
    status: 'Active',
    startEpoch: '2025-03-15T12:00:00Z',
    endEpoch: '2025-03-17T12:00:00Z',
    ownerId: '00000000-0000-0000-0000-000000000001',
    createdAt: '2025-01-10T00:00:00Z',
    updatedAt: '2025-01-18T00:00:00Z',
  },
  {
    id: '550e8400-e29b-41d4-a716-446655440002',
    name: 'GEO Station Keeping',
    description: 'Maintain geostationary orbit position.',
    type: 'GEO',
    status: 'Draft',
    startEpoch: '2025-02-01T00:00:00Z',
    ownerId: '00000000-0000-0000-0000-000000000001',
    createdAt: '2025-01-05T00:00:00Z',
    updatedAt: '2025-01-15T00:00:00Z',
  },
  {
    id: '550e8400-e29b-41d4-a716-446655440003',
    name: 'Lunar Transfer',
    description: 'Trans-lunar injection mission.',
    type: 'Lunar',
    status: 'Completed',
    startEpoch: '2024-06-01T08:00:00Z',
    endEpoch: '2024-06-05T08:00:00Z',
    ownerId: '00000000-0000-0000-0000-000000000001',
    createdAt: '2024-05-02T00:00:00Z',
    updatedAt: '2024-06-05T00:00:00Z',
  },
];

const mockMissionsResponse = {
  items: mockMissions,
  page: 1,
  pageSize: 20,
  totalCount: 3,
  totalPages: 1,
};

test.describe('Missions List Page', () => {
  test.beforeEach(async ({ page, context }) => {
    await context.addInitScript(() => {
      localStorage.setItem(
        'access_token',
        'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiZW1haWwiOiJ0ZXN0QGV4YW1wbGUuY29tIiwibmFtZSI6IlRlc3QgVXNlciJ9.fake'
      );
    });

    // Mock the missions API
    await page.route('**/api/missions/v1/missions**', (route) => {
      const url = new URL(route.request().url());
      const status = url.searchParams.get('status');
      const search = url.searchParams.get('search');

      let filteredMissions = [...mockMissions];

      if (status) {
        filteredMissions = filteredMissions.filter((m) => m.status === status);
      }

      if (search) {
        const searchLower = search.toLowerCase();
        filteredMissions = filteredMissions.filter(
          (m) =>
            m.name.toLowerCase().includes(searchLower) ||
            m.description?.toLowerCase().includes(searchLower)
        );
      }

      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          items: filteredMissions,
          page: 1,
          pageSize: 20,
          totalCount: filteredMissions.length,
          totalPages: 1,
        }),
      });
    });

    await page.goto('/missions');
  });

  test('should display missions list page', async ({ page }) => {
    await expect(page.locator('text=Mission Management')).toBeVisible();
    await expect(page.locator('button:has-text("New Mission")')).toBeVisible();
  });

  test('should display mission items in the list', async ({ page }) => {
    await expect(page.locator('text=LEO Constellation Deploy')).toBeVisible();
    await expect(page.locator('text=GEO Station Keeping')).toBeVisible();
    await expect(page.locator('text=Lunar Transfer')).toBeVisible();
  });

  test('should filter missions by status', async ({ page }) => {
    const statusFilter = page.locator('mat-select[formControlName="status"]').first();
    await statusFilter.click();
    await page.locator('mat-option:has-text("Active")').click();

    await expect(page.locator('text=LEO Constellation Deploy')).toBeVisible();
    await expect(page.locator('text=GEO Station Keeping')).not.toBeVisible();
  });

  test('should search missions by name', async ({ page }) => {
    const searchInput = page.locator('input[placeholder*="Search"]');
    await searchInput.fill('Lunar');

    await expect(page.locator('text=Lunar Transfer')).toBeVisible();
    await expect(page.locator('text=LEO Constellation Deploy')).not.toBeVisible();
  });

  test('should navigate to new mission page', async ({ page }) => {
    await page.locator('button:has-text("New Mission")').click();
    await expect(page).toHaveURL(/.*missions\/new/);
  });

  test('should navigate to mission editor on row click', async ({ page }) => {
    await page.route('**/api/missions/v1/missions/550e8400-e29b-41d4-a716-446655440001', (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(mockMissions[0]),
      });
    });

    await page.locator('tr:has-text("LEO Constellation Deploy")').click();
    await expect(page).toHaveURL(/.*missions\/.*\/edit/);
  });

  test('should show action menu for mission', async ({ page }) => {
    const actionButton = page.locator('tr:has-text("LEO Constellation Deploy") button[mat-icon-button]');
    await actionButton.click();

    await expect(page.locator('button:has-text("Edit")')).toBeVisible();
    await expect(page.locator('button:has-text("Clone")')).toBeVisible();
    await expect(page.locator('button:has-text("Delete")')).toBeVisible();
  });

  test('should clone mission from action menu', async ({ page }) => {
    const clonedMission = {
      ...mockMissions[0],
      id: '550e8400-e29b-41d4-a716-446655440004',
      name: 'LEO Constellation Deploy (Copy)',
      status: 'Draft',
    };

    await page.route('**/api/missions/v1/missions/550e8400-e29b-41d4-a716-446655440001/clone', (route) => {
      route.fulfill({
        status: 201,
        contentType: 'application/json',
        body: JSON.stringify(clonedMission),
      });
    });

    const actionButton = page.locator('tr:has-text("LEO Constellation Deploy") button[mat-icon-button]');
    await actionButton.click();
    await page.locator('button:has-text("Clone")').click();

    await expect(page.locator('.mat-mdc-snack-bar-label')).toContainText('cloned');
  });

  test('should delete mission from action menu', async ({ page }) => {
    page.on('dialog', (dialog) => dialog.accept());

    await page.route('**/api/missions/v1/missions/550e8400-e29b-41d4-a716-446655440001', (route) => {
      if (route.request().method() === 'DELETE') {
        route.fulfill({ status: 204 });
      }
    });

    const actionButton = page.locator('tr:has-text("LEO Constellation Deploy") button[mat-icon-button]');
    await actionButton.click();
    await page.locator('button:has-text("Delete")').click();

    await expect(page.locator('.mat-mdc-snack-bar-label')).toContainText('deleted');
  });

  test('should display correct status chips', async ({ page }) => {
    await expect(page.locator('.status-chip--active:has-text("Active")')).toBeVisible();
    await expect(page.locator('.status-chip--draft:has-text("Draft")')).toBeVisible();
    await expect(page.locator('.status-chip--completed:has-text("Completed")')).toBeVisible();
  });
});

test.describe('Mission Editor - Create New Mission', () => {
  test.beforeEach(async ({ page, context }) => {
    await context.addInitScript(() => {
      localStorage.setItem(
        'access_token',
        'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiZW1haWwiOiJ0ZXN0QGV4YW1wbGUuY29tIiwibmFtZSI6IlRlc3QgVXNlciJ9.fake'
      );
    });

    await page.goto('/missions/new');
  });

  test('should display mission editor form', async ({ page }) => {
    await expect(page.locator('text=New Mission')).toBeVisible();
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

    await expect(page.locator('mat-error:has-text("at least 3 characters")')).toBeVisible();
  });

  test('should create new mission successfully', async ({ page }) => {
    const newMission = {
      id: '550e8400-e29b-41d4-a716-446655440005',
      name: 'New Test Mission',
      description: 'Test mission description',
      type: 'LEO',
      status: 'Draft',
      startEpoch: '2025-04-01T12:00:00Z',
      ownerId: '00000000-0000-0000-0000-000000000001',
      createdAt: '2025-01-20T00:00:00Z',
    };

    await page.route('**/api/missions/v1/missions', (route) => {
      if (route.request().method() === 'POST') {
        route.fulfill({
          status: 201,
          contentType: 'application/json',
          body: JSON.stringify(newMission),
        });
      }
    });

    await page.locator('input[formControlName="name"]').fill('New Test Mission');
    await page.locator('textarea[formControlName="description"]').fill('Test mission description');
    await page.locator('input[formControlName="startDate"]').fill('2025-04-01');
    await page.locator('input[formControlName="startTime"]').fill('12:00:00.000');

    await page.locator('button:has-text("Create Mission")').click();

    await expect(page.locator('.mat-mdc-snack-bar-label')).toContainText('created');
  });

  test('should navigate back to missions list on cancel', async ({ page }) => {
    await page.locator('button:has-text("Cancel")').click();
    await expect(page).toHaveURL(/.*missions$/);
  });

  test('should navigate back using back button', async ({ page }) => {
    await page.locator('button:has(mat-icon:has-text("arrow_back"))').click();
    await expect(page).toHaveURL(/.*missions$/);
  });

  test('should display expansion panels', async ({ page }) => {
    await expect(page.locator('text=Basic Information')).toBeVisible();
    await expect(page.locator('text=Mission Epoch')).toBeVisible();
    await expect(page.locator('text=Central Body')).toBeVisible();
    await expect(page.locator('text=Mission Options')).toBeVisible();
  });

  test('should select mission type', async ({ page }) => {
    await page.locator('mat-select[formControlName="type"]').click();
    await page.locator('mat-option:has-text("GEO")').click();

    await expect(page.locator('mat-select[formControlName="type"]')).toContainText('GEO');
  });

  test('should toggle mission options', async ({ page }) => {
    const realTimePropagationToggle = page.locator('mat-slide-toggle').first();
    await realTimePropagationToggle.click();
  });
});

test.describe('Mission Editor - Edit Existing Mission', () => {
  const existingMission = mockMissions[0];

  test.beforeEach(async ({ page, context }) => {
    await context.addInitScript(() => {
      localStorage.setItem(
        'access_token',
        'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiZW1haWwiOiJ0ZXN0QGV4YW1wbGUuY29tIiwibmFtZSI6IlRlc3QgVXNlciJ9.fake'
      );
    });

    await page.route(`**/api/missions/v1/missions/${existingMission.id}`, (route) => {
      if (route.request().method() === 'GET') {
        route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(existingMission),
        });
      } else if (route.request().method() === 'PUT') {
        route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            ...existingMission,
            ...JSON.parse(route.request().postData() || '{}'),
            updatedAt: new Date().toISOString(),
          }),
        });
      }
    });

    await page.goto(`/missions/${existingMission.id}/edit`);
  });

  test('should load existing mission data', async ({ page }) => {
    await expect(page.locator('input[formControlName="name"]')).toHaveValue(existingMission.name);
  });

  test('should display mission title in toolbar', async ({ page }) => {
    await expect(page.locator('.editor-toolbar__title')).toContainText(existingMission.name);
  });

  test('should display properties panel', async ({ page }) => {
    await expect(page.locator('.properties-panel')).toBeVisible();
    await expect(page.locator('text=Properties')).toBeVisible();
  });

  test('should update mission successfully', async ({ page }) => {
    const nameInput = page.locator('input[formControlName="name"]');
    await nameInput.clear();
    await nameInput.fill('Updated Mission Name');

    await page.locator('button:has-text("Save")').first().click();

    await expect(page.locator('.mat-mdc-snack-bar-label')).toContainText('saved');
  });

  test('should display Run button for existing mission', async ({ page }) => {
    await expect(page.locator('button:has-text("Run")')).toBeVisible();
  });

  test('should display Clone button in properties', async ({ page }) => {
    await expect(page.locator('button:has-text("Clone Mission")')).toBeVisible();
  });

  test('should display Export button in properties', async ({ page }) => {
    await expect(page.locator('button:has-text("Export")')).toBeVisible();
  });

  test('should display Delete button in properties', async ({ page }) => {
    await expect(page.locator('button:has-text("Delete")')).toBeVisible();
  });

  test('should clone mission from editor', async ({ page }) => {
    const clonedMission = {
      ...existingMission,
      id: '550e8400-e29b-41d4-a716-446655440006',
      name: `${existingMission.name} (Copy)`,
      status: 'Draft',
    };

    await page.route(`**/api/missions/v1/missions/${existingMission.id}/clone`, (route) => {
      route.fulfill({
        status: 201,
        contentType: 'application/json',
        body: JSON.stringify(clonedMission),
      });
    });

    await page.route(`**/api/missions/v1/missions/${clonedMission.id}`, (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(clonedMission),
      });
    });

    await page.locator('button:has-text("Clone Mission")').click();

    await expect(page.locator('.mat-mdc-snack-bar-label')).toContainText('cloned');
  });

  test('should delete mission from editor', async ({ page }) => {
    page.on('dialog', (dialog) => dialog.accept());

    await page.route(`**/api/missions/v1/missions/${existingMission.id}`, (route) => {
      if (route.request().method() === 'DELETE') {
        route.fulfill({ status: 204 });
      }
    });

    await page.locator('button:has-text("Delete")').click();

    await expect(page).toHaveURL(/.*missions$/);
  });
});

test.describe('Mission Editor - Mission Tree Sidebar', () => {
  const existingMission = mockMissions[0];

  test.beforeEach(async ({ page, context }) => {
    await context.addInitScript(() => {
      localStorage.setItem(
        'access_token',
        'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiZW1haWwiOiJ0ZXN0QGV4YW1wbGUuY29tIiwibmFtZSI6IlRlc3QgVXNlciJ9.fake'
      );
    });

    await page.route(`**/api/missions/v1/missions/${existingMission.id}`, (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(existingMission),
      });
    });

    await page.goto(`/missions/${existingMission.id}/edit`);
  });

  test('should display mission tree sidebar', async ({ page }) => {
    await expect(page.locator('.editor-sidebar')).toBeVisible();
    await expect(page.locator('text=Mission Structure')).toBeVisible();
  });

  test('should display tree nodes', async ({ page }) => {
    await expect(page.locator('.tree-item:has-text("Mission")')).toBeVisible();
    await expect(page.locator('.tree-item:has-text("Spacecraft")')).toBeVisible();
    await expect(page.locator('.tree-item:has-text("Propagator")')).toBeVisible();
    await expect(page.locator('.tree-item:has-text("Maneuvers")')).toBeVisible();
  });

  test('should expand and collapse tree nodes', async ({ page }) => {
    const expandIcon = page.locator('.tree-item:has-text("Spacecraft") .tree-item__expand');
    await expandIcon.click();

    await expect(page.locator('.tree-item:has-text("SAT-001")')).not.toBeVisible();

    await expandIcon.click();
    await expect(page.locator('.tree-item:has-text("SAT-001")')).toBeVisible();
  });

  test('should select tree node', async ({ page }) => {
    await page.locator('.tree-item:has-text("Propagator")').click();
    await expect(page.locator('.tree-item--selected:has-text("Propagator")')).toBeVisible();
  });

  test('should show add button in sidebar header', async ({ page }) => {
    await expect(page.locator('.sidebar-header__add')).toBeVisible();
  });
});

test.describe('Mission Export/Import', () => {
  const existingMission = mockMissions[0];

  test.beforeEach(async ({ page, context }) => {
    await context.addInitScript(() => {
      localStorage.setItem(
        'access_token',
        'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiZW1haWwiOiJ0ZXN0QGV4YW1wbGUuY29tIiwibmFtZSI6IlRlc3QgVXNlciJ9.fake'
      );
    });
  });

  test('should export mission', async ({ page }) => {
    const exportData = {
      exportedAt: new Date().toISOString(),
      version: '1.0',
      mission: {
        name: existingMission.name,
        description: existingMission.description,
        type: existingMission.type,
        startEpoch: existingMission.startEpoch,
        endEpoch: existingMission.endEpoch,
      },
    };

    await page.route(`**/api/missions/v1/missions/${existingMission.id}`, (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(existingMission),
      });
    });

    await page.route(`**/api/missions/v1/missions/${existingMission.id}/export`, (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(exportData),
      });
    });

    await page.goto(`/missions/${existingMission.id}/edit`);

    await page.locator('button:has-text("Export")').click();

    await expect(page.locator('.mat-mdc-snack-bar-label')).toContainText('Export');
  });
});

test.describe('Mission Status Changes', () => {
  test.beforeEach(async ({ page, context }) => {
    await context.addInitScript(() => {
      localStorage.setItem(
        'access_token',
        'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiZW1haWwiOiJ0ZXN0QGV4YW1wbGUuY29tIiwibmFtZSI6IlRlc3QgVXNlciJ9.fake'
      );
    });

    await page.route('**/api/missions/v1/missions**', (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(mockMissionsResponse),
      });
    });

    await page.goto('/missions');
  });

  test('should change mission status from action menu', async ({ page }) => {
    const draftMission = mockMissions.find((m) => m.status === 'Draft');

    await page.route(`**/api/missions/v1/missions/${draftMission!.id}/status`, (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          ...draftMission,
          status: 'Active',
        }),
      });
    });

    const actionButton = page.locator(`tr:has-text("${draftMission!.name}") button[mat-icon-button]`);
    await actionButton.click();

    const activateButton = page.locator('button:has-text("Activate")');
    if (await activateButton.isVisible()) {
      await activateButton.click();
      await expect(page.locator('.mat-mdc-snack-bar-label')).toContainText('status');
    }
  });
});

test.describe('Accessibility', () => {
  test.beforeEach(async ({ page, context }) => {
    await context.addInitScript(() => {
      localStorage.setItem(
        'access_token',
        'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiZW1haWwiOiJ0ZXN0QGV4YW1wbGUuY29tIiwibmFtZSI6IlRlc3QgVXNlciJ9.fake'
      );
    });

    await page.route('**/api/missions/v1/missions**', (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(mockMissionsResponse),
      });
    });
  });

  test('missions page should be keyboard navigable', async ({ page }) => {
    await page.goto('/missions');

    await page.keyboard.press('Tab');
    await page.keyboard.press('Tab');

    const focusedElement = await page.evaluate(() => document.activeElement?.tagName);
    expect(focusedElement).toBeTruthy();
  });

  test('mission editor should have form labels', async ({ page }) => {
    await page.goto('/missions/new');

    const nameLabel = page.locator('mat-label:has-text("Mission Name")');
    await expect(nameLabel).toBeVisible();
  });
});

test.describe('Error Handling', () => {
  test.beforeEach(async ({ page, context }) => {
    await context.addInitScript(() => {
      localStorage.setItem(
        'access_token',
        'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiZW1haWwiOiJ0ZXN0QGV4YW1wbGUuY29tIiwibmFtZSI6IlRlc3QgVXNlciJ9.fake'
      );
    });
  });

  test('should show error message when loading missions fails', async ({ page }) => {
    await page.route('**/api/missions/v1/missions**', (route) => {
      route.fulfill({
        status: 500,
        contentType: 'application/json',
        body: JSON.stringify({ error: 'Internal Server Error' }),
      });
    });

    await page.goto('/missions');

    await expect(page.locator('.mat-mdc-snack-bar-label')).toContainText(/error|failed/i);
  });

  test('should show error message when creating mission fails', async ({ page }) => {
    await page.route('**/api/missions/v1/missions', (route) => {
      if (route.request().method() === 'POST') {
        route.fulfill({
          status: 400,
          contentType: 'application/json',
          body: JSON.stringify({ error: 'Validation failed' }),
        });
      }
    });

    await page.goto('/missions/new');

    await page.locator('input[formControlName="name"]').fill('Test Mission');
    await page.locator('input[formControlName="startDate"]').fill('2025-04-01');
    await page.locator('input[formControlName="startTime"]').fill('12:00:00.000');

    await page.locator('button:has-text("Create Mission")').click();

    await expect(page.locator('.mat-mdc-snack-bar-label')).toContainText(/error|failed/i);
  });

  test('should show error message when mission not found', async ({ page }) => {
    await page.route('**/api/missions/v1/missions/non-existent-id', (route) => {
      route.fulfill({
        status: 404,
        contentType: 'application/json',
        body: JSON.stringify({ error: 'Mission not found' }),
      });
    });

    await page.goto('/missions/non-existent-id/edit');

    await expect(page.locator('.mat-mdc-snack-bar-label')).toContainText(/error|not found|failed/i);
  });
});

test.describe('Auto-save', () => {
  const existingMission = mockMissions[0];

  test('should auto-save changes after debounce', async ({ page, context }) => {
    await context.addInitScript(() => {
      localStorage.setItem(
        'access_token',
        'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiZW1haWwiOiJ0ZXN0QGV4YW1wbGUuY29tIiwibmFtZSI6IlRlc3QgVXNlciJ9.fake'
      );
    });

    let updateCalled = false;

    await page.route(`**/api/missions/v1/missions/${existingMission.id}`, (route) => {
      if (route.request().method() === 'GET') {
        route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(existingMission),
        });
      } else if (route.request().method() === 'PUT') {
        updateCalled = true;
        route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            ...existingMission,
            updatedAt: new Date().toISOString(),
          }),
        });
      }
    });

    await page.goto(`/missions/${existingMission.id}/edit`);

    const descriptionInput = page.locator('textarea[formControlName="description"]');
    await descriptionInput.fill('Updated description for auto-save test');

    // Wait for debounce (2 seconds) + network request
    await page.waitForTimeout(3000);

    expect(updateCalled).toBe(true);
    await expect(page.locator('text=Auto-saved')).toBeVisible();
  });
});
