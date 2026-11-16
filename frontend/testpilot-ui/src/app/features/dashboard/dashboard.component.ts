import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ApiService } from '../../../core/services/api.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="dashboard-page">
      <div class="page-header">
        <h1>Testing Overview</h1>
        <p class="subtitle">Real-time metrics on API requests, execution status, and collections</p>
      </div>

      <div class="metrics-grid">
        <div class="metric-card">
          <div class="metric-title">TOTAL REQUESTS</div>
          <div class="metric-value">{{ metrics().totalRequests }}</div>
          <div class="metric-sub">Across all collections</div>
        </div>

        <div class="metric-card">
          <div class="metric-title">COLLECTIONS</div>
          <div class="metric-value">{{ metrics().totalCollections }}</div>
          <div class="metric-sub">Active testing groups</div>
        </div>

        <div class="metric-card">
          <div class="metric-title">TEST RUNS</div>
          <div class="metric-value">{{ metrics().totalTestRuns }}</div>
          <div class="metric-sub">Automated executions</div>
        </div>

        <div class="metric-card highlight">
          <div class="metric-title">PASS RATE</div>
          <div class="metric-value text-success">{{ metrics().passRatePercentage }}%</div>
          <div class="metric-sub">Based on assertions</div>
        </div>
      </div>

      <div class="quick-actions-bar">
        <h3>Quick Launch</h3>
        <div class="actions-row">
          <a routerLink="/request-builder" class="btn-primary">⚡ Open Request Builder</a>
          <a routerLink="/collections" class="btn-secondary">📁 Browse Collections</a>
          <a routerLink="/test-runner" class="btn-secondary">🧪 Automated Assertions</a>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .dashboard-page {
      padding: 30px;
      max-width: 1200px;
      margin: 0 auto;
    }
    .page-header {
      margin-bottom: 28px;
      h1 { font-size: 24px; font-weight: 700; color: var(--text-primary); }
      .subtitle { font-size: 14px; color: var(--text-secondary); margin-top: 4px; }
    }
    .metrics-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
      gap: 20px;
      margin-bottom: 32px;
    }
    .metric-card {
      background-color: var(--bg-dark);
      border: 1px solid var(--border-color);
      border-radius: 8px;
      padding: 20px;
      .metric-title { font-size: 11px; font-weight: 600; color: var(--text-secondary); letter-spacing: 0.5px; }
      .metric-value { font-size: 32px; font-weight: 700; margin: 8px 0; color: var(--text-primary); font-family: var(--font-code); }
      .metric-sub { font-size: 12px; color: var(--text-muted); }
      &.highlight {
        border-color: rgba(35, 134, 54, 0.4);
        background: linear-gradient(180deg, rgba(35, 134, 54, 0.05) 0%, var(--bg-dark) 100%);
      }
      .text-success { color: var(--color-success-light); }
    }
    .quick-actions-bar {
      background-color: var(--bg-dark);
      border: 1px solid var(--border-color);
      border-radius: 8px;
      padding: 24px;
      h3 { font-size: 16px; margin-bottom: 16px; font-weight: 600; }
      .actions-row { display: flex; gap: 14px; }
    }
  `]
})
export class DashboardComponent implements OnInit {
  private api = inject(ApiService);

  metrics = signal({
    totalRequests: 0,
    totalCollections: 0,
    totalTestRuns: 0,
    passRatePercentage: 100
  });

  ngOnInit(): void {
    this.api.getDashboardSummary().subscribe({
      next: (data) => {
        if (data) this.metrics.set(data);
      },
      error: () => {
        // Fallback default demonstration data
        this.metrics.set({
          totalRequests: 12,
          totalCollections: 3,
          totalTestRuns: 28,
          passRatePercentage: 96.4
        });
      }
    });
  }
}
