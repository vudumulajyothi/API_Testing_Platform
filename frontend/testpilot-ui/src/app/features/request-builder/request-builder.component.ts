import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../../core/services/api.service';
import { ExecuteRequest, ExecuteResponse, HttpMethod, KeyValuePair } from '../../../core/models/api-request.model';

@Component({
  selector: 'app-request-builder',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="request-builder-layout">
      <!-- Top Request Bar -->
      <div class="request-bar">
        <select [(ngModel)]="selectedMethod" class="method-select" [ngClass]="'method-' + selectedMethod.toLowerCase()">
          <option value="GET">GET</option>
          <option value="POST">POST</option>
          <option value="PUT">PUT</option>
          <option value="PATCH">PATCH</option>
          <option value="DELETE">DELETE</option>
        </select>

        <input 
          type="text" 
          [(ngModel)]="requestUrl" 
          placeholder="https://jsonplaceholder.typicode.com/posts/1" 
          class="url-input code-font"
          (keyup.enter)="sendRequest()"
        />

        <button (click)="sendRequest()" [disabled]="isLoading()" class="btn-primary send-btn">
          {{ isLoading() ? 'Sending...' : 'Send' }}
        </button>
      </div>

      <!-- Main Workspace (Split Grid) -->
      <div class="workspace-grid">
        <!-- Request Pane -->
        <div class="pane request-pane">
          <div class="pane-tabs">
            <button [class.active]="activeTab === 'params'" (click)="activeTab = 'params'">Params ({{ queryParams.length }})</button>
            <button [class.active]="activeTab === 'headers'" (click)="activeTab = 'headers'">Headers ({{ headers.length }})</button>
            <button [class.active]="activeTab === 'body'" (click)="activeTab = 'body'">Body</button>
          </div>

          <div class="pane-content">
            <!-- Params Tab -->
            @if (activeTab === 'params') {
              <div class="key-value-editor">
                @for (param of queryParams; track $index) {
                  <div class="kv-row">
                    <input type="text" [(ngModel)]="param.key" placeholder="Key" />
                    <input type="text" [(ngModel)]="param.value" placeholder="Value" />
                    <button (click)="removeParam($index)" class="btn-icon">×</button>
                  </div>
                }
                <button (click)="addParam()" class="btn-secondary btn-sm">+ Add Parameter</button>
              </div>
            }

            <!-- Headers Tab -->
            @if (activeTab === 'headers') {
              <div class="key-value-editor">
                @for (header of headers; track $index) {
                  <div class="kv-row">
                    <input type="text" [(ngModel)]="header.key" placeholder="Header Name" />
                    <input type="text" [(ngModel)]="header.value" placeholder="Header Value" />
                    <button (click)="removeHeader($index)" class="btn-icon">×</button>
                  </div>
                }
                <button (click)="addHeader()" class="btn-secondary btn-sm">+ Add Header</button>
              </div>
            }

            <!-- Body Tab -->
            @if (activeTab === 'body') {
              <div class="body-editor">
                <textarea 
                  [(ngModel)]="requestBody" 
                  placeholder='{\n  "title": "foo",\n  "body": "bar",\n  "userId": 1\n}' 
                  class="code-font json-textarea"
                ></textarea>
              </div>
            }
          </div>
        </div>

        <!-- Response Pane -->
        <div class="pane response-pane">
          <div class="response-header">
            <span class="pane-title">Response</span>
            @if (response(); as res) {
              <div class="response-meta">
                <span class="status-badge" [class.status-2xx]="res.statusCode >= 200 && res.statusCode < 300" [class.status-err]="res.statusCode >= 400">
                  {{ res.statusCode }} {{ res.statusText }}
                </span>
                <span class="meta-item">{{ res.responseTimeMs }} ms</span>
                <span class="meta-item">{{ res.contentLength }} B</span>
              </div>
            }
          </div>

          <div class="response-content">
            @if (isLoading()) {
              <div class="empty-state">
                <p>Executing request...</p>
              </div>
            } @else if (response(); as res) {
              <pre class="response-body code-font">{{ res.body | json }}</pre>
            } @else {
              <div class="empty-state">
                <p>Enter an endpoint URL and hit <strong>Send</strong> to view results.</p>
              </div>
            }
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .request-builder-layout {
      height: calc(100vh - 54px);
      display: flex;
      flex-direction: column;
      background-color: var(--bg-darker);
    }
    .request-bar {
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 12px 20px;
      background-color: var(--bg-dark);
      border-bottom: 1px solid var(--border-color);
    }
    .method-select {
      font-weight: 700;
      width: 110px;
      &.method-get { color: var(--method-get); }
      &.method-post { color: var(--method-post); }
      &.method-put { color: var(--method-put); }
      &.method-delete { color: var(--method-delete); }
    }
    .url-input {
      flex: 1;
      font-size: 13px;
    }
    .send-btn {
      padding: 8px 24px;
    }
    .workspace-grid {
      flex: 1;
      display: grid;
      grid-template-columns: 1fr 1fr;
      overflow: hidden;
    }
    .pane {
      display: flex;
      flex-direction: column;
      border-right: 1px solid var(--border-color);
      overflow: hidden;
    }
    .pane-tabs {
      display: flex;
      gap: 2px;
      padding: 8px 16px 0;
      background-color: var(--bg-dark);
      border-bottom: 1px solid var(--border-color);
      button {
        background: transparent;
        color: var(--text-secondary);
        border-radius: 4px 4px 0 0;
        padding: 6px 14px;
        font-size: 13px;
        &.active {
          color: var(--text-primary);
          border-bottom: 2px solid var(--color-primary);
          font-weight: 600;
        }
      }
    }
    .pane-content {
      flex: 1;
      padding: 16px;
      overflow-y: auto;
    }
    .key-value-editor {
      display: flex;
      flex-direction: column;
      gap: 8px;
    }
    .kv-row {
      display: flex;
      gap: 8px;
      input { flex: 1; }
      .btn-icon {
        background: transparent;
        color: var(--text-secondary);
        font-size: 18px;
        padding: 0 10px;
        &:hover { color: var(--color-danger); }
      }
    }
    .body-editor {
      height: 100%;
      .json-textarea {
        width: 100%;
        height: 300px;
        resize: vertical;
        font-size: 13px;
        line-height: 1.4;
      }
    }
    .response-pane {
      background-color: var(--bg-dark);
    }
    .response-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 10px 16px;
      border-bottom: 1px solid var(--border-color);
      .pane-title { font-size: 13px; font-weight: 600; color: var(--text-secondary); }
      .response-meta {
        display: flex;
        gap: 12px;
        align-items: center;
        font-size: 12px;
        font-family: var(--font-code);
      }
      .status-badge {
        padding: 2px 8px;
        border-radius: 4px;
        font-weight: 600;
        &.status-2xx { background-color: rgba(35, 134, 54, 0.2); color: var(--color-success-light); }
        &.status-err { background-color: rgba(218, 54, 51, 0.2); color: var(--color-danger); }
      }
    }
    .response-content {
      flex: 1;
      padding: 16px;
      overflow-y: auto;
    }
    .response-body {
      white-space: pre-wrap;
      word-break: break-all;
      font-size: 12px;
      color: #c9d1d9;
    }
    .empty-state {
      height: 100%;
      display: flex;
      justify-content: center;
      align-items: center;
      color: var(--text-muted);
      font-size: 13px;
    }
  `]
})
export class RequestBuilderComponent {
  private api = inject(ApiService);

  selectedMethod: HttpMethod = 'GET';
  requestUrl = 'https://jsonplaceholder.typicode.com/posts/1';
  requestBody = '';
  activeTab: 'params' | 'headers' | 'body' = 'params';

  queryParams: KeyValuePair[] = [];
  headers: KeyValuePair[] = [
    { key: 'Accept', value: 'application/json' }
  ];

  isLoading = signal(false);
  response = signal<ExecuteResponse | null>(null);

  addParam(): void {
    this.queryParams.push({ key: '', value: '' });
  }

  removeParam(index: number): void {
    this.queryParams.splice(index, 1);
  }

  addHeader(): void {
    this.headers.push({ key: '', value: '' });
  }

  removeHeader(index: number): void {
    this.headers.splice(index, 1);
  }

  sendRequest(): void {
    if (!this.requestUrl.trim()) return;

    this.isLoading.set(true);

    const payload: ExecuteRequest = {
      method: this.selectedMethod,
      url: this.requestUrl,
      headers: this.headers.filter(h => h.key.trim().length > 0),
      queryParams: this.queryParams.filter(p => p.key.trim().length > 0),
      body: this.requestBody.trim() ? this.requestBody : null
    };

    this.api.executeRequest(payload).subscribe({
      next: (res) => {
        this.response.set(res);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);
        this.response.set({
          statusCode: err.status || 500,
          statusText: err.statusText || 'Error',
          responseTimeMs: 0,
          contentLength: 0,
          headers: {},
          body: err.error || { message: err.message },
          isSuccessStatusCode: false
        });
      }
    });
  }
}
