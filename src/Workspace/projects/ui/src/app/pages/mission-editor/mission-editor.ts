import { Component, OnInit, OnDestroy, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { Subject, takeUntil, debounceTime } from 'rxjs';

import { MissionService } from '../../services/mission.service';
import { Mission, MissionStatus, MissionType, MissionTreeNode } from '../../models/mission.model';

@Component({
  selector: 'app-mission-editor',
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    MatToolbarModule,
    MatIconModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatSlideToggleModule,
    MatExpansionModule,
    MatTooltipModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatMenuModule,
    MatDividerModule
  ],
  templateUrl: './mission-editor.html',
  styleUrl: './mission-editor.scss'
})
export class MissionEditor implements OnInit, OnDestroy {
  protected readonly missionTypes = Object.values(MissionType);
  protected readonly missionStatuses = Object.values(MissionStatus);
  protected readonly timeSystemOptions = [
    { value: 'UTC', label: 'UTC - Coordinated Universal Time' },
    { value: 'TAI', label: 'TAI - International Atomic Time' },
    { value: 'TDB', label: 'TDB - Barycentric Dynamical Time' },
    { value: 'TT', label: 'TT - Terrestrial Time' }
  ];
  protected readonly epochFormatOptions = [
    { value: 'Gregorian', label: 'Gregorian' },
    { value: 'MJD', label: 'Modified Julian Date' },
    { value: 'JD', label: 'Julian Date' }
  ];
  protected readonly centralBodyOptions = ['Earth', 'Moon', 'Mars', 'Sun'];
  protected readonly referenceFrameOptions = [
    { value: 'J2000', label: 'J2000 (Earth Centered Inertial)' },
    { value: 'ECEF', label: 'ECEF (Earth Centered Earth Fixed)' },
    { value: 'ICRF', label: 'ICRF (International Celestial Reference Frame)' }
  ];

  protected form!: FormGroup;
  protected mission = signal<Mission | null>(null);
  protected missionTree = signal<MissionTreeNode[]>([]);
  protected selectedNode = signal<MissionTreeNode | null>(null);
  protected loading = signal(false);
  protected saving = signal(false);
  protected autoSaved = signal(false);
  protected isNew = signal(false);
  protected validationStatus = signal<'valid' | 'warning' | 'error'>('valid');
  protected validationMessage = signal('All mission parameters are valid. Ready to propagate.');

  // Mission options toggles
  protected enableRealTimePropagation = signal(true);
  protected enableManeuverOptimization = signal(true);
  protected enableEphemerisOutput = signal(false);
  protected enableCollisionAvoidance = signal(true);

  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private missionService: MissionService,
    private snackBar: MatSnackBar
  ) {
    this.initForm();
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');

    if (id === 'new') {
      this.isNew.set(true);
      this.mission.set({
        id: '',
        name: '',
        description: '',
        type: MissionType.LEO,
        status: MissionStatus.Draft,
        startEpoch: new Date(),
        ownerId: '',
        createdAt: new Date()
      });
    } else if (id) {
      this.loadMission(id);
    }

    this.loadMissionTree();
    this.setupAutoSave();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initForm(): void {
    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3)]],
      description: [''],
      type: [MissionType.LEO, Validators.required],
      status: [MissionStatus.Draft, Validators.required],
      startDate: ['', Validators.required],
      startTime: ['12:00:00.000', Validators.required],
      endDate: [''],
      endTime: ['12:00:00.000'],
      timeSystem: ['UTC'],
      epochFormat: ['Gregorian'],
      centralBody: ['Earth'],
      referenceFrame: ['J2000']
    });
  }

  private loadMission(id: string): void {
    this.loading.set(true);
    this.missionService.getMission(id).subscribe({
      next: (mission) => {
        if (mission) {
          this.mission.set(mission);
          this.patchForm(mission);
        }
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.snackBar.open('Failed to load mission', 'Close', { duration: 3000 });
      }
    });
  }

  private patchForm(mission: Mission): void {
    const startDate = new Date(mission.startEpoch);
    const endDate = mission.endEpoch ? new Date(mission.endEpoch) : null;

    this.form.patchValue({
      name: mission.name,
      description: mission.description || '',
      type: mission.type,
      status: mission.status,
      startDate: this.formatDateForInput(startDate),
      startTime: this.formatTimeForInput(startDate),
      endDate: endDate ? this.formatDateForInput(endDate) : '',
      endTime: endDate ? this.formatTimeForInput(endDate) : '12:00:00.000'
    });
  }

  private loadMissionTree(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.missionService.getMissionTree(id).subscribe({
        next: (tree) => {
          this.missionTree.set(tree);
          if (tree.length > 0) {
            this.selectedNode.set(tree[0]);
          }
        }
      });
    } else {
      // Default tree for new mission
      this.missionTree.set([{
        id: 'mission',
        label: 'Mission',
        icon: 'rocket_launch',
        type: 'mission',
        expanded: true,
        children: []
      }]);
      this.selectedNode.set(this.missionTree()[0]);
    }
  }

  private setupAutoSave(): void {
    this.form.valueChanges
      .pipe(
        takeUntil(this.destroy$),
        debounceTime(2000)
      )
      .subscribe(() => {
        if (this.form.valid && !this.isNew() && this.mission()) {
          this.autoSave();
        }
      });
  }

  private autoSave(): void {
    const mission = this.mission();
    if (!mission) return;

    const formValue = this.form.value;
    this.missionService.updateMission(mission.id, {
      name: formValue.name,
      description: formValue.description,
      startEpoch: this.combineDateTime(formValue.startDate, formValue.startTime),
      endEpoch: formValue.endDate ? this.combineDateTime(formValue.endDate, formValue.endTime) : undefined
    }).subscribe({
      next: () => {
        this.autoSaved.set(true);
        setTimeout(() => this.autoSaved.set(false), 2000);
      }
    });
  }

  protected onSave(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    const formValue = this.form.value;

    if (this.isNew()) {
      this.missionService.createMission({
        name: formValue.name,
        description: formValue.description,
        type: formValue.type,
        startEpoch: this.combineDateTime(formValue.startDate, formValue.startTime),
        endEpoch: formValue.endDate ? this.combineDateTime(formValue.endDate, formValue.endTime) : undefined
      }).subscribe({
        next: (mission) => {
          this.saving.set(false);
          this.snackBar.open('Mission created successfully', 'Close', { duration: 3000 });
          this.router.navigate(['/missions', mission.id, 'edit']);
        },
        error: () => {
          this.saving.set(false);
          this.snackBar.open('Failed to create mission', 'Close', { duration: 3000 });
        }
      });
    } else {
      const mission = this.mission();
      if (!mission) return;

      this.missionService.updateMission(mission.id, {
        name: formValue.name,
        description: formValue.description,
        startEpoch: this.combineDateTime(formValue.startDate, formValue.startTime),
        endEpoch: formValue.endDate ? this.combineDateTime(formValue.endDate, formValue.endTime) : undefined
      }).subscribe({
        next: () => {
          this.saving.set(false);
          this.snackBar.open('Mission saved successfully', 'Close', { duration: 3000 });
        },
        error: () => {
          this.saving.set(false);
          this.snackBar.open('Failed to save mission', 'Close', { duration: 3000 });
        }
      });
    }
  }

  protected onCancel(): void {
    this.router.navigate(['/missions']);
  }

  protected onRun(): void {
    this.snackBar.open('Running mission propagation...', 'Close', { duration: 3000 });
  }

  protected onClone(): void {
    const mission = this.mission();
    if (!mission) return;

    this.missionService.cloneMission(mission.id).subscribe({
      next: (cloned) => {
        this.snackBar.open('Mission cloned successfully', 'Close', { duration: 3000 });
        this.router.navigate(['/missions', cloned.id, 'edit']);
      }
    });
  }

  protected onExport(): void {
    this.snackBar.open('Exporting mission...', 'Close', { duration: 3000 });
  }

  protected onDelete(): void {
    const mission = this.mission();
    if (!mission) return;

    if (confirm(`Are you sure you want to delete "${mission.name}"?`)) {
      this.missionService.deleteMission(mission.id).subscribe({
        next: () => {
          this.snackBar.open('Mission deleted', 'Close', { duration: 3000 });
          this.router.navigate(['/missions']);
        }
      });
    }
  }

  protected selectNode(node: MissionTreeNode): void {
    this.selectedNode.set(node);
  }

  protected toggleNode(node: MissionTreeNode, event: Event): void {
    event.stopPropagation();
    node.expanded = !node.expanded;
  }

  protected addTreeItem(): void {
    this.snackBar.open('Add component dialog coming soon', 'Close', { duration: 2000 });
  }

  protected goBack(): void {
    this.router.navigate(['/missions']);
  }

  protected getValidationIcon(): string {
    switch (this.validationStatus()) {
      case 'valid': return 'check_circle';
      case 'warning': return 'warning';
      case 'error': return 'error';
    }
  }

  private formatDateForInput(date: Date): string {
    return date.toISOString().split('T')[0];
  }

  private formatTimeForInput(date: Date): string {
    return date.toISOString().split('T')[1].split('Z')[0];
  }

  private combineDateTime(dateStr: string, timeStr: string): Date {
    return new Date(`${dateStr}T${timeStr}Z`);
  }

  protected getDuration(): string {
    const startDate = this.form.get('startDate')?.value;
    const startTime = this.form.get('startTime')?.value;
    const endDate = this.form.get('endDate')?.value;
    const endTime = this.form.get('endTime')?.value;

    if (!startDate || !endDate) return '';

    const start = this.combineDateTime(startDate, startTime || '00:00:00');
    const end = this.combineDateTime(endDate, endTime || '00:00:00');
    const diffMs = end.getTime() - start.getTime();
    const days = Math.floor(diffMs / (1000 * 60 * 60 * 24));
    const hours = Math.floor((diffMs % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));

    if (days > 0) {
      return `Duration: ${days}d ${hours}h`;
    }
    return `Duration: ${hours}h`;
  }
}
