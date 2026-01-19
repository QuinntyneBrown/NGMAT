import { Component, OnInit, OnDestroy } from '@angular/core';
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
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDividerModule } from '@angular/material/divider';
import { BehaviorSubject, Subject, combineLatest, of } from 'rxjs';
import { switchMap, tap, catchError, takeUntil, map, startWith } from 'rxjs/operators';

import { MissionService } from '../../services/mission.service';
import { Mission, MissionStatus, MissionType, MissionListResponse } from '../../models/mission.model';

interface MissionsViewModel {
  missions: Mission[];
  loading: boolean;
  totalCount: number;
  pageSize: number;
  pageIndex: number;
  searchTerm: string;
  statusFilter: MissionStatus | null;
}

@Component({
  selector: 'app-missions',
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
    MatDialogModule,
    MatSnackBarModule,
    MatDividerModule,
  ],
  templateUrl: './missions.html',
  styleUrl: './missions.scss',
})
export class Missions implements OnInit, OnDestroy {
  protected readonly displayedColumns = ['name', 'type', 'status', 'startEpoch', 'updatedAt', 'actions'];
  protected readonly statusOptions = Object.values(MissionStatus);
  protected readonly typeOptions = Object.values(MissionType);

  private readonly pageIndex$ = new BehaviorSubject<number>(0);
  private readonly pageSize$ = new BehaviorSubject<number>(10);
  private readonly searchTerm$ = new BehaviorSubject<string>('');
  private readonly statusFilter$ = new BehaviorSubject<MissionStatus | null>(null);
  private readonly refresh$ = new BehaviorSubject<void>(undefined);
  private readonly destroy$ = new Subject<void>();

  protected searchTermValue = '';
  protected statusFilterValue: MissionStatus | null = null;

  protected readonly viewModel$ = combineLatest([
    this.pageIndex$,
    this.pageSize$,
    this.searchTerm$,
    this.statusFilter$,
    this.refresh$,
  ]).pipe(
    switchMap(([pageIndex, pageSize, searchTerm, statusFilter]) =>
      this.missionService
        .getMissions(pageIndex + 1, pageSize, statusFilter || undefined, searchTerm || undefined)
        .pipe(
          map((response) => ({
            missions: response.items,
            loading: false,
            totalCount: response.totalCount,
            pageSize,
            pageIndex,
            searchTerm,
            statusFilter,
          })),
          startWith({
            missions: [] as Mission[],
            loading: true,
            totalCount: 0,
            pageSize,
            pageIndex,
            searchTerm,
            statusFilter,
          }),
          catchError(() => {
            this.snackBar.open('Failed to load missions', 'Close', { duration: 3000 });
            return of({
              missions: [] as Mission[],
              loading: false,
              totalCount: 0,
              pageSize,
              pageIndex,
              searchTerm,
              statusFilter,
            });
          })
        )
    ),
    takeUntil(this.destroy$)
  );

  constructor(
    private missionService: MissionService,
    private router: Router,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.searchTermValue = this.searchTerm$.value;
    this.statusFilterValue = this.statusFilter$.value;
  }

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

  protected onStatusFilterChange(status: MissionStatus | null): void {
    this.statusFilterValue = status;
    this.statusFilter$.next(status);
    this.pageIndex$.next(0);
  }

  protected clearFilters(): void {
    this.searchTermValue = '';
    this.statusFilterValue = null;
    this.searchTerm$.next('');
    this.statusFilter$.next(null);
    this.pageIndex$.next(0);
  }

  protected clearSearch(): void {
    this.searchTermValue = '';
    this.searchTerm$.next('');
    this.pageIndex$.next(0);
  }

  protected createMission(): void {
    this.router.navigate(['/missions/new']);
  }

  protected editMission(mission: Mission): void {
    this.router.navigate(['/missions', mission.id, 'edit']);
  }

  protected viewMission(mission: Mission): void {
    this.router.navigate(['/missions', mission.id]);
  }

  protected cloneMission(mission: Mission): void {
    this.missionService.cloneMission(mission.id).pipe(takeUntil(this.destroy$)).subscribe({
      next: (cloned) => {
        this.snackBar.open('Mission cloned successfully', 'Close', { duration: 3000 });
        this.refresh$.next();
        this.router.navigate(['/missions', cloned.id, 'edit']);
      },
      error: () => {
        this.snackBar.open('Failed to clone mission', 'Close', { duration: 3000 });
      },
    });
  }

  protected deleteMission(mission: Mission): void {
    if (confirm(`Are you sure you want to delete "${mission.name}"?`)) {
      this.missionService.deleteMission(mission.id).pipe(takeUntil(this.destroy$)).subscribe({
        next: () => {
          this.snackBar.open('Mission deleted', 'Close', { duration: 3000 });
          this.refresh$.next();
        },
        error: () => {
          this.snackBar.open('Failed to delete mission', 'Close', { duration: 3000 });
        },
      });
    }
  }

  protected getStatusClass(status: MissionStatus): string {
    switch (status) {
      case MissionStatus.Active:
        return 'status-chip--active';
      case MissionStatus.Draft:
        return 'status-chip--draft';
      case MissionStatus.Completed:
        return 'status-chip--completed';
      case MissionStatus.Archived:
        return 'status-chip--archived';
      default:
        return '';
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
    if (!date) return '-';
    return new Date(date).toLocaleString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  }
}
