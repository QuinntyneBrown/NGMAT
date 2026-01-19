import { Component, OnInit, OnDestroy } from '@angular/core';
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
import { BehaviorSubject, Subject, combineLatest, of } from 'rxjs';
import { takeUntil, debounceTime, switchMap, map, catchError, tap, filter } from 'rxjs/operators';

import { MissionService } from '../../services/mission.service';
import { Mission, MissionStatus, MissionType, MissionTreeNode } from '../../models/mission.model';

interface MissionEditorViewModel {
  mission: Mission | null;
  missionTree: MissionTreeNode[];
  selectedNode: MissionTreeNode | null;
  loading: boolean;
  saving: boolean;
  autoSaved: boolean;
  isNew: boolean;
  validationStatus: 'valid' | 'warning' | 'error';
  validationMessage: string;
  enableRealTimePropagation: boolean;
  enableManeuverOptimization: boolean;
  enableEphemerisOutput: boolean;
  enableCollisionAvoidance: boolean;
}

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
    MatDividerModule,
  ],
  templateUrl: './mission-editor.html',
  styleUrl: './mission-editor.scss',
})
export class MissionEditor implements OnInit, OnDestroy {
  protected readonly missionTypes = Object.values(MissionType);
  protected readonly missionStatuses = Object.values(MissionStatus);
  protected readonly timeSystemOptions = [
    { value: 'UTC', label: 'UTC - Coordinated Universal Time' },
    { value: 'TAI', label: 'TAI - International Atomic Time' },
    { value: 'TDB', label: 'TDB - Barycentric Dynamical Time' },
    { value: 'TT', label: 'TT - Terrestrial Time' },
  ];
  protected readonly epochFormatOptions = [
    { value: 'Gregorian', label: 'Gregorian' },
    { value: 'MJD', label: 'Modified Julian Date' },
    { value: 'JD', label: 'Julian Date' },
  ];
  protected readonly centralBodyOptions = ['Earth', 'Moon', 'Mars', 'Sun'];
  protected readonly referenceFrameOptions = [
    { value: 'J2000', label: 'J2000 (Earth Centered Inertial)' },
    { value: 'ECEF', label: 'ECEF (Earth Centered Earth Fixed)' },
    { value: 'ICRF', label: 'ICRF (International Celestial Reference Frame)' },
  ];

  protected form!: FormGroup;

  private readonly mission$ = new BehaviorSubject<Mission | null>(null);
  private readonly missionTree$ = new BehaviorSubject<MissionTreeNode[]>([]);
  private readonly selectedNode$ = new BehaviorSubject<MissionTreeNode | null>(null);
  private readonly loading$ = new BehaviorSubject<boolean>(false);
  private readonly saving$ = new BehaviorSubject<boolean>(false);
  private readonly autoSaved$ = new BehaviorSubject<boolean>(false);
  private readonly isNew$ = new BehaviorSubject<boolean>(false);
  private readonly validationStatus$ = new BehaviorSubject<'valid' | 'warning' | 'error'>('valid');
  private readonly validationMessage$ = new BehaviorSubject<string>(
    'All mission parameters are valid. Ready to propagate.'
  );
  private readonly enableRealTimePropagation$ = new BehaviorSubject<boolean>(true);
  private readonly enableManeuverOptimization$ = new BehaviorSubject<boolean>(true);
  private readonly enableEphemerisOutput$ = new BehaviorSubject<boolean>(false);
  private readonly enableCollisionAvoidance$ = new BehaviorSubject<boolean>(true);

  private readonly destroy$ = new Subject<void>();

  protected readonly viewModel$ = combineLatest([
    this.mission$,
    this.missionTree$,
    this.selectedNode$,
    this.loading$,
    this.saving$,
    this.autoSaved$,
    this.isNew$,
    this.validationStatus$,
    this.validationMessage$,
    this.enableRealTimePropagation$,
    this.enableManeuverOptimization$,
    this.enableEphemerisOutput$,
    this.enableCollisionAvoidance$,
  ]).pipe(
    map(
      ([
        mission,
        missionTree,
        selectedNode,
        loading,
        saving,
        autoSaved,
        isNew,
        validationStatus,
        validationMessage,
        enableRealTimePropagation,
        enableManeuverOptimization,
        enableEphemerisOutput,
        enableCollisionAvoidance,
      ]) => ({
        mission,
        missionTree,
        selectedNode,
        loading,
        saving,
        autoSaved,
        isNew,
        validationStatus,
        validationMessage,
        enableRealTimePropagation,
        enableManeuverOptimization,
        enableEphemerisOutput,
        enableCollisionAvoidance,
      })
    )
  );

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
      this.isNew$.next(true);
      this.mission$.next({
        id: '',
        name: '',
        description: '',
        type: MissionType.LEO,
        status: MissionStatus.Draft,
        startEpoch: new Date(),
        ownerId: '',
        createdAt: new Date(),
      });
      this.initDefaultTree();
    } else if (id) {
      this.loadMission(id);
      this.loadMissionTree(id);
    }

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
      referenceFrame: ['J2000'],
    });
  }

  private loadMission(id: string): void {
    this.loading$.next(true);
    this.missionService
      .getMission(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (mission) => {
          if (mission) {
            this.mission$.next(mission);
            this.patchForm(mission);
          }
          this.loading$.next(false);
        },
        error: () => {
          this.loading$.next(false);
          this.snackBar.open('Failed to load mission', 'Close', { duration: 3000 });
        },
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
      endTime: endDate ? this.formatTimeForInput(endDate) : '12:00:00.000',
    });
  }

  private loadMissionTree(id: string): void {
    this.missionService
      .getMissionTree(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (tree) => {
          this.missionTree$.next(tree);
          if (tree.length > 0) {
            this.selectedNode$.next(tree[0]);
          }
        },
      });
  }

  private initDefaultTree(): void {
    const defaultTree: MissionTreeNode[] = [
      {
        id: 'mission',
        label: 'Mission',
        icon: 'rocket_launch',
        type: 'mission',
        expanded: true,
        children: [],
      },
    ];
    this.missionTree$.next(defaultTree);
    this.selectedNode$.next(defaultTree[0]);
  }

  private setupAutoSave(): void {
    this.form.valueChanges
      .pipe(
        takeUntil(this.destroy$),
        debounceTime(2000),
        filter(() => this.form.valid && !this.isNew$.value && this.mission$.value !== null)
      )
      .subscribe(() => {
        this.autoSave();
      });
  }

  private autoSave(): void {
    const mission = this.mission$.value;
    if (!mission) return;

    const formValue = this.form.value;
    this.missionService
      .updateMission(mission.id, {
        name: formValue.name,
        description: formValue.description,
        startEpoch: this.combineDateTime(formValue.startDate, formValue.startTime),
        endEpoch: formValue.endDate
          ? this.combineDateTime(formValue.endDate, formValue.endTime)
          : undefined,
      })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.autoSaved$.next(true);
          setTimeout(() => this.autoSaved$.next(false), 2000);
        },
      });
  }

  protected onSave(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving$.next(true);
    const formValue = this.form.value;

    if (this.isNew$.value) {
      this.missionService
        .createMission({
          name: formValue.name,
          description: formValue.description,
          type: formValue.type,
          startEpoch: this.combineDateTime(formValue.startDate, formValue.startTime),
          endEpoch: formValue.endDate
            ? this.combineDateTime(formValue.endDate, formValue.endTime)
            : undefined,
        })
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (mission) => {
            this.saving$.next(false);
            this.snackBar.open('Mission created successfully', 'Close', { duration: 3000 });
            this.router.navigate(['/missions', mission.id, 'edit']);
          },
          error: () => {
            this.saving$.next(false);
            this.snackBar.open('Failed to create mission', 'Close', { duration: 3000 });
          },
        });
    } else {
      const mission = this.mission$.value;
      if (!mission) return;

      this.missionService
        .updateMission(mission.id, {
          name: formValue.name,
          description: formValue.description,
          startEpoch: this.combineDateTime(formValue.startDate, formValue.startTime),
          endEpoch: formValue.endDate
            ? this.combineDateTime(formValue.endDate, formValue.endTime)
            : undefined,
        })
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.saving$.next(false);
            this.snackBar.open('Mission saved successfully', 'Close', { duration: 3000 });
          },
          error: () => {
            this.saving$.next(false);
            this.snackBar.open('Failed to save mission', 'Close', { duration: 3000 });
          },
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
    const mission = this.mission$.value;
    if (!mission) return;

    this.missionService
      .cloneMission(mission.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (cloned) => {
          this.snackBar.open('Mission cloned successfully', 'Close', { duration: 3000 });
          this.router.navigate(['/missions', cloned.id, 'edit']);
        },
      });
  }

  protected onExport(): void {
    this.snackBar.open('Exporting mission...', 'Close', { duration: 3000 });
  }

  protected onDelete(): void {
    const mission = this.mission$.value;
    if (!mission) return;

    if (confirm(`Are you sure you want to delete "${mission.name}"?`)) {
      this.missionService
        .deleteMission(mission.id)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.snackBar.open('Mission deleted', 'Close', { duration: 3000 });
            this.router.navigate(['/missions']);
          },
        });
    }
  }

  protected selectNode(node: MissionTreeNode): void {
    this.selectedNode$.next(node);
  }

  protected toggleNode(node: MissionTreeNode, event: Event): void {
    event.stopPropagation();
    node.expanded = !node.expanded;
    // Trigger change detection by creating a new array reference
    this.missionTree$.next([...this.missionTree$.value]);
  }

  protected addTreeItem(): void {
    this.snackBar.open('Add component dialog coming soon', 'Close', { duration: 2000 });
  }

  protected goBack(): void {
    this.router.navigate(['/missions']);
  }

  protected getValidationIcon(status: 'valid' | 'warning' | 'error'): string {
    switch (status) {
      case 'valid':
        return 'check_circle';
      case 'warning':
        return 'warning';
      case 'error':
        return 'error';
    }
  }

  protected onToggleRealTimePropagation(checked: boolean): void {
    this.enableRealTimePropagation$.next(checked);
  }

  protected onToggleManeuverOptimization(checked: boolean): void {
    this.enableManeuverOptimization$.next(checked);
  }

  protected onToggleEphemerisOutput(checked: boolean): void {
    this.enableEphemerisOutput$.next(checked);
  }

  protected onToggleCollisionAvoidance(checked: boolean): void {
    this.enableCollisionAvoidance$.next(checked);
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
