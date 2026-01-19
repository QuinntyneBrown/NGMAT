export type ApiKeyStatus = 'Active' | 'Expired' | 'Revoked';

export interface ApiKey {
  id: string;
  userId: string;
  userEmail?: string;
  name: string;
  keyPrefix: string;
  scopes: string[];
  createdAt: Date;
  expiresAt?: Date;
  lastUsedAt?: Date;
  isActive: boolean;
  status: ApiKeyStatus;
}

export interface CreateApiKeyRequest {
  name: string;
  prefix: string;
  scopes?: string[];
  expiresInDays?: number;
}

export interface CreateApiKeyResponse extends ApiKey {
  key: string;
}

export interface UpdateApiKeyRequest {
  name: string;
  scopes?: string[];
}

export interface ApiKeyListResponse {
  items: ApiKey[];
  totalCount: number;
  skip: number;
  take: number;
}

export interface ApiKeyStats {
  totalKeys: number;
  activeKeys: number;
  expiredOrRevokedKeys: number;
}
