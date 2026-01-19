export interface Mission {
  id: string;
  name: string;
  description?: string;
  type: MissionType;
  status: MissionStatus;
  startEpoch: Date;
  endEpoch?: Date;
  ownerId: string;
  createdAt: Date;
  updatedAt?: Date;
}

export enum MissionType {
  LEO = 'LEO',
  MEO = 'MEO',
  GEO = 'GEO',
  HEO = 'HEO',
  Lunar = 'Lunar',
  Interplanetary = 'Interplanetary',
  Other = 'Other'
}

export enum MissionStatus {
  Draft = 'Draft',
  Active = 'Active',
  Completed = 'Completed',
  Archived = 'Archived'
}

export interface MissionListResponse {
  items: Mission[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface CreateMissionRequest {
  name: string;
  description?: string;
  type: MissionType;
  startEpoch: Date;
  endEpoch?: Date;
}

export interface UpdateMissionRequest {
  name?: string;
  description?: string;
  startEpoch?: Date;
  endEpoch?: Date;
}

export interface MissionTreeNode {
  id: string;
  label: string;
  icon: string;
  type: 'mission' | 'spacecraft' | 'propagator' | 'maneuver' | 'forceModel' | 'report';
  children?: MissionTreeNode[];
  expanded?: boolean;
}
