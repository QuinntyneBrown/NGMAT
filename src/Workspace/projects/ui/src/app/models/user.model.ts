export type UserStatus = 'active' | 'inactive' | 'locked';

export interface User {
  id: string;
  email: string;
  displayName?: string;
  isEmailVerified: boolean;
  isActive: boolean;
  isMfaEnabled: boolean;
  isLockedOut: boolean;
  createdAt: Date;
  lastLoginAt?: Date;
  roles: string[];
}

export interface UserDetail extends User {
  failedLoginAttempts: number;
  lockoutEndAt?: Date;
  activeSessions: number;
  roles: UserRole[];
}

export interface UserRole {
  id: string;
  name: string;
  assignedAt: Date;
}

export interface UserListResponse {
  items: User[];
  totalCount: number;
  skip: number;
  take: number;
}

export interface UserStats {
  totalUsers: number;
  activeUsers: number;
  inactiveUsers: number;
  lockedUsers: number;
}

export interface CreateUserRequest {
  email: string;
  password: string;
  displayName?: string;
  roleIds?: string[];
}

export interface UpdateUserRequest {
  displayName?: string;
}

export interface UpdateUserRolesRequest {
  roleIds: string[];
}

export interface ResetPasswordResponse {
  userId: string;
  email: string;
  temporaryPassword: string;
  message: string;
}
