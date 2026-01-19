import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import {
  BehaviorSubject,
  Observable,
  tap,
  catchError,
  throwError,
  filter,
  take,
  switchMap,
  of,
} from 'rxjs';
import { environment } from '../../../environments/environment';

export interface User {
  id: string;
  email: string;
  name: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  user: User;
}

interface TokenPayload {
  sub: string;
  email: string;
  name?: string;
  exp: number;
  iat: number;
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly currentUser$ = new BehaviorSubject<User | null>(null);
  readonly user$ = this.currentUser$.asObservable();

  private readonly ACCESS_TOKEN_KEY = 'access_token';
  private readonly REFRESH_TOKEN_KEY = 'refresh_token';

  private readonly REFRESH_THRESHOLD_SECONDS = 60;

  private isRefreshing$ = new BehaviorSubject<boolean>(false);
  private refreshResult$ = new BehaviorSubject<AuthResponse | null>(null);

  constructor(private readonly http: HttpClient) {
    this.loadUserFromStorage();
  }

  get isAuthenticated(): boolean {
    return !!this.getAccessToken() && !this.isTokenExpired();
  }

  get currentUser(): User | null {
    return this.currentUser$.value;
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${environment.baseUrl}/api/auth/login`, request)
      .pipe(
        tap((response) => this.handleAuthResponse(response)),
        catchError((error) => {
          return throwError(() => error);
        })
      );
  }

  logout(): void {
    localStorage.removeItem(this.ACCESS_TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    this.currentUser$.next(null);
    this.isRefreshing$.next(false);
    this.refreshResult$.next(null);
  }

  refreshToken(): Observable<AuthResponse> {
    if (this.isRefreshing$.value) {
      return this.refreshResult$.pipe(
        filter((result): result is AuthResponse => result !== null),
        take(1)
      );
    }

    this.isRefreshing$.next(true);
    this.refreshResult$.next(null);

    const refreshToken = this.getRefreshToken();

    if (!refreshToken) {
      this.isRefreshing$.next(false);
      this.logout();
      return throwError(() => new Error('No refresh token available'));
    }

    return this.http
      .post<AuthResponse>(`${environment.baseUrl}/api/auth/refresh`, { refreshToken })
      .pipe(
        tap((response) => {
          this.handleAuthResponse(response);
          this.refreshResult$.next(response);
          this.isRefreshing$.next(false);
        }),
        catchError((error) => {
          this.isRefreshing$.next(false);
          this.refreshResult$.next(null);
          this.logout();
          return throwError(() => error);
        })
      );
  }

  refreshTokenIfNeeded(): Observable<AuthResponse | null> {
    if (!this.getAccessToken()) {
      return of(null);
    }

    if (this.shouldRefreshToken()) {
      return this.refreshToken();
    }

    return of(null);
  }

  getAccessToken(): string | null {
    return localStorage.getItem(this.ACCESS_TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.REFRESH_TOKEN_KEY);
  }

  isTokenExpired(): boolean {
    const token = this.getAccessToken();
    if (!token) return true;

    const payload = this.decodeToken(token);
    if (!payload) return true;

    const now = Math.floor(Date.now() / 1000);
    return payload.exp <= now;
  }

  shouldRefreshToken(): boolean {
    const token = this.getAccessToken();
    if (!token) return false;

    const payload = this.decodeToken(token);
    if (!payload) return false;

    const now = Math.floor(Date.now() / 1000);
    const timeUntilExpiry = payload.exp - now;

    return timeUntilExpiry > 0 && timeUntilExpiry <= this.REFRESH_THRESHOLD_SECONDS;
  }

  getTokenExpirationTime(): Date | null {
    const token = this.getAccessToken();
    if (!token) return null;

    const payload = this.decodeToken(token);
    if (!payload) return null;

    return new Date(payload.exp * 1000);
  }

  getTimeUntilExpiry(): number | null {
    const token = this.getAccessToken();
    if (!token) return null;

    const payload = this.decodeToken(token);
    if (!payload) return null;

    const now = Math.floor(Date.now() / 1000);
    return Math.max(0, payload.exp - now);
  }

  get isRefreshing(): boolean {
    return this.isRefreshing$.value;
  }

  waitForRefresh(): Observable<AuthResponse> {
    return this.refreshResult$.pipe(
      filter((result): result is AuthResponse => result !== null),
      take(1)
    );
  }

  private decodeToken(token: string): TokenPayload | null {
    try {
      const parts = token.split('.');
      if (parts.length !== 3) return null;

      const payload = JSON.parse(atob(parts[1]));
      return payload as TokenPayload;
    } catch {
      return null;
    }
  }

  private handleAuthResponse(response: AuthResponse): void {
    localStorage.setItem(this.ACCESS_TOKEN_KEY, response.accessToken);
    localStorage.setItem(this.REFRESH_TOKEN_KEY, response.refreshToken);
    this.currentUser$.next(response.user);
  }

  private loadUserFromStorage(): void {
    const token = this.getAccessToken();
    if (token && !this.isTokenExpired()) {
      const payload = this.decodeToken(token);
      if (payload) {
        this.currentUser$.next({
          id: payload.sub,
          email: payload.email,
          name: payload.name || payload.email,
        });
      }
    } else if (token) {
      this.logout();
    }
  }
}
