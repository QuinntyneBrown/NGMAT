import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { Permission, PermissionGroup } from '../models/permission.model';

@Injectable({
  providedIn: 'root'
})
export class PermissionService {
  private readonly apiUrl = `${environment.baseUrl}/api/permissions`;

  private readonly resourceIcons: Record<string, string> = {
    missions: 'rocket_launch',
    spacecraft: 'satellite',
    propagation: 'timeline',
    users: 'people',
    roles: 'admin_panel_settings',
    settings: 'settings'
  };

  constructor(private http: HttpClient) {}

  getPermissions(): Observable<Permission[]> {
    return this.http.get<Permission[]>(this.apiUrl);
  }

  getPermission(id: string): Observable<Permission> {
    return this.http.get<Permission>(`${this.apiUrl}/${id}`);
  }

  getResources(): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/resources`);
  }

  getPermissionsByResource(resource: string): Observable<Permission[]> {
    return this.http.get<Permission[]>(`${this.apiUrl}/resources/${resource}`);
  }

  getPermissionGroups(): Observable<PermissionGroup[]> {
    return this.getPermissions().pipe(
      map(permissions => {
        const grouped = permissions.reduce((acc, permission) => {
          if (!acc[permission.resource]) {
            acc[permission.resource] = [];
          }
          acc[permission.resource].push(permission);
          return acc;
        }, {} as Record<string, Permission[]>);

        return Object.entries(grouped).map(([resource, perms]) => ({
          resource,
          icon: this.resourceIcons[resource] || 'security',
          permissions: perms
        }));
      })
    );
  }
}
