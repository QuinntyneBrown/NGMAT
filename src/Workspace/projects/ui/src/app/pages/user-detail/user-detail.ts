import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { BehaviorSubject, Subject, combineLatest, of } from 'rxjs';
import { switchMap, takeUntil, map, startWith, catchError, tap } from 'rxjs/operators';

import { UserService } from '../../services/user.service';
import { RoleService } from '../../services/role.service';
import { UserDetail } from '../../models/user.model';
import { Role } from '../../models/role.model';

interface UserDetailViewModel {
  user: UserDetail | null;
  roles: Role[];
  loading: boolean;
  saving: boolean;
  isNew: boolean;
}

@Component({
  selector: 'app-user-detail',
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    ReactiveFormsModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatCheckboxModule,
    MatSlideToggleModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDialogModule,
    MatChipsModule,
    MatDividerModule,
  ],
  templateUrl: './user-detail.html',
  styleUrl: './user-detail.scss',
})
export class UserDetail implements OnInit, OnDestroy {
  protected userForm!: FormGroup;
  protected selectedRoleIds: string[] = [];

  private readonly refresh$ = new BehaviorSubject<void>(undefined);
  private readonly saving$ = new BehaviorSubject<boolean>(false);
  private readonly destroy$ = new Subject<void>();
  private userId: string | null = null;

  protected readonly viewModel$ = this.refresh$.pipe(
    switchMap(() => {
      const id = this.route.snapshot.paramMap.get('id');
      this.userId = id;
      const isNew = id === 'new';

      return combineLatest([
        isNew ? of(null) : this.userService.getUser(id!),
        this.roleService.getRoles(),
        this.saving$
      ]).pipe(
        map(([user, roles, saving]) => ({
          user,
          roles,
          loading: false,
          saving,
          isNew
        })),
        tap(({ user, isNew }) => {
          if (!isNew && user) {
            this.patchForm(user);
            this.selectedRoleIds = user.roles.map(r => r.id);
          }
        }),
        startWith({
          user: null as UserDetail | null,
          roles: [] as Role[],
          loading: true,
          saving: false,
          isNew
        }),
        catchError(() => {
          this.snackBar.open('Failed to load user', 'Close', { duration: 3000 });
          return of({
            user: null as UserDetail | null,
            roles: [] as Role[],
            loading: false,
            saving: false,
            isNew
          });
        })
      );
    }),
    takeUntil(this.destroy$)
  );

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private userService: UserService,
    private roleService: RoleService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) {
    this.initForm();
  }

  ngOnInit(): void {}

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initForm(): void {
    this.userForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      displayName: [''],
      password: [''],
    });
  }

  private patchForm(user: UserDetail): void {
    this.userForm.patchValue({
      email: user.email,
      displayName: user.displayName || '',
    });
    this.userForm.get('email')?.disable();
  }

  protected isRoleSelected(roleId: string): boolean {
    return this.selectedRoleIds.includes(roleId);
  }

  protected toggleRole(roleId: string): void {
    const index = this.selectedRoleIds.indexOf(roleId);
    if (index === -1) {
      this.selectedRoleIds.push(roleId);
    } else {
      this.selectedRoleIds.splice(index, 1);
    }
  }

  protected saveUser(vm: UserDetailViewModel): void {
    if (this.userForm.invalid) {
      this.snackBar.open('Please fix form errors', 'Close', { duration: 3000 });
      return;
    }

    this.saving$.next(true);
    const formValue = this.userForm.getRawValue();

    if (vm.isNew) {
      this.userService.createUser({
        email: formValue.email,
        password: formValue.password,
        displayName: formValue.displayName || undefined,
        roleIds: this.selectedRoleIds.length > 0 ? this.selectedRoleIds : undefined
      }).pipe(takeUntil(this.destroy$)).subscribe({
        next: (user) => {
          this.saving$.next(false);
          this.snackBar.open('User created', 'Close', { duration: 3000 });
          this.router.navigate(['/users', user.id]);
        },
        error: () => {
          this.saving$.next(false);
          this.snackBar.open('Failed to create user', 'Close', { duration: 3000 });
        }
      });
    } else {
      this.userService.updateUser(this.userId!, {
        displayName: formValue.displayName || undefined
      }).pipe(
        switchMap(() => this.userService.updateUserRoles(this.userId!, { roleIds: this.selectedRoleIds })),
        takeUntil(this.destroy$)
      ).subscribe({
        next: () => {
          this.saving$.next(false);
          this.snackBar.open('User updated', 'Close', { duration: 3000 });
          this.refresh$.next();
        },
        error: () => {
          this.saving$.next(false);
          this.snackBar.open('Failed to update user', 'Close', { duration: 3000 });
        }
      });
    }
  }

  protected resetPassword(): void {
    if (!this.userId) return;

    if (confirm('Are you sure you want to reset this user\'s password? A temporary password will be generated.')) {
      this.userService.resetPassword(this.userId).pipe(takeUntil(this.destroy$)).subscribe({
        next: (response) => {
          this.snackBar.open(`Password reset. Temporary password: ${response.temporaryPassword}`, 'Copy', {
            duration: 30000
          });
        },
        error: () => {
          this.snackBar.open('Failed to reset password', 'Close', { duration: 3000 });
        }
      });
    }
  }

  protected toggleStatus(user: UserDetail): void {
    const action = user.isActive
      ? this.userService.deactivateUser(user.id)
      : this.userService.activateUser(user.id);

    action.pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.snackBar.open(`User ${user.isActive ? 'deactivated' : 'activated'}`, 'Close', { duration: 3000 });
        this.refresh$.next();
      },
      error: () => {
        this.snackBar.open('Failed to update user status', 'Close', { duration: 3000 });
      }
    });
  }

  protected unlockUser(): void {
    if (!this.userId) return;

    this.userService.unlockUser(this.userId).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.snackBar.open('User unlocked', 'Close', { duration: 3000 });
        this.refresh$.next();
      },
      error: () => {
        this.snackBar.open('Failed to unlock user', 'Close', { duration: 3000 });
      }
    });
  }

  protected getInitials(user: UserDetail): string {
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

  protected getStatusClass(user: UserDetail): string {
    if (user.isLockedOut) return 'status-badge--locked';
    return user.isActive ? 'status-badge--active' : 'status-badge--inactive';
  }

  protected getStatusLabel(user: UserDetail): string {
    if (user.isLockedOut) return 'Locked';
    return user.isActive ? 'Active' : 'Inactive';
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

  protected goBack(): void {
    this.router.navigate(['/users']);
  }
}
