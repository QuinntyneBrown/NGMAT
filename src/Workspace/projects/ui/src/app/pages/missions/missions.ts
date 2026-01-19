import { Component, OnInit, signal, computed } from '@angular/core';
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
import { MatDividerModule } from '@angular/material/divider';

import { MissionService } from '../../services/mission.service';
import { Mission, MissionStatus, MissionType } from '../../models/mission.model';

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
    MatDividerModule,
  ],
  templateUrl: './missions.html',
  styleUrl: './missions.scss'
})
export class Missions implements OnInit {
  protected readonly displayedColumns = ['name', 'type', 'status', 'startEpoch', 'updatedAt', 'actions'];
  protected readonly statusOptions = Object.values(MissionStatus);
  protected readonly typeOptions = Object.values(MissionType);

  protected missions = signal<Mission[]>([]);
  protected loading = signal(false);
  protected totalCount = signal(0);
  protected pageSize = signal(10);
  protected pageIndex = signal(0);
  protected searchTerm = signal('');
  protected statusFilter = signal<MissionStatus | null>(null);

  constructor(
    private missionService: MissionService,
    private router: Router,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.loadMissions();
  }

  protected loadMissions(): void {
    this.loading.set(true);
    this.missionService.getMissions(
      this.pageIndex() + 1,
      this.pageSize(),
      this.statusFilter() || undefined,
      this.searchTerm() || undefined
    ).subscribe({
      next: (response) => {
        this.missions.set(response.items);
        this.totalCount.set(response.totalCount);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }

  protected onPageChange(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.loadMissions();
  }

  protected onSearch(): void {
    this.pageIndex.set(0);
    this.loadMissions();
  }

  protected onStatusFilterChange(status: MissionStatus | null): void {
    this.statusFilter.set(status);
    this.pageIndex.set(0);
    this.loadMissions();
  }

  protected clearFilters(): void {
    this.searchTerm.set('');
    this.statusFilter.set(null);
    this.pageIndex.set(0);
    this.loadMissions();
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
    this.missionService.cloneMission(mission.id).subscribe({
      next: (cloned) => {
        this.loadMissions();
        this.router.navigate(['/missions', cloned.id, 'edit']);
      }
    });
  }

  protected deleteMission(mission: Mission): void {
    if (confirm(`Are you sure you want to delete "${mission.name}"?`)) {
      this.missionService.deleteMission(mission.id).subscribe({
        next: () => {
          this.loadMissions();
        }
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
      day: 'numeric'
    });
  }

  protected formatDateTime(date: Date | undefined): string {
    if (!date) return '-';
    return new Date(date).toLocaleString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }
}
