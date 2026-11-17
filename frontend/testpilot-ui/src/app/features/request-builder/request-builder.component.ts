import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { ExecuteRequest, ExecuteResponse, HttpMethod, KeyValuePair } from '../../core/models/api-request.model';
import { Collection, SavedApiRequest } from '../../core/models/collection.model';

@Component({
  selector: 'app-request-builder',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  template: `
    <div class="request-builder-layout">
      <!-- Left Sidebar: Collections & Saved Requests -->
      <aside class="sidebar">
        <div class="sidebar-header">
          <span class="sidebar-title">Collections</span>
          <button (click)="loadCollections()" class="btn-icon" title="Refresh">↻</button>
        </div>

        <div class="collections-tree">
          @for (coll of collections(); track coll.collectionId) {
            <div class="collection-item">
              <div class="coll-row" (click)="toggleCollection(coll.collectionId)">
                <span class="coll-arrow">{{ expandedCollections.has(coll.collectionId) ? '▼' : '▶' }}</span>
                <span class="coll-icon">📁</span>
                <span class="coll-name">{{ coll.name }}</span>
              </div>

              @if (expandedCollections.has(coll.collectionId)) {
                <div class="sub-requests">
                  @for (req of getRequestsForCollection(coll.collectionId); track req.requestId) {
                    <div class="request-row" (click)="loadRequestIntoBuilder(req)">
                      <span class="badge-mini" [ngClass]="'method-' + req.method.toLowerCase()">{{ req.method }}</span>
                      <span class="req-title">{{ req.name }}</span>
                    </div>
                  } @empty {
                    <div class="no-reqs">No saved requests</div>
                  }
                </div>
              }
            </div>
          } @empty {
            <div class="empty-sidebar">
              <p>No collections found.</p>
              <a routerLink="/collections" class="btn-sm-link">+ Add Collection</a>
            </div>
          }
        </div>
      </aside>

      <!-- Main Workspace -->
      <div class="main-editor">
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

          <button (click)="openSaveModal()" class="btn-secondary save-btn">
            Save
          </button>
        </div>

        <!-- Split Workspace Grid -->
        <div class="workspace-grid">
          <!-- Request Pane -->
          <div class="pane request-pane">
            <div class="pane-tabs">
              <button [class.active]="activeTab === 'params'" (click)="activeTab = 'params'">Params ({{ queryParams.length }})</button>
              <button [class.active]="activeTab === 'headers'" (click)="activeTab = 'headers'">Headers ({{ headers.length }})</button>
              <button [class.active]="activeTab === 'body'" (click)="activeTab = 'body'">Body</button>
            </div>

            <div class="pane-content">
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
                  <p>Executing HTTP request...</p>
                </div>
              } @else if (response()) {
                <pre class="response-body code-font">{{ response()?.body | json }}</pre>
              } @else {
                <div class="empty-state">
                  <p>Enter an endpoint URL and hit <strong>Send</strong> to view results.</p>
                </div>
              }
            </div>
          </div>
        </div>
      </div>

      <!-- Save Request Modal -->
      @if (showSaveModal) {
        <div class="modal-backdrop">
          <div class="modal-card">
            <h3>Save API Request</h3>
            <div class="form-group">
              <label>Request Name</label>
              <input type="text" [(ngModel)]="saveRequestName" placeholder="e.g. Fetch Single Post" />
            </div>
            <div class="form-group">
              <label>Select Collection</label>
              <select [(ngModel)]="saveCollectionId">
                <option value="" disabled>Choose a collection...</option>
                @for (c of collections(); track c.collectionId) {
                  <option [value]="c.collectionId">{{ c.name }}</option>
                }
              </select>
            </div>
            <div class="modal-actions">
              <button (click)="showSaveModal = false" class="btn-secondary">Cancel</button>
              <button (click)="saveRequest()" [disabled]="!saveRequestName.trim() || !saveCollectionId" class="btn-primary">Save Request</button>
            </div>
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    .request-builder-layout {
      height: calc(100vh - 54px);
      display: flex;
      background-color: var(--bg-darker);
    }
    .sidebar {
      width: 260px;
      background-color: var(--bg-dark);
      border-right: 1px solid var(--border-color);
      display: flex;
      flex-direction: column;
    }
    .sidebar-header {
      padding: 12px 16px;
      display: flex;
      justify-content: space-between;
      align-items: center;
      border-bottom: 1px solid var(--border-color);
      .sidebar-title { font-size: 12px; font-weight: 700; color: var(--text-secondary); text-transform: uppercase; letter-spacing: 0.5px; }
      .btn-icon { background: none; border: none; color: var(--text-secondary); cursor: pointer; font-size: 14px; }
    }
    .collections-tree {
      flex: 1;
      overflow-y: auto;
      padding: 8px 0;
    }
    .coll-row {
      display: flex;
      align-items: center;
      gap: 6px;
      padding: 6px 12px;
      cursor: pointer;
      font-size: 13px;
      &:hover { background-color: var(--bg-surface); }
      .coll-arrow { font-size: 9px; color: var(--text-muted); width: 12px; }
      .coll-icon { font-size: 14px; }
      .coll-name { font-weight: 500; color: var(--text-primary); }
    }
    .sub-requests {
      padding-left: 24px;
    }
    .request-row {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 5px 8px;
      cursor: pointer;
      font-size: 12px;
      border-radius: 4px;
      &:hover { background-color: var(--bg-surface-hover); }
      .badge-mini {
        font-size: 9px;
        font-weight: 700;
        font-family: var(--font-code);
        &.method-get { color: var(--method-get); }
        &.method-post { color: var(--method-post); }
        &.method-put { color: var(--method-put); }
        &.method-delete { color: var(--method-delete); }
      }
      .req-title { color: var(--text-secondary); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    }
    .no-reqs, .empty-sidebar {
      padding: 12px 16px;
      font-size: 12px;
      color: var(--text-muted);
    }
    .main-editor {
      flex: 1;
      display: flex;
      flex-direction: column;
      overflow: hidden;
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
    .url-input { flex: 1; font-size: 13px; }
    .send-btn { padding: 8px 24px; }
    .save-btn { padding: 8px 16px; }
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
    .pane-content { flex: 1; padding: 16px; overflow-y: auto; }
    .key-value-editor { display: flex; flex-direction: column; gap: 8px; }
    .kv-row {
      display: flex; gap: 8px; input { flex: 1; }
      .btn-icon { background: transparent; color: var(--text-secondary); font-size: 18px; padding: 0 10px; &:hover { color: var(--color-danger); } }
    }
    .body-editor { height: 100%; .json-textarea { width: 100%; height: 300px; resize: vertical; font-size: 13px; line-height: 1.4; } }
    .response-pane { background-color: var(--bg-dark); }
    .response-header {
      display: flex; justify-content: space-between; align-items: center; padding: 10px 16px; border-bottom: 1px solid var(--border-color);
      .pane-title { font-size: 13px; font-weight: 600; color: var(--text-secondary); }
      .response-meta { display: flex; gap: 12px; align-items: center; font-size: 12px; font-family: var(--font-code); }
      .status-badge { padding: 2px 8px; border-radius: 4px; font-weight: 600; &.status-2xx { background-color: rgba(35, 134, 54, 0.2); color: var(--color-success-light); } &.status-err { background-color: rgba(218, 54, 51, 0.2); color: var(--color-danger); } }
    }
    .response-content { flex: 1; padding: 16px; overflow-y: auto; }
    .response-body { white-space: pre-wrap; word-break: break-all; font-size: 12px; color: #c9d1d9; }
    .empty-state { height: 100%; display: flex; justify-content: center; align-items: center; color: var(--text-muted); font-size: 13px; }
    .modal-backdrop { position: fixed; inset: 0; background: rgba(0,0,0,0.7); display: flex; align-items: center; justify-content: center; z-index: 100; }
    .modal-card { background: var(--bg-dark); border: 1px solid var(--border-color); border-radius: 8px; padding: 24px; width: 420px; h3 { margin-bottom: 16px; } .form-group { margin-bottom: 14px; display: flex; flex-direction: column; gap: 6px; } .modal-actions { display: flex; justify-content: flex-end; gap: 10px; margin-top: 20px; } }
  `]
})
export class RequestBuilderComponent implements OnInit {
  private api = inject(ApiService);

  collections = signal<Collection[]>([]);
  expandedCollections = new Set<string>();
  savedRequestsMap = new Map<string, SavedApiRequest[]>();

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

  // Save Modal
  showSaveModal = false;
  saveRequestName = '';
  saveCollectionId = '';

  ngOnInit(): void {
    this.loadCollections();
  }

  loadCollections(): void {
    this.api.getCollections().subscribe({
      next: (colls) => this.collections.set(colls),
      error: () => {
        // Fallback demo collections
        const demo: Collection[] = [
          { collectionId: 'coll-1', name: 'Sample API Tests', description: 'Public demo endpoints', createdAt: new Date().toISOString(), requestCount: 2 }
        ];
        this.collections.set(demo);
        this.savedRequestsMap.set('coll-1', [
          { requestId: 'req-1', collectionId: 'coll-1', name: 'Get Post 1', url: 'https://jsonplaceholder.typicode.com/posts/1', method: 'GET', headers: [{ key: 'Accept', value: 'application/json' }], queryParams: [], createdAt: '' },
          { requestId: 'req-2', collectionId: 'coll-1', name: 'Create Post', url: 'https://jsonplaceholder.typicode.com/posts', method: 'POST', body: '{"title":"Test"}', headers: [{ key: 'Content-Type', value: 'application/json' }], queryParams: [], createdAt: '' }
        ]);
        this.expandedCollections.add('coll-1');
      }
    });
  }

  toggleCollection(id: string): void {
    if (this.expandedCollections.has(id)) {
      this.expandedCollections.delete(id);
    } else {
      this.expandedCollections.add(id);
    }
  }

  getRequestsForCollection(id: string): SavedApiRequest[] {
    return this.savedRequestsMap.get(id) || [];
  }

  loadRequestIntoBuilder(req: SavedApiRequest): void {
    this.selectedMethod = req.method as HttpMethod;
    this.requestUrl = req.url;
    this.requestBody = req.body || '';
    this.headers = [...(req.headers || [])];
    this.queryParams = [...(req.queryParams || [])];
  }

  addParam(): void { this.queryParams.push({ key: '', value: '' }); }
  removeParam(i: number): void { this.queryParams.splice(i, 1); }
  addHeader(): void { this.headers.push({ key: '', value: '' }); }
  removeHeader(i: number): void { this.headers.splice(i, 1); }

  openSaveModal(): void {
    this.saveRequestName = 'Request ' + (this.requestUrl.split('/').pop() || 'Endpoint');
    this.saveCollectionId = this.collections().length > 0 ? this.collections()[0].collectionId : '';
    this.showSaveModal = true;
  }

  saveRequest(): void {
    if (!this.saveRequestName.trim() || !this.saveCollectionId) return;

    const payload = {
      collectionId: this.saveCollectionId,
      name: this.saveRequestName.trim(),
      url: this.requestUrl.trim(),
      method: this.selectedMethod,
      body: this.requestBody.trim() ? this.requestBody : null,
      headers: this.headers.filter(h => h.key.trim().length > 0),
      queryParams: this.queryParams.filter(p => p.key.trim().length > 0)
    };

    this.api.saveRequest(payload).subscribe({
      next: (saved) => {
        const existing = this.savedRequestsMap.get(this.saveCollectionId) || [];
        this.savedRequestsMap.set(this.saveCollectionId, [...existing, saved]);
        this.showSaveModal = false;
      },
      error: () => {
        const dummy: SavedApiRequest = {
          requestId: Math.random().toString(),
          collectionId: this.saveCollectionId,
          name: this.saveRequestName,
          url: this.requestUrl,
          method: this.selectedMethod,
          body: this.requestBody,
          headers: this.headers,
          queryParams: this.queryParams,
          createdAt: new Date().toISOString()
        };
        const existing = this.savedRequestsMap.get(this.saveCollectionId) || [];
        this.savedRequestsMap.set(this.saveCollectionId, [...existing, dummy]);
        this.showSaveModal = false;
      }
    });
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
