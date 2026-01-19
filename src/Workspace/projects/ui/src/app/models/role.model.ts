export interface Role {
  id: string;
  name: string;
  description?: string;
  isSystem: boolean;
  createdAt: Date;
  userCount: number;
  permissionCount: number;
}

export interface RoleDetail extends Role {
  permissions: PermissionSummary[];
}

export interface PermissionSummary {
  id: string;
  name: string;
  resource: string;
  action: string;
  description?: string;
}

export interface UserSummary {
  id: string;
  email: string;
  displayName?: string;
  isActive: boolean;
}

export interface CreateRoleRequest {
  name: string;
  description?: string;
  permissionIds?: string[];
}

export interface UpdateRoleRequest {
  name: string;
  description?: string;
}

export interface UpdateRolePermissionsRequest {
  permissionIds: string[];
}
