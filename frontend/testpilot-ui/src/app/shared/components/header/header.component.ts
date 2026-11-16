import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <header class="app-header">
      <div class="logo-area">
        <span class="logo-icon">⚡</span>
        <span class="logo-text">TestPilot</span>
        <span class="badge">v1.0</span>
      </div>

      <div class="center-nav">
        <a routerLink="/dashboard" routerLinkActive="active" class="nav-item">Dashboard</a>
        <a routerLink="/request-builder" routerLinkActive="active" class="nav-item">Request Builder</a>
        <a routerLink="/collections" routerLinkActive="active" class="nav-item">Collections</a>
        <a routerLink="/test-runner" routerLinkActive="active" class="nav-item">Test Runner</a>
        <a routerLink="/history" routerLinkActive="active" class="nav-item">History</a>
        <a routerLink="/reports" routerLinkActive="active" class="nav-item">Reports</a>
      </div>

      <div class="user-area">
        @if (authService.currentUser(); as user) {
          <span class="user-name">{{ user.name }}</span>
          <button (click)="logout()" class="btn-secondary btn-sm">Logout</button>
        } @else {
          <a routerLink="/auth/login" class="btn-primary btn-sm">Login</a>
        }
      </div>
    </header>
  `,
  styles: [`
    .app-header {
      height: 54px;
      background-color: var(--bg-dark);
      border-bottom: 1px solid var(--border-color);
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 0 20px;
      user-select: none;
    }
    .logo-area {
      display: flex;
      align-items: center;
      gap: 8px;
      .logo-icon { font-size: 20px; }
      .logo-text { font-weight: 700; font-size: 16px; color: var(--text-primary); letter-spacing: -0.5px; }
      .badge {
        font-size: 10px;
        background-color: var(--bg-surface);
        padding: 2px 6px;
        border-radius: 4px;
        color: var(--color-primary);
        font-family: var(--font-code);
      }
    }
    .center-nav {
      display: flex;
      align-items: center;
      gap: 12px;
      .nav-item {
        color: var(--text-secondary);
        font-size: 13px;
        padding: 6px 10px;
        border-radius: 6px;
        transition: all 0.15s ease;
        &.active, &:hover {
          color: var(--text-primary);
          background-color: var(--bg-surface);
        }
      }
    }
    .user-area {
      display: flex;
      align-items: center;
      gap: 12px;
      .user-name { font-size: 13px; color: var(--text-secondary); }
      .btn-sm { padding: 4px 10px; font-size: 12px; }
    }
  `]
})
export class HeaderComponent {
  authService = inject(AuthService);
  private router = inject(Router);

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/auth/login']);
  }
}
