import { Component, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { BehaviorSubject, Subject, combineLatest, of } from 'rxjs';
import { switchMap, takeUntil, map, startWith, catchError } from 'rxjs/operators';

import { PermissionService } from '../../services/permission.service';
import { PermissionGroup } from '../../models/permission.model';

interface PermissionsViewModel {
  permissionGroups: PermissionGroup[];
  resources: string[];
  loading: boolean;
  searchTerm: string;
  resourceFilter: string | null;
}

@Component({
  selector: 'app-permissions',
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
  ],
  templateUrl: './permissions.html',
  styleUrl: './permissions.scss',
})
export class Permissions implements OnDestroy {
  private readonly searchTerm$ = new BehaviorSubject<string>('');
  private readonly resourceFilter$ = new BehaviorSubject<string | null>(null);
  private readonly destroy$ = new Subject<void>();

  protected searchTermValue = '';
  protected resourceFilterValue: string | null = null;

  protected readonly viewModel$ = combineLatest([
    this.permissionService.getPermissionGroups(),
    this.permissionService.getResources(),
    this.searchTerm$,
    this.resourceFilter$
  ]).pipe(
    map(([groups, resources, searchTerm, resourceFilter]) => {
      let filteredGroups = groups;

      if (resourceFilter) {
        filteredGroups = groups.filter(g => g.resource === resourceFilter);
      }

      if (searchTerm) {
        const search = searchTerm.toLowerCase();
        filteredGroups = filteredGroups.map(group => ({
          ...group,
          permissions: group.permissions.filter(p =>
            p.name.toLowerCase().includes(search) ||
            p.description?.toLowerCase().includes(search) ||
            p.action.toLowerCase().includes(search)
          )
        })).filter(g => g.permissions.length > 0);
      }

      return {
        permissionGroups: filteredGroups,
        resources,
        loading: false,
        searchTerm,
        resourceFilter,
      };
    }),
    startWith({
      permissionGroups: [] as PermissionGroup[],
      resources: [] as string[],
      loading: true,
      searchTerm: '',
      resourceFilter: null,
    }),
    catchError(() => {
      this.snackBar.open('Failed to load permissions', 'Close', { duration: 3000 });
      return of({
        permissionGroups: [] as PermissionGroup[],
        resources: [] as string[],
        loading: false,
        searchTerm: '',
        resourceFilter: null,
      });
    }),
    takeUntil(this.destroy$)
  );

  constructor(
    private permissionService: PermissionService,
    private snackBar: MatSnackBar
  ) {}

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  protected onSearch(): void {
    this.searchTerm$.next(this.searchTermValue);
  }

  protected onResourceFilterChange(resource: string | null): void {
    this.resourceFilterValue = resource;
    this.resourceFilter$.next(resource);
  }

  protected clearSearch(): void {
    this.searchTermValue = '';
    this.searchTerm$.next('');
  }

  protected clearFilters(): void {
    this.searchTermValue = '';
    this.resourceFilterValue = null;
    this.searchTerm$.next('');
    this.resourceFilter$.next(null);
  }
}
