export interface Permission {
  id: string;
  name: string;
  resource: string;
  action: string;
  description?: string;
  roles: RoleSummary[];
}

export interface RoleSummary {
  id: string;
  name: string;
  isSystem: boolean;
}

export interface PermissionGroup {
  resource: string;
  icon: string;
  permissions: Permission[];
}
