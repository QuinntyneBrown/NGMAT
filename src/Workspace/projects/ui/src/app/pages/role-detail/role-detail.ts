import { Component, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDividerModule } from '@angular/material/divider';
import { MatChipsModule } from '@angular/material/chips';
import { BehaviorSubject, Subject, combineLatest, of } from 'rxjs';
import { switchMap, takeUntil, map, startWith, catchError, tap } from 'rxjs/operators';

import { RoleService } from '../../services/role.service';
import { PermissionService } from '../../services/permission.service';
import { RoleDetail } from '../../models/role.model';
import { PermissionGroup } from '../../models/permission.model';

interface RoleDetailViewModel {
  role: RoleDetail | null;
  permissionGroups: PermissionGroup[];
  loading: boolean;
  saving: boolean;
  isNew: boolean;
}

@Component({
  selector: 'app-role-detail',
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
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDividerModule,
    MatChipsModule,
  ],
  templateUrl: './role-detail.html',
  styleUrl: './role-detail.scss',
})
export class RoleDetailComponent implements OnDestroy {
  protected roleForm!: FormGroup;
  protected selectedPermissionIds: string[] = [];

  private readonly refresh$ = new BehaviorSubject<void>(undefined);
  private readonly saving$ = new BehaviorSubject<boolean>(false);
  private readonly destroy$ = new Subject<void>();
  private roleId: string | null = null;

  protected readonly viewModel$ = this.refresh$.pipe(
    switchMap(() => {
      const id = this.route.snapshot.paramMap.get('id');
      this.roleId = id;
      const isNew = id === 'new';

      return combineLatest([
        isNew ? of(null) : this.roleService.getRole(id!),
        this.permissionService.getPermissionGroups(),
        this.saving$
      ]).pipe(
        map(([role, permissionGroups, saving]) => ({
          role,
          permissionGroups,
          loading: false,
          saving,
          isNew
        })),
        tap(({ role, isNew }) => {
          if (!isNew && role) {
            this.patchForm(role);
            this.selectedPermissionIds = role.permissions.map(p => p.id);
          }
        }),
        startWith({
          role: null as RoleDetail | null,
          permissionGroups: [] as PermissionGroup[],
          loading: true,
          saving: false,
          isNew
        }),
        catchError(() => {
          this.snackBar.open('Failed to load role', 'Close', { duration: 3000 });
          return of({
            role: null as RoleDetail | null,
            permissionGroups: [] as PermissionGroup[],
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
    private roleService: RoleService,
    private permissionService: PermissionService,
    private snackBar: MatSnackBar
  ) {
    this.initForm();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initForm(): void {
    this.roleForm = this.fb.group({
      name: ['', Validators.required],
      description: [''],
    });
  }

  private patchForm(role: RoleDetail): void {
    this.roleForm.patchValue({
      name: role.name,
      description: role.description || '',
    });
    if (role.isSystem) {
      this.roleForm.disable();
    }
  }

  protected isPermissionSelected(permissionId: string): boolean {
    return this.selectedPermissionIds.includes(permissionId);
  }

  protected togglePermission(permissionId: string, vm: RoleDetailViewModel): void {
    if (vm.role?.isSystem) return;

    const index = this.selectedPermissionIds.indexOf(permissionId);
    if (index === -1) {
      this.selectedPermissionIds.push(permissionId);
    } else {
      this.selectedPermissionIds.splice(index, 1);
    }
  }

  protected saveRole(vm: RoleDetailViewModel): void {
    if (vm.role?.isSystem) return;

    if (this.roleForm.invalid) {
      this.snackBar.open('Please fix form errors', 'Close', { duration: 3000 });
      return;
    }

    this.saving$.next(true);
    const formValue = this.roleForm.value;

    if (vm.isNew) {
      this.roleService.createRole({
        name: formValue.name,
        description: formValue.description || undefined,
        permissionIds: this.selectedPermissionIds.length > 0 ? this.selectedPermissionIds : undefined
      }).pipe(takeUntil(this.destroy$)).subscribe({
        next: (role) => {
          this.saving$.next(false);
          this.snackBar.open('Role created', 'Close', { duration: 3000 });
          this.router.navigate(['/roles', role.id]);
        },
        error: () => {
          this.saving$.next(false);
          this.snackBar.open('Failed to create role', 'Close', { duration: 3000 });
        }
      });
    } else {
      this.roleService.updateRole(this.roleId!, {
        name: formValue.name,
        description: formValue.description || undefined
      }).pipe(
        switchMap(() => this.roleService.updateRolePermissions(this.roleId!, { permissionIds: this.selectedPermissionIds })),
        takeUntil(this.destroy$)
      ).subscribe({
        next: () => {
          this.saving$.next(false);
          this.snackBar.open('Role updated', 'Close', { duration: 3000 });
          this.refresh$.next();
        },
        error: () => {
          this.saving$.next(false);
          this.snackBar.open('Failed to update role', 'Close', { duration: 3000 });
        }
      });
    }
  }

  protected goBack(): void {
    this.router.navigate(['/roles']);
  }
}
