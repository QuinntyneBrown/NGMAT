import { Component, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatMenuModule } from '@angular/material/menu';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { BehaviorSubject, Subject, of } from 'rxjs';
import { switchMap, takeUntil, map, startWith, catchError } from 'rxjs/operators';

import { RoleService } from '../../services/role.service';
import { Role } from '../../models/role.model';

interface RolesViewModel {
  roles: Role[];
  loading: boolean;
}

@Component({
  selector: 'app-roles',
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatChipsModule,
    MatMenuModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
  ],
  templateUrl: './roles.html',
  styleUrl: './roles.scss',
})
export class Roles implements OnDestroy {
  private readonly refresh$ = new BehaviorSubject<void>(undefined);
  private readonly destroy$ = new Subject<void>();

  protected readonly viewModel$ = this.refresh$.pipe(
    switchMap(() =>
      this.roleService.getRoles().pipe(
        map((roles) => ({
          roles,
          loading: false,
        })),
        startWith({
          roles: [] as Role[],
          loading: true,
        }),
        catchError(() => {
          this.snackBar.open('Failed to load roles', 'Close', { duration: 3000 });
          return of({
            roles: [] as Role[],
            loading: false,
          });
        })
      )
    ),
    takeUntil(this.destroy$)
  );

  constructor(
    private roleService: RoleService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  protected createRole(): void {
    this.router.navigate(['/roles/new']);
  }

  protected viewRole(role: Role): void {
    this.router.navigate(['/roles', role.id]);
  }

  protected editRole(role: Role): void {
    this.router.navigate(['/roles', role.id]);
  }

  protected deleteRole(role: Role): void {
    if (role.isSystem) {
      this.snackBar.open('System roles cannot be deleted', 'Close', { duration: 3000 });
      return;
    }

    if (role.userCount > 0) {
      this.snackBar.open('Cannot delete role with assigned users', 'Close', { duration: 3000 });
      return;
    }

    if (confirm(`Are you sure you want to delete the "${role.name}" role?`)) {
      this.roleService.deleteRole(role.id).pipe(takeUntil(this.destroy$)).subscribe({
        next: () => {
          this.snackBar.open('Role deleted', 'Close', { duration: 3000 });
          this.refresh$.next();
        },
        error: () => {
          this.snackBar.open('Failed to delete role', 'Close', { duration: 3000 });
        },
      });
    }
  }

  protected getRoleIcon(role: Role): string {
    const iconMap: Record<string, string> = {
      Admin: 'admin_panel_settings',
      User: 'person',
      ReadOnly: 'visibility',
    };
    return iconMap[role.name] || 'shield';
  }

  protected getSystemRoles(roles: Role[]): Role[] {
    return roles.filter(r => r.isSystem);
  }

  protected getCustomRoles(roles: Role[]): Role[] {
    return roles.filter(r => !r.isSystem);
  }
}
