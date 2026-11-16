import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ExecuteRequest, ExecuteResponse, TestCase } from '../models/api-request.model';
import { Collection, SavedApiRequest } from '../models/collection.model';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private readonly baseUrl = 'http://localhost:5123/api/v1';

  constructor(private http: HttpClient) {}

  // Request Execution
  executeRequest(req: ExecuteRequest): Observable<ExecuteResponse> {
    return this.http.post<ExecuteResponse>(`${this.baseUrl}/execute`, req);
  }

  // Collections
  getCollections(): Observable<Collection[]> {
    return this.http.get<Collection[]>(`${this.baseUrl}/collections`);
  }

  createCollection(data: { name: string; description?: string }): Observable<Collection> {
    return this.http.post<Collection>(`${this.baseUrl}/collections`, data);
  }

  deleteCollection(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/collections/${id}`);
  }

  // Saved Requests
  saveRequest(data: any): Observable<SavedApiRequest> {
    return this.http.post<SavedApiRequest>(`${this.baseUrl}/requests`, data);
  }

  getSavedRequest(id: string): Observable<SavedApiRequest> {
    return this.http.get<SavedApiRequest>(`${this.baseUrl}/requests/${id}`);
  }

  // Test Runner
  runTests(requestId: string): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/requests/${requestId}/run-tests`, {});
  }

  // Dashboard
  getDashboardSummary(): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/dashboard/summary`);
  }

  // Health
  checkHealth(): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/health`);
  }
}
