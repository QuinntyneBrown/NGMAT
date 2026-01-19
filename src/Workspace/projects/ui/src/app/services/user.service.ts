import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import {
  User,
  UserDetail,
  UserListResponse,
  UserStats,
  CreateUserRequest,
  UpdateUserRequest,
  UpdateUserRolesRequest,
  ResetPasswordResponse,
  UserStatus
} from '../models/user.model';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private readonly apiUrl = `${environment.baseUrl}/api/users`;

  constructor(private http: HttpClient) {}

  getUsers(
    skip: number = 0,
    take: number = 10,
    search?: string,
    status?: UserStatus,
    role?: string
  ): Observable<UserListResponse> {
    let params = new HttpParams()
      .set('skip', skip.toString())
      .set('take', take.toString());

    if (search) {
      params = params.set('search', search);
    }
    if (status) {
      params = params.set('status', status);
    }
    if (role) {
      params = params.set('role', role);
    }

    return this.http.get<UserListResponse>(this.apiUrl, { params }).pipe(
      map(response => ({
        ...response,
        items: response.items.map(this.mapUserDates)
      }))
    );
  }

  getUser(id: string): Observable<UserDetail> {
    return this.http.get<UserDetail>(`${this.apiUrl}/${id}`).pipe(
      map(this.mapUserDetailDates)
    );
  }

  getCurrentUser(): Observable<User> {
    return this.http.get<User>(`${this.apiUrl}/me`).pipe(
      map(this.mapUserDates)
    );
  }

  getUserStats(): Observable<UserStats> {
    return this.http.get<UserStats>(`${this.apiUrl}/stats`);
  }

  createUser(request: CreateUserRequest): Observable<User> {
    return this.http.post<User>(this.apiUrl, request).pipe(
      map(this.mapUserDates)
    );
  }

  updateUser(id: string, request: UpdateUserRequest): Observable<User> {
    return this.http.put<User>(`${this.apiUrl}/${id}`, request).pipe(
      map(this.mapUserDates)
    );
  }

  deleteUser(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  activateUser(id: string): Observable<User> {
    return this.http.post<User>(`${this.apiUrl}/${id}/activate`, {}).pipe(
      map(this.mapUserDates)
    );
  }

  deactivateUser(id: string): Observable<User> {
    return this.http.post<User>(`${this.apiUrl}/${id}/deactivate`, {}).pipe(
      map(this.mapUserDates)
    );
  }

  unlockUser(id: string): Observable<User> {
    return this.http.post<User>(`${this.apiUrl}/${id}/unlock`, {}).pipe(
      map(this.mapUserDates)
    );
  }

  resetPassword(id: string): Observable<ResetPasswordResponse> {
    return this.http.post<ResetPasswordResponse>(`${this.apiUrl}/${id}/reset-password`, {});
  }

  updateUserRoles(id: string, request: UpdateUserRolesRequest): Observable<User> {
    return this.http.put<User>(`${this.apiUrl}/${id}/roles`, request).pipe(
      map(this.mapUserDates)
    );
  }

  private mapUserDates = (user: User): User => ({
    ...user,
    createdAt: new Date(user.createdAt),
    lastLoginAt: user.lastLoginAt ? new Date(user.lastLoginAt) : undefined
  });

  private mapUserDetailDates = (user: UserDetail): UserDetail => ({
    ...user,
    createdAt: new Date(user.createdAt),
    lastLoginAt: user.lastLoginAt ? new Date(user.lastLoginAt) : undefined,
    lockoutEndAt: user.lockoutEndAt ? new Date(user.lockoutEndAt) : undefined,
    roles: user.roles.map(r => ({
      ...r,
      assignedAt: new Date(r.assignedAt)
    }))
  });
}
