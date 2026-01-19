import { test, expect, Page } from '@playwright/test';

function createToken(payload: object, expiresInSeconds: number): string {
  const header = { alg: 'HS256', typ: 'JWT' };
  const now = Math.floor(Date.now() / 1000);
  const fullPayload = {
    ...payload,
    iat: now,
    exp: now + expiresInSeconds,
  };

  const base64Header = btoa(JSON.stringify(header));
  const base64Payload = btoa(JSON.stringify(fullPayload));
  const signature = 'fake-signature';

  return `${base64Header}.${base64Payload}.${signature}`;
}

function createValidToken(): string {
  return createToken(
    { sub: '1', email: 'test@example.com', name: 'Test User' },
    3600
  );
}

function createExpiredToken(): string {
  return createToken(
    { sub: '1', email: 'test@example.com', name: 'Test User' },
    -10
  );
}

function createExpiringToken(secondsUntilExpiry: number): string {
  return createToken(
    { sub: '1', email: 'test@example.com', name: 'Test User' },
    secondsUntilExpiry
  );
}

function createRefreshResponse(accessTokenExpiresIn: number = 3600) {
  return {
    accessToken: createToken(
      { sub: '1', email: 'test@example.com', name: 'Test User' },
      accessTokenExpiresIn
    ),
    refreshToken: 'new-refresh-token',
    user: {
      id: '1',
      email: 'test@example.com',
      name: 'Test User',
    },
  };
}

test.describe('Refresh Token Functionality', () => {
  test.describe('Expired Token Handling', () => {
    test('should refresh token when access token is expired and make API call', async ({ page, context }) => {
      const expiredToken = createExpiredToken();
      const refreshResponse = createRefreshResponse();

      await context.addInitScript((token) => {
        localStorage.setItem('access_token', token);
        localStorage.setItem('refresh_token', 'valid-refresh-token');
      }, expiredToken);

      let refreshCalled = false;
      let apiCallMade = false;

      await page.route('**/api/auth/refresh', async (route) => {
        refreshCalled = true;
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(refreshResponse),
        });
      });

      await page.route('**/api/missions**', async (route) => {
        apiCallMade = true;
        const headers = route.request().headers();
        expect(headers['authorization']).toContain('Bearer');
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({ items: [], totalCount: 0 }),
        });
      });

      await page.goto('/missions');

      await page.waitForTimeout(1000);

      expect(refreshCalled).toBe(true);
    });

    test('should redirect to login when refresh token is invalid', async ({ page, context }) => {
      const expiredToken = createExpiredToken();

      await context.addInitScript((token) => {
        localStorage.setItem('access_token', token);
        localStorage.setItem('refresh_token', 'invalid-refresh-token');
      }, expiredToken);

      await page.route('**/api/auth/refresh', async (route) => {
        await route.fulfill({
          status: 401,
          contentType: 'application/json',
          body: JSON.stringify({ message: 'Invalid refresh token' }),
        });
      });

      await page.goto('/missions');

      await expect(page).toHaveURL(/.*login/);
    });

    test('should clear tokens and redirect when refresh fails', async ({ page, context }) => {
      const expiredToken = createExpiredToken();

      await context.addInitScript((token) => {
        localStorage.setItem('access_token', token);
        localStorage.setItem('refresh_token', 'expired-refresh-token');
      }, expiredToken);

      await page.route('**/api/auth/refresh', async (route) => {
        await route.fulfill({
          status: 403,
          contentType: 'application/json',
          body: JSON.stringify({ message: 'Refresh token expired' }),
        });
      });

      await page.goto('/missions');

      await expect(page).toHaveURL(/.*login/);

      const accessToken = await page.evaluate(() => localStorage.getItem('access_token'));
      const refreshToken = await page.evaluate(() => localStorage.getItem('refresh_token'));

      expect(accessToken).toBeNull();
      expect(refreshToken).toBeNull();
    });
  });

  test.describe('401 Response Handling', () => {
    test('should refresh token on 401 response and retry request', async ({ page, context }) => {
      const validToken = createValidToken();
      const refreshResponse = createRefreshResponse();

      await context.addInitScript((token) => {
        localStorage.setItem('access_token', token);
        localStorage.setItem('refresh_token', 'valid-refresh-token');
      }, validToken);

      let apiCallCount = 0;
      let refreshCalled = false;

      await page.route('**/api/auth/refresh', async (route) => {
        refreshCalled = true;
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(refreshResponse),
        });
      });

      await page.route('**/api/missions**', async (route) => {
        apiCallCount++;
        if (apiCallCount === 1) {
          await route.fulfill({
            status: 401,
            contentType: 'application/json',
            body: JSON.stringify({ message: 'Token expired' }),
          });
        } else {
          await route.fulfill({
            status: 200,
            contentType: 'application/json',
            body: JSON.stringify({ items: [], totalCount: 0 }),
          });
        }
      });

      await page.goto('/missions');

      await page.waitForTimeout(1000);

      expect(refreshCalled).toBe(true);
      expect(apiCallCount).toBe(2);
    });

    test('should redirect to login on 401 when refresh also fails', async ({ page, context }) => {
      const validToken = createValidToken();

      await context.addInitScript((token) => {
        localStorage.setItem('access_token', token);
        localStorage.setItem('refresh_token', 'invalid-refresh-token');
      }, validToken);

      await page.route('**/api/auth/refresh', async (route) => {
        await route.fulfill({
          status: 401,
          contentType: 'application/json',
          body: JSON.stringify({ message: 'Invalid refresh token' }),
        });
      });

      await page.route('**/api/missions**', async (route) => {
        await route.fulfill({
          status: 401,
          contentType: 'application/json',
          body: JSON.stringify({ message: 'Token expired' }),
        });
      });

      await page.goto('/missions');

      await expect(page).toHaveURL(/.*login/);
    });
  });

  test.describe('Proactive Token Refresh', () => {
    test('should proactively refresh token when close to expiry', async ({ page, context }) => {
      const expiringToken = createExpiringToken(30);
      const refreshResponse = createRefreshResponse();

      await context.addInitScript((token) => {
        localStorage.setItem('access_token', token);
        localStorage.setItem('refresh_token', 'valid-refresh-token');
      }, expiringToken);

      let refreshCalled = false;

      await page.route('**/api/auth/refresh', async (route) => {
        refreshCalled = true;
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(refreshResponse),
        });
      });

      await page.route('**/api/missions**', async (route) => {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({ items: [], totalCount: 0 }),
        });
      });

      await page.goto('/missions');

      await page.waitForTimeout(1000);

      expect(refreshCalled).toBe(true);
    });
  });

  test.describe('Concurrent Requests During Refresh', () => {
    test('should handle multiple concurrent requests during token refresh', async ({ page, context }) => {
      const expiredToken = createExpiredToken();
      const refreshResponse = createRefreshResponse();

      await context.addInitScript((token) => {
        localStorage.setItem('access_token', token);
        localStorage.setItem('refresh_token', 'valid-refresh-token');
      }, expiredToken);

      let refreshCallCount = 0;
      let apiCallCount = 0;

      await page.route('**/api/auth/refresh', async (route) => {
        refreshCallCount++;
        await new Promise((resolve) => setTimeout(resolve, 500));
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(refreshResponse),
        });
      });

      await page.route('**/api/**', async (route) => {
        if (!route.request().url().includes('/auth/')) {
          apiCallCount++;
          await route.fulfill({
            status: 200,
            contentType: 'application/json',
            body: JSON.stringify({ items: [], totalCount: 0 }),
          });
        }
      });

      await page.goto('/missions');

      await page.waitForTimeout(2000);

      expect(refreshCallCount).toBe(1);
    });

    test('should queue requests while refresh is in progress', async ({ page, context }) => {
      const expiredToken = createExpiredToken();
      const refreshResponse = createRefreshResponse();

      await context.addInitScript((token) => {
        localStorage.setItem('access_token', token);
        localStorage.setItem('refresh_token', 'valid-refresh-token');
      }, expiredToken);

      const requestTimings: number[] = [];
      let refreshCompleteTime = 0;

      await page.route('**/api/auth/refresh', async (route) => {
        await new Promise((resolve) => setTimeout(resolve, 300));
        refreshCompleteTime = Date.now();
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(refreshResponse),
        });
      });

      await page.route('**/api/missions**', async (route) => {
        requestTimings.push(Date.now());
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({ items: [], totalCount: 0 }),
        });
      });

      await page.goto('/missions');

      await page.waitForTimeout(1000);

      for (const timing of requestTimings) {
        expect(timing).toBeGreaterThanOrEqual(refreshCompleteTime - 50);
      }
    });
  });

  test.describe('Token Storage', () => {
    test('should update tokens in localStorage after successful refresh', async ({ page, context }) => {
      const expiredToken = createExpiredToken();
      const refreshResponse = createRefreshResponse();
      const newAccessToken = refreshResponse.accessToken;

      await context.addInitScript((token) => {
        localStorage.setItem('access_token', token);
        localStorage.setItem('refresh_token', 'old-refresh-token');
      }, expiredToken);

      await page.route('**/api/auth/refresh', async (route) => {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(refreshResponse),
        });
      });

      await page.route('**/api/missions**', async (route) => {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({ items: [], totalCount: 0 }),
        });
      });

      await page.goto('/missions');

      await page.waitForTimeout(1000);

      const storedAccessToken = await page.evaluate(() => localStorage.getItem('access_token'));
      const storedRefreshToken = await page.evaluate(() => localStorage.getItem('refresh_token'));

      expect(storedAccessToken).toBe(newAccessToken);
      expect(storedRefreshToken).toBe('new-refresh-token');
    });
  });

  test.describe('No Refresh Token Available', () => {
    test('should redirect to login when no refresh token is available', async ({ page, context }) => {
      const expiredToken = createExpiredToken();

      await context.addInitScript((token) => {
        localStorage.setItem('access_token', token);
      }, expiredToken);

      await page.goto('/missions');

      await expect(page).toHaveURL(/.*login/);
    });

    test('should redirect to login on 401 without refresh token', async ({ page, context }) => {
      const validToken = createValidToken();

      await context.addInitScript((token) => {
        localStorage.setItem('access_token', token);
      }, validToken);

      await page.route('**/api/missions**', async (route) => {
        await route.fulfill({
          status: 401,
          contentType: 'application/json',
          body: JSON.stringify({ message: 'Token expired' }),
        });
      });

      await page.goto('/missions');

      await expect(page).toHaveURL(/.*login/);
    });
  });

  test.describe('Session Persistence After Refresh', () => {
    test('should maintain user session after token refresh', async ({ page, context }) => {
      const expiredToken = createExpiredToken();
      const refreshResponse = createRefreshResponse();

      await context.addInitScript((token) => {
        localStorage.setItem('access_token', token);
        localStorage.setItem('refresh_token', 'valid-refresh-token');
      }, expiredToken);

      await page.route('**/api/auth/refresh', async (route) => {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(refreshResponse),
        });
      });

      await page.route('**/api/missions**', async (route) => {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            items: [
              {
                id: '1',
                name: 'Test Mission',
                type: 'LEO',
                status: 'Active',
              },
            ],
            totalCount: 1,
          }),
        });
      });

      await page.goto('/missions');

      await expect(page).toHaveURL(/.*missions/);

      await expect(page.locator('text=Test Mission')).toBeVisible({ timeout: 5000 });
    });

    test('should use new token for subsequent API calls after refresh', async ({ page, context }) => {
      const expiredToken = createExpiredToken();
      const refreshResponse = createRefreshResponse();
      const newToken = refreshResponse.accessToken;

      await context.addInitScript((token) => {
        localStorage.setItem('access_token', token);
        localStorage.setItem('refresh_token', 'valid-refresh-token');
      }, expiredToken);

      await page.route('**/api/auth/refresh', async (route) => {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(refreshResponse),
        });
      });

      const authHeaders: string[] = [];

      await page.route('**/api/missions**', async (route) => {
        const headers = route.request().headers();
        if (headers['authorization']) {
          authHeaders.push(headers['authorization']);
        }
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({ items: [], totalCount: 0 }),
        });
      });

      await page.goto('/missions');

      await page.waitForTimeout(1000);

      const lastHeader = authHeaders[authHeaders.length - 1];
      expect(lastHeader).toBe(`Bearer ${newToken}`);
    });
  });
});

test.describe('Network Error Handling', () => {
  test('should handle network error during refresh gracefully', async ({ page, context }) => {
    const expiredToken = createExpiredToken();

    await context.addInitScript((token) => {
      localStorage.setItem('access_token', token);
      localStorage.setItem('refresh_token', 'valid-refresh-token');
    }, expiredToken);

    await page.route('**/api/auth/refresh', async (route) => {
      await route.abort('failed');
    });

    await page.goto('/missions');

    await expect(page).toHaveURL(/.*login/);
  });
});
