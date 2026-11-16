import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { AuthResponse, LoginRequest, RegisterRequest, User } from '../models/auth.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly baseUrl = 'http://localhost:5123/api/v1/auth';
  currentUser = signal<User | null>(this.getStoredUser());

  constructor(private http: HttpClient) {}

  register(data: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/register`, data).pipe(
      tap(res => this.handleAuthSuccess(res))
    );
  }

  login(data: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/login`, data).pipe(
      tap(res => this.handleAuthSuccess(res))
    );
  }

  logout(): void {
    localStorage.removeItem('testpilot_token');
    localStorage.removeItem('testpilot_user');
    this.currentUser.set(null);
  }

  isAuthenticated(): boolean {
    return !!localStorage.getItem('testpilot_token');
  }

  private handleAuthSuccess(res: AuthResponse): void {
    localStorage.setItem('testpilot_token', res.token);
    const user: User = { userId: res.userId, name: res.name, email: res.email };
    localStorage.setItem('testpilot_user', JSON.stringify(user));
    this.currentUser.set(user);
  }

  private getStoredUser(): User | null {
    const raw = localStorage.getItem('testpilot_user');
    return raw ? JSON.parse(raw) : null;
  }
}
