import { HttpInterceptorFn, HttpErrorResponse, HttpRequest, HttpHandlerFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError, Observable } from 'rxjs';
import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (isAuthEndpoint(req.url)) {
    return next(req);
  }

  if (authService.isTokenExpired() && authService.getRefreshToken()) {
    return handleTokenRefresh(req, next, authService, router);
  }

  if (authService.shouldRefreshToken()) {
    return handleProactiveRefresh(req, next, authService, router);
  }

  return executeRequest(req, next, authService, router);
};

function isAuthEndpoint(url: string): boolean {
  return url.includes('/auth/login') || url.includes('/auth/refresh');
}

function addAuthHeader(req: HttpRequest<unknown>, token: string | null): HttpRequest<unknown> {
  if (!token) {
    return req;
  }
  return req.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`,
    },
  });
}

function handleTokenRefresh(
  req: HttpRequest<unknown>,
  next: HttpHandlerFn,
  authService: AuthService,
  router: Router
): Observable<any> {
  if (authService.isRefreshing) {
    return authService.waitForRefresh().pipe(
      switchMap(() => {
        const token = authService.getAccessToken();
        return next(addAuthHeader(req, token));
      }),
      catchError((error) => {
        router.navigate(['/login']);
        return throwError(() => error);
      })
    );
  }

  return authService.refreshToken().pipe(
    switchMap(() => {
      const token = authService.getAccessToken();
      return next(addAuthHeader(req, token));
    }),
    catchError((error) => {
      router.navigate(['/login']);
      return throwError(() => error);
    })
  );
}

function handleProactiveRefresh(
  req: HttpRequest<unknown>,
  next: HttpHandlerFn,
  authService: AuthService,
  router: Router
): Observable<any> {
  const currentToken = authService.getAccessToken();
  const authReq = addAuthHeader(req, currentToken);

  if (authService.isRefreshing) {
    return next(authReq);
  }

  authService.refreshToken().subscribe({
    error: () => {},
  });

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        return handle401Error(req, next, authService, router);
      }
      return throwError(() => error);
    })
  );
}

function executeRequest(
  req: HttpRequest<unknown>,
  next: HttpHandlerFn,
  authService: AuthService,
  router: Router
): Observable<any> {
  const token = authService.getAccessToken();
  const authReq = addAuthHeader(req, token);

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        return handle401Error(req, next, authService, router);
      }
      return throwError(() => error);
    })
  );
}

function handle401Error(
  req: HttpRequest<unknown>,
  next: HttpHandlerFn,
  authService: AuthService,
  router: Router
): Observable<any> {
  if (authService.isRefreshing) {
    return authService.waitForRefresh().pipe(
      switchMap(() => {
        const token = authService.getAccessToken();
        return next(addAuthHeader(req, token));
      }),
      catchError((error) => {
        router.navigate(['/login']);
        return throwError(() => error);
      })
    );
  }

  return authService.refreshToken().pipe(
    switchMap(() => {
      const token = authService.getAccessToken();
      return next(addAuthHeader(req, token));
    }),
    catchError((error) => {
      router.navigate(['/login']);
      return throwError(() => error);
    })
  );
}
