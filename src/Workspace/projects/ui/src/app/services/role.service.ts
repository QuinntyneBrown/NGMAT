import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import {
  Role,
  RoleDetail,
  UserSummary,
  CreateRoleRequest,
  UpdateRoleRequest,
  UpdateRolePermissionsRequest
} from '../models/role.model';

@Injectable({
  providedIn: 'root'
})
export class RoleService {
  private readonly apiUrl = `${environment.baseUrl}/api/roles`;

  constructor(private http: HttpClient) {}

  getRoles(): Observable<Role[]> {
    return this.http.get<Role[]>(this.apiUrl).pipe(
      map(roles => roles.map(this.mapRoleDates))
    );
  }

  getRole(id: string): Observable<RoleDetail> {
    return this.http.get<RoleDetail>(`${this.apiUrl}/${id}`).pipe(
      map(this.mapRoleDetailDates)
    );
  }

  createRole(request: CreateRoleRequest): Observable<Role> {
    return this.http.post<Role>(this.apiUrl, request).pipe(
      map(this.mapRoleDates)
    );
  }

  updateRole(id: string, request: UpdateRoleRequest): Observable<Role> {
    return this.http.put<Role>(`${this.apiUrl}/${id}`, request).pipe(
      map(this.mapRoleDates)
    );
  }

  deleteRole(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  updateRolePermissions(id: string, request: UpdateRolePermissionsRequest): Observable<RoleDetail> {
    return this.http.put<RoleDetail>(`${this.apiUrl}/${id}/permissions`, request).pipe(
      map(this.mapRoleDetailDates)
    );
  }

  getRoleUsers(id: string): Observable<UserSummary[]> {
    return this.http.get<UserSummary[]>(`${this.apiUrl}/${id}/users`);
  }

  private mapRoleDates = (role: Role): Role => ({
    ...role,
    createdAt: new Date(role.createdAt)
  });

  private mapRoleDetailDates = (role: RoleDetail): RoleDetail => ({
    ...role,
    createdAt: new Date(role.createdAt)
  });
}
