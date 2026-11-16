import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-history',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="history-page">
      <div class="page-header">
        <h1>Execution History</h1>
        <p class="subtitle">Complete audit trail of all manual and automated API executions</p>
      </div>

      <div class="history-list">
        @for (item of history(); track item.id) {
          <div class="history-card">
            <div class="method-tag" [ngClass]="'method-' + item.method.toLowerCase()">{{ item.method }}</div>
            <div class="url-info">
              <span class="url code-font">{{ item.url }}</span>
              <span class="time">{{ item.timestamp }}</span>
            </div>
            <div class="status-info">
              <span class="badge" [class.success]="item.statusCode >= 200 && item.statusCode < 300">{{ item.statusCode }}</span>
              <span class="duration code-font">{{ item.duration }}ms</span>
            </div>
          </div>
        }
      </div>
    </div>
  `,
  styles: [`
    .history-page { padding: 30px; max-width: 1200px; margin: 0 auto; }
    .page-header { margin-bottom: 24px; h1 { font-size: 24px; font-weight: 700; } .subtitle { font-size: 14px; color: var(--text-secondary); margin-top: 4px; } }
    .history-list { display: flex; flex-direction: column; gap: 12px; }
    .history-card {
      background: var(--bg-dark); border: 1px solid var(--border-color); border-radius: 8px;
      padding: 14px 20px; display: flex; align-items: center; gap: 16px;
      .method-tag { font-weight: 700; font-size: 12px; width: 60px; }
      .method-get { color: var(--method-get); }
      .method-post { color: var(--method-post); }
      .method-delete { color: var(--method-delete); }
      .url-info { flex: 1; display: flex; flex-direction: column; gap: 2px; .url { font-size: 13px; color: var(--text-primary); } .time { font-size: 11px; color: var(--text-muted); } }
      .status-info { display: flex; align-items: center; gap: 12px; .badge { font-weight: 600; font-size: 12px; color: var(--color-danger); &.success { color: var(--color-success-light); } } .duration { font-size: 12px; color: var(--text-secondary); } }
    }
  `]
})
export class HistoryComponent {
  history = signal([
    { id: '1', method: 'GET', url: 'https://jsonplaceholder.typicode.com/posts/1', statusCode: 200, duration: 142, timestamp: 'Today at 21:40' },
    { id: '2', method: 'POST', url: 'https://jsonplaceholder.typicode.com/posts', statusCode: 201, duration: 215, timestamp: 'Today at 21:35' },
    { id: '3', method: 'DELETE', url: 'https://jsonplaceholder.typicode.com/posts/1', statusCode: 200, duration: 110, timestamp: 'Today at 21:12' }
  ]);
}
