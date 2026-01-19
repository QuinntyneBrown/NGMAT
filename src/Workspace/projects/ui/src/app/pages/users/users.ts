import { Component, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatMenuModule } from '@angular/material/menu';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { BehaviorSubject, Subject, combineLatest, of } from 'rxjs';
import { switchMap, takeUntil, map, startWith, catchError } from 'rxjs/operators';

import { UserService } from '../../services/user.service';
import { RoleService } from '../../services/role.service';
import { User, UserStats, UserStatus } from '../../models/user.model';
import { Role } from '../../models/role.model';

interface UsersViewModel {
  users: User[];
  stats: UserStats | null;
  roles: Role[];
  loading: boolean;
  totalCount: number;
  pageSize: number;
  pageIndex: number;
  searchTerm: string;
  statusFilter: UserStatus | null;
  roleFilter: string | null;
}

@Component({
  selector: 'app-users',
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatTableModule,
    MatPaginatorModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatMenuModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    MatSnackBarModule,
  ],
  templateUrl: './users.html',
  styleUrl: './users.scss',
})
export class Users implements OnDestroy {
  protected readonly displayedColumns = ['user', 'roles', 'status', 'lastLogin', 'mfa', 'createdAt', 'actions'];
  protected readonly statusOptions: UserStatus[] = ['active', 'inactive', 'locked'];

  private readonly pageIndex$ = new BehaviorSubject<number>(0);
  private readonly pageSize$ = new BehaviorSubject<number>(10);
  private readonly searchTerm$ = new BehaviorSubject<string>('');
  private readonly statusFilter$ = new BehaviorSubject<UserStatus | null>(null);
  private readonly roleFilter$ = new BehaviorSubject<string | null>(null);
  private readonly refresh$ = new BehaviorSubject<void>(undefined);
  private readonly destroy$ = new Subject<void>();

  protected searchTermValue = '';
  protected statusFilterValue: UserStatus | null = null;
  protected roleFilterValue: string | null = null;

  protected readonly viewModel$ = combineLatest([
    this.pageIndex$,
    this.pageSize$,
    this.searchTerm$,
    this.statusFilter$,
    this.roleFilter$,
    this.refresh$,
  ]).pipe(
    switchMap(([pageIndex, pageSize, searchTerm, statusFilter, roleFilter]) =>
      combineLatest([
        this.userService.getUsers(pageIndex * pageSize, pageSize, searchTerm || undefined, statusFilter || undefined, roleFilter || undefined),
        this.userService.getUserStats(),
        this.roleService.getRoles()
      ]).pipe(
        map(([usersResponse, stats, roles]) => ({
          users: usersResponse.items,
          stats,
          roles,
          loading: false,
          totalCount: usersResponse.totalCount,
          pageSize,
          pageIndex,
          searchTerm,
          statusFilter,
          roleFilter,
        })),
        startWith({
          users: [] as User[],
          stats: null as UserStats | null,
          roles: [] as Role[],
          loading: true,
          totalCount: 0,
          pageSize,
          pageIndex,
          searchTerm,
          statusFilter,
          roleFilter,
        }),
        catchError(() => {
          this.snackBar.open('Failed to load users', 'Close', { duration: 3000 });
          return of({
            users: [] as User[],
            stats: null as UserStats | null,
            roles: [] as Role[],
            loading: false,
            totalCount: 0,
            pageSize,
            pageIndex,
            searchTerm,
            statusFilter,
            roleFilter,
          });
        })
      )
    ),
    takeUntil(this.destroy$)
  );

  constructor(
    private userService: UserService,
    private roleService: RoleService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  protected onPageChange(event: PageEvent): void {
    this.pageIndex$.next(event.pageIndex);
    this.pageSize$.next(event.pageSize);
  }

  protected onSearch(): void {
    this.searchTerm$.next(this.searchTermValue);
    this.pageIndex$.next(0);
  }

  protected onStatusFilterChange(status: UserStatus | null): void {
    this.statusFilterValue = status;
    this.statusFilter$.next(status);
    this.pageIndex$.next(0);
  }

  protected onRoleFilterChange(role: string | null): void {
    this.roleFilterValue = role;
    this.roleFilter$.next(role);
    this.pageIndex$.next(0);
  }

  protected clearFilters(): void {
    this.searchTermValue = '';
    this.statusFilterValue = null;
    this.roleFilterValue = null;
    this.searchTerm$.next('');
    this.statusFilter$.next(null);
    this.roleFilter$.next(null);
    this.pageIndex$.next(0);
  }

  protected clearSearch(): void {
    this.searchTermValue = '';
    this.searchTerm$.next('');
    this.pageIndex$.next(0);
  }

  protected createUser(): void {
    this.router.navigate(['/users/new']);
  }

  protected editUser(user: User): void {
    this.router.navigate(['/users', user.id]);
  }

  protected deleteUser(user: User): void {
    if (confirm(`Are you sure you want to delete "${user.displayName || user.email}"?`)) {
      this.userService.deleteUser(user.id).pipe(takeUntil(this.destroy$)).subscribe({
        next: () => {
          this.snackBar.open('User deleted', 'Close', { duration: 3000 });
          this.refresh$.next();
        },
        error: () => {
          this.snackBar.open('Failed to delete user', 'Close', { duration: 3000 });
        },
      });
    }
  }

  protected toggleUserStatus(user: User): void {
    const action = user.isActive ? this.userService.deactivateUser(user.id) : this.userService.activateUser(user.id);
    action.pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.snackBar.open(`User ${user.isActive ? 'deactivated' : 'activated'}`, 'Close', { duration: 3000 });
        this.refresh$.next();
      },
      error: () => {
        this.snackBar.open('Failed to update user status', 'Close', { duration: 3000 });
      },
    });
  }

  protected unlockUser(user: User): void {
    this.userService.unlockUser(user.id).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.snackBar.open('User unlocked', 'Close', { duration: 3000 });
        this.refresh$.next();
      },
      error: () => {
        this.snackBar.open('Failed to unlock user', 'Close', { duration: 3000 });
      },
    });
  }

  protected getStatusClass(user: User): string {
    if (user.isLockedOut) return 'status-chip--locked';
    return user.isActive ? 'status-chip--active' : 'status-chip--inactive';
  }

  protected getStatusLabel(user: User): string {
    if (user.isLockedOut) return 'Locked';
    return user.isActive ? 'Active' : 'Inactive';
  }

  protected getInitials(user: User): string {
    if (user.displayName) {
      return user.displayName
        .split(' ')
        .map(n => n[0])
        .join('')
        .toUpperCase()
        .slice(0, 2);
    }
    return user.email[0].toUpperCase();
  }

  protected formatDate(date: Date | undefined): string {
    if (!date) return '-';
    return new Date(date).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  }

  protected formatDateTime(date: Date | undefined): string {
    if (!date) return 'Never';
    return new Date(date).toLocaleString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  }
}
