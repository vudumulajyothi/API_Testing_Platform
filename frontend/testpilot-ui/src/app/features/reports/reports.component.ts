import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="reports-page">
      <div class="page-header">
        <h1>Quality & Compliance Reports</h1>
        <p class="subtitle">Summary analytics on API test coverage, assertion pass rates, and latency distribution</p>
      </div>

      <div class="reports-card">
        <h3>Test Execution Distribution</h3>
        <div class="progress-bar-container">
          <div class="progress-bar pass" style="width: 92%">92% Passed</div>
          <div class="progress-bar fail" style="width: 8%">8% Failed</div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .reports-page { padding: 30px; max-width: 1200px; margin: 0 auto; }
    .page-header { margin-bottom: 24px; h1 { font-size: 24px; font-weight: 700; } .subtitle { font-size: 14px; color: var(--text-secondary); margin-top: 4px; } }
    .reports-card {
      background: var(--bg-dark); border: 1px solid var(--border-color); border-radius: 8px; padding: 24px;
      h3 { margin-bottom: 16px; font-size: 16px; }
    }
    .progress-bar-container {
      display: flex; height: 32px; border-radius: 6px; overflow: hidden; font-size: 12px; font-weight: 600; text-align: center; line-height: 32px;
      .pass { background-color: var(--color-success); color: white; }
      .fail { background-color: var(--color-danger); color: white; }
    }
  `]
})
export class ReportsComponent {}
