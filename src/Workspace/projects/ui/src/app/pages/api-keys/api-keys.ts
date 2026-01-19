import { Component, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatMenuModule } from '@angular/material/menu';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { BehaviorSubject, Subject, combineLatest, of } from 'rxjs';
import { switchMap, takeUntil, map, startWith, catchError } from 'rxjs/operators';
import { Clipboard } from '@angular/cdk/clipboard';

import { ApiKeyService } from '../../services/api-key.service';
import { ApiKey, ApiKeyStats, CreateApiKeyResponse } from '../../models/api-key.model';

interface ApiKeysViewModel {
  apiKeys: ApiKey[];
  stats: ApiKeyStats | null;
  loading: boolean;
  totalCount: number;
  pageSize: number;
  pageIndex: number;
}

@Component({
  selector: 'app-api-keys',
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatTableModule,
    MatPaginatorModule,
    MatFormFieldModule,
    MatInputModule,
    MatMenuModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    MatDialogModule,
    MatSnackBarModule,
  ],
  templateUrl: './api-keys.html',
  styleUrl: './api-keys.scss',
})
export class ApiKeys implements OnDestroy {
  protected readonly displayedColumns = ['name', 'keyPrefix', 'scopes', 'created', 'lastUsed', 'expires', 'status', 'actions'];

  protected showCreateForm = false;
  protected createForm!: FormGroup;
  protected createdKey: string | null = null;

  private readonly pageIndex$ = new BehaviorSubject<number>(0);
  private readonly pageSize$ = new BehaviorSubject<number>(10);
  private readonly refresh$ = new BehaviorSubject<void>(undefined);
  private readonly destroy$ = new Subject<void>();

  protected readonly viewModel$ = combineLatest([
    this.pageIndex$,
    this.pageSize$,
    this.refresh$,
  ]).pipe(
    switchMap(([pageIndex, pageSize]) =>
      combineLatest([
        this.apiKeyService.getAllApiKeys(pageIndex * pageSize, pageSize),
        this.apiKeyService.getApiKeyStats()
      ]).pipe(
        map(([keysResponse, stats]) => ({
          apiKeys: keysResponse.items,
          stats,
          loading: false,
          totalCount: keysResponse.totalCount,
          pageSize,
          pageIndex,
        })),
        startWith({
          apiKeys: [] as ApiKey[],
          stats: null as ApiKeyStats | null,
          loading: true,
          totalCount: 0,
          pageSize,
          pageIndex,
        }),
        catchError(() => {
          this.snackBar.open('Failed to load API keys', 'Close', { duration: 3000 });
          return of({
            apiKeys: [] as ApiKey[],
            stats: null as ApiKeyStats | null,
            loading: false,
            totalCount: 0,
            pageSize,
            pageIndex,
          });
        })
      )
    ),
    takeUntil(this.destroy$)
  );

  constructor(
    private fb: FormBuilder,
    private apiKeyService: ApiKeyService,
    private snackBar: MatSnackBar,
    private clipboard: Clipboard
  ) {
    this.initForm();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initForm(): void {
    this.createForm = this.fb.group({
      name: ['', Validators.required],
      prefix: ['api', Validators.required],
      expiresInDays: [90],
    });
  }

  protected onPageChange(event: PageEvent): void {
    this.pageIndex$.next(event.pageIndex);
    this.pageSize$.next(event.pageSize);
  }

  protected showCreate(): void {
    this.showCreateForm = true;
    this.createdKey = null;
    this.createForm.reset({ name: '', prefix: 'api', expiresInDays: 90 });
  }

  protected cancelCreate(): void {
    this.showCreateForm = false;
    this.createdKey = null;
  }

  protected createApiKey(): void {
    if (this.createForm.invalid) return;

    const formValue = this.createForm.value;
    this.apiKeyService.createApiKey({
      name: formValue.name,
      prefix: formValue.prefix,
      expiresInDays: formValue.expiresInDays || undefined
    }).pipe(takeUntil(this.destroy$)).subscribe({
      next: (response) => {
        this.createdKey = response.key;
        this.snackBar.open('API key created! Copy it now - it won\'t be shown again.', 'Copy', {
          duration: 30000
        }).onAction().subscribe(() => {
          this.copyKey(response.key);
        });
        this.refresh$.next();
      },
      error: () => {
        this.snackBar.open('Failed to create API key', 'Close', { duration: 3000 });
      }
    });
  }

  protected copyKey(key: string): void {
    this.clipboard.copy(key);
    this.snackBar.open('API key copied to clipboard', 'Close', { duration: 3000 });
  }

  protected revokeApiKey(apiKey: ApiKey): void {
    if (confirm(`Are you sure you want to revoke "${apiKey.name}"? This action cannot be undone.`)) {
      this.apiKeyService.revokeApiKey(apiKey.id).pipe(takeUntil(this.destroy$)).subscribe({
        next: () => {
          this.snackBar.open('API key revoked', 'Close', { duration: 3000 });
          this.refresh$.next();
        },
        error: () => {
          this.snackBar.open('Failed to revoke API key', 'Close', { duration: 3000 });
        }
      });
    }
  }

  protected deleteApiKey(apiKey: ApiKey): void {
    if (confirm(`Are you sure you want to delete "${apiKey.name}"?`)) {
      this.apiKeyService.deleteApiKey(apiKey.id).pipe(takeUntil(this.destroy$)).subscribe({
        next: () => {
          this.snackBar.open('API key deleted', 'Close', { duration: 3000 });
          this.refresh$.next();
        },
        error: () => {
          this.snackBar.open('Failed to delete API key', 'Close', { duration: 3000 });
        }
      });
    }
  }

  protected getStatusClass(status: string): string {
    switch (status) {
      case 'Active': return 'status-chip--active';
      case 'Expired': return 'status-chip--expired';
      case 'Revoked': return 'status-chip--revoked';
      default: return '';
    }
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
