import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject, of } from 'rxjs';
import { map, tap, catchError } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import {
  Mission,
  MissionType,
  MissionStatus,
  MissionListResponse,
  CreateMissionRequest,
  UpdateMissionRequest,
  MissionTreeNode
} from '../models/mission.model';

export interface ChangeStatusRequest {
  status: MissionStatus;
}

export interface MissionExportData {
  exportedAt: string;
  version: string;
  mission: MissionData;
}

export interface MissionData {
  name: string;
  description?: string;
  type: MissionType;
  startEpoch: string;
  endEpoch?: string;
}

@Injectable({
  providedIn: 'root'
})
export class MissionService {
  private readonly apiUrl = `${environment.baseUrl}/api/missions/v1/missions`;

  private currentMission = new BehaviorSubject<Mission | null>(null);
  currentMission$ = this.currentMission.asObservable();

  constructor(private http: HttpClient) {}

  getMissions(
    page: number = 1,
    pageSize: number = 20,
    status?: MissionStatus,
    search?: string
  ): Observable<MissionListResponse> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('size', pageSize.toString());

    if (status) {
      params = params.set('status', status);
    }

    if (search) {
      params = params.set('search', search);
    }

    return this.http.get<MissionListResponse>(this.apiUrl, { params }).pipe(
      map(response => ({
        ...response,
        items: response.items.map(this.mapMissionDates)
      }))
    );
  }

  getMission(id: string): Observable<Mission | undefined> {
    return this.http.get<Mission>(`${this.apiUrl}/${id}`).pipe(
      map(this.mapMissionDates),
      tap(mission => this.currentMission.next(mission)),
      catchError(() => of(undefined))
    );
  }

  createMission(request: CreateMissionRequest): Observable<Mission> {
    const payload = {
      name: request.name,
      description: request.description,
      type: request.type,
      startEpoch: request.startEpoch.toISOString(),
      endEpoch: request.endEpoch?.toISOString()
    };

    return this.http.post<Mission>(this.apiUrl, payload).pipe(
      map(this.mapMissionDates)
    );
  }

  updateMission(id: string, request: UpdateMissionRequest): Observable<Mission> {
    const payload = {
      name: request.name,
      description: request.description,
      startEpoch: request.startEpoch?.toISOString(),
      endEpoch: request.endEpoch?.toISOString()
    };

    return this.http.put<Mission>(`${this.apiUrl}/${id}`, payload).pipe(
      map(this.mapMissionDates),
      tap(mission => this.currentMission.next(mission))
    );
  }

  deleteMission(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  changeStatus(id: string, status: MissionStatus): Observable<Mission> {
    const request: ChangeStatusRequest = { status };
    return this.http.patch<Mission>(`${this.apiUrl}/${id}/status`, request).pipe(
      map(this.mapMissionDates),
      tap(mission => this.currentMission.next(mission))
    );
  }

  cloneMission(id: string): Observable<Mission> {
    return this.http.post<Mission>(`${this.apiUrl}/${id}/clone`, {}).pipe(
      map(this.mapMissionDates)
    );
  }

  exportMission(id: string): Observable<MissionExportData> {
    return this.http.get<MissionExportData>(`${this.apiUrl}/${id}/export`);
  }

  exportMissions(missionIds?: string[], status?: MissionStatus): Observable<any> {
    return this.http.post(`${this.apiUrl}/export`, { missionIds, status });
  }

  importMission(mission: MissionData, overwriteExisting: boolean = false): Observable<any> {
    return this.http.post(`${this.apiUrl}/import`, { mission, overwriteExisting });
  }

  importMissions(missions: MissionData[], overwriteExisting: boolean = false, stopOnError: boolean = true): Observable<any> {
    return this.http.post(`${this.apiUrl}/import/batch`, { missions, overwriteExisting, stopOnError });
  }

  getMissionTree(missionId: string): Observable<MissionTreeNode[]> {
    // Mission tree structure is not stored in backend yet
    // Return default structure for now
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
    ]);
  }

  setCurrentMission(mission: Mission | null): void {
    this.currentMission.next(mission);
  }

  private mapMissionDates = (mission: Mission): Mission => ({
    ...mission,
    startEpoch: new Date(mission.startEpoch),
    endEpoch: mission.endEpoch ? new Date(mission.endEpoch) : undefined,
    createdAt: new Date(mission.createdAt),
    updatedAt: mission.updatedAt ? new Date(mission.updatedAt) : undefined
  });
}
