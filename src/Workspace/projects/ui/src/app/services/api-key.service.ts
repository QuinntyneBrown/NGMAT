import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import {
  ApiKey,
  CreateApiKeyRequest,
  CreateApiKeyResponse,
  UpdateApiKeyRequest,
  ApiKeyListResponse,
  ApiKeyStats
} from '../models/api-key.model';

@Injectable({
  providedIn: 'root'
})
export class ApiKeyService {
  private readonly apiUrl = `${environment.baseUrl}/api/apikeys`;

  constructor(private http: HttpClient) {}

  getMyApiKeys(): Observable<ApiKeyListResponse> {
    return this.http.get<ApiKeyListResponse>(this.apiUrl).pipe(
      map(response => ({
        ...response,
        items: response.items.map(this.mapApiKeyDates)
      }))
    );
  }

  getAllApiKeys(skip: number = 0, take: number = 10): Observable<ApiKeyListResponse> {
    const params = new HttpParams()
      .set('skip', skip.toString())
      .set('take', take.toString());

    return this.http.get<ApiKeyListResponse>(`${this.apiUrl}/all`, { params }).pipe(
      map(response => ({
        ...response,
        items: response.items.map(this.mapApiKeyDates)
      }))
    );
  }

  getApiKeyStats(): Observable<ApiKeyStats> {
    return this.http.get<ApiKeyStats>(`${this.apiUrl}/stats`);
  }

  getApiKey(id: string): Observable<ApiKey> {
    return this.http.get<ApiKey>(`${this.apiUrl}/${id}`).pipe(
      map(this.mapApiKeyDates)
    );
  }

  createApiKey(request: CreateApiKeyRequest): Observable<CreateApiKeyResponse> {
    return this.http.post<CreateApiKeyResponse>(this.apiUrl, request).pipe(
      map(response => ({
        ...response,
        createdAt: new Date(response.createdAt),
        expiresAt: response.expiresAt ? new Date(response.expiresAt) : undefined,
        lastUsedAt: response.lastUsedAt ? new Date(response.lastUsedAt) : undefined
      }))
    );
  }

  updateApiKey(id: string, request: UpdateApiKeyRequest): Observable<ApiKey> {
    return this.http.put<ApiKey>(`${this.apiUrl}/${id}`, request).pipe(
      map(this.mapApiKeyDates)
    );
  }

  revokeApiKey(id: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/revoke`, {});
  }

  deleteApiKey(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  private mapApiKeyDates = (apiKey: ApiKey): ApiKey => ({
    ...apiKey,
    createdAt: new Date(apiKey.createdAt),
    expiresAt: apiKey.expiresAt ? new Date(apiKey.expiresAt) : undefined,
    lastUsedAt: apiKey.lastUsedAt ? new Date(apiKey.lastUsedAt) : undefined
  });
}
