import { Injectable, signal, computed } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of, delay, BehaviorSubject } from 'rxjs';
import { map } from 'rxjs/operators';
import {
  Mission,
  MissionType,
  MissionStatus,
  MissionListResponse,
  CreateMissionRequest,
  UpdateMissionRequest,
  MissionTreeNode
} from '../models/mission.model';

@Injectable({
  providedIn: 'root'
})
export class MissionService {
  private readonly apiUrl = '/api/v1/missions';

  // Mock data for development
  private readonly mockMissions: Mission[] = [
    {
      id: 'MSN-2025-0042',
      name: 'LEO Constellation Deploy',
      description: 'Deploy 6 satellites to low Earth orbit constellation for global coverage.',
      type: MissionType.LEO,
      status: MissionStatus.Active,
      startEpoch: new Date('2025-03-15T12:00:00Z'),
      endEpoch: new Date('2025-03-17T12:00:00Z'),
      ownerId: 'user-001',
      createdAt: new Date('2025-01-10'),
      updatedAt: new Date('2025-01-18')
    },
    {
      id: 'MSN-2025-0041',
      name: 'GEO Station Keeping',
      description: 'Maintain geostationary orbit position for communication satellite.',
      type: MissionType.GEO,
      status: MissionStatus.Active,
      startEpoch: new Date('2025-02-01T00:00:00Z'),
      endEpoch: new Date('2025-12-31T23:59:59Z'),
      ownerId: 'user-001',
      createdAt: new Date('2025-01-05'),
      updatedAt: new Date('2025-01-15')
    },
    {
      id: 'MSN-2025-0040',
      name: 'Lunar Transfer',
      description: 'Trans-lunar injection and lunar orbit insertion mission.',
      type: MissionType.Lunar,
      status: MissionStatus.Draft,
      startEpoch: new Date('2025-06-01T08:00:00Z'),
      ownerId: 'user-001',
      createdAt: new Date('2025-01-02'),
      updatedAt: new Date('2025-01-12')
    },
    {
      id: 'MSN-2024-0156',
      name: 'MEO Navigation Constellation',
      description: 'Medium Earth orbit navigation satellite deployment.',
      type: MissionType.MEO,
      status: MissionStatus.Completed,
      startEpoch: new Date('2024-09-15T06:00:00Z'),
      endEpoch: new Date('2024-12-15T18:00:00Z'),
      ownerId: 'user-001',
      createdAt: new Date('2024-08-01'),
      updatedAt: new Date('2024-12-15')
    },
    {
      id: 'MSN-2024-0122',
      name: 'ISS Resupply Mission',
      description: 'Cargo delivery to International Space Station.',
      type: MissionType.LEO,
      status: MissionStatus.Archived,
      startEpoch: new Date('2024-05-10T14:30:00Z'),
      endEpoch: new Date('2024-05-12T08:00:00Z'),
      ownerId: 'user-001',
      createdAt: new Date('2024-04-01'),
      updatedAt: new Date('2024-05-12')
    }
  ];

  private currentMission = new BehaviorSubject<Mission | null>(null);
  currentMission$ = this.currentMission.asObservable();

  constructor(private http: HttpClient) {}

  getMissions(
    page: number = 1,
    pageSize: number = 20,
    status?: MissionStatus,
    search?: string
  ): Observable<MissionListResponse> {
    // For now, return mock data
    let filtered = [...this.mockMissions];

    if (status) {
      filtered = filtered.filter(m => m.status === status);
    }

    if (search) {
      const searchLower = search.toLowerCase();
      filtered = filtered.filter(m =>
        m.name.toLowerCase().includes(searchLower) ||
        m.description?.toLowerCase().includes(searchLower)
      );
    }

    const start = (page - 1) * pageSize;
    const items = filtered.slice(start, start + pageSize);

    return of({
      items,
      page,
      pageSize,
      totalCount: filtered.length,
      totalPages: Math.ceil(filtered.length / pageSize)
    }).pipe(delay(300));
  }

  getMission(id: string): Observable<Mission | undefined> {
    const mission = this.mockMissions.find(m => m.id === id);
    if (mission) {
      this.currentMission.next(mission);
    }
    return of(mission).pipe(delay(200));
  }

  createMission(request: CreateMissionRequest): Observable<Mission> {
    const newMission: Mission = {
      id: `MSN-2025-${String(Math.floor(Math.random() * 9999)).padStart(4, '0')}`,
      ...request,
      status: MissionStatus.Draft,
      ownerId: 'user-001',
      createdAt: new Date(),
      updatedAt: new Date()
    };
    this.mockMissions.unshift(newMission);
    return of(newMission).pipe(delay(300));
  }

  updateMission(id: string, request: UpdateMissionRequest): Observable<Mission> {
    const index = this.mockMissions.findIndex(m => m.id === id);
    if (index >= 0) {
      this.mockMissions[index] = {
        ...this.mockMissions[index],
        ...request,
        updatedAt: new Date()
      };
      this.currentMission.next(this.mockMissions[index]);
      return of(this.mockMissions[index]).pipe(delay(300));
    }
    throw new Error('Mission not found');
  }

  deleteMission(id: string): Observable<void> {
    const index = this.mockMissions.findIndex(m => m.id === id);
    if (index >= 0) {
      this.mockMissions.splice(index, 1);
    }
    return of(undefined).pipe(delay(300));
  }

  changeStatus(id: string, status: MissionStatus): Observable<Mission> {
    const index = this.mockMissions.findIndex(m => m.id === id);
    if (index >= 0) {
      this.mockMissions[index] = {
        ...this.mockMissions[index],
        status,
        updatedAt: new Date()
      };
      this.currentMission.next(this.mockMissions[index]);
      return of(this.mockMissions[index]).pipe(delay(300));
    }
    throw new Error('Mission not found');
  }

  cloneMission(id: string): Observable<Mission> {
    const original = this.mockMissions.find(m => m.id === id);
    if (!original) {
      throw new Error('Mission not found');
    }

    const cloned: Mission = {
      ...original,
      id: `MSN-2025-${String(Math.floor(Math.random() * 9999)).padStart(4, '0')}`,
      name: `${original.name} (Copy)`,
      status: MissionStatus.Draft,
      createdAt: new Date(),
      updatedAt: new Date()
    };
    this.mockMissions.unshift(cloned);
    return of(cloned).pipe(delay(300));
  }

  getMissionTree(missionId: string): Observable<MissionTreeNode[]> {
    return of([
      {
        id: 'mission',
        label: 'Mission',
        icon: 'rocket_launch',
        type: 'mission' as const,
        expanded: true,
        children: [
          {
            id: 'spacecraft',
            label: 'Spacecraft',
            icon: 'satellite_alt',
            type: 'spacecraft' as const,
            expanded: true,
            children: [
              { id: 'sat-001', label: 'SAT-001', icon: 'satellite', type: 'spacecraft' as const },
              { id: 'sat-002', label: 'SAT-002', icon: 'satellite', type: 'spacecraft' as const },
              { id: 'sat-003', label: 'SAT-003', icon: 'satellite', type: 'spacecraft' as const }
            ]
          },
          {
            id: 'propagator',
            label: 'Propagator',
            icon: 'route',
            type: 'propagator' as const
          },
          {
            id: 'maneuvers',
            label: 'Maneuvers',
            icon: 'bolt',
            type: 'maneuver' as const,
            expanded: true,
            children: [
              { id: 'orbit-raise', label: 'Orbit Raise', icon: 'adjust', type: 'maneuver' as const },
              { id: 'plane-change', label: 'Plane Change', icon: 'adjust', type: 'maneuver' as const }
            ]
          },
          {
            id: 'force-model',
            label: 'Force Model',
            icon: 'public',
            type: 'forceModel' as const
          },
          {
            id: 'reports',
            label: 'Reports',
            icon: 'assessment',
            type: 'report' as const
          }
        ]
      }
    ]).pipe(delay(200));
  }

  setCurrentMission(mission: Mission | null): void {
    this.currentMission.next(mission);
  }
}
