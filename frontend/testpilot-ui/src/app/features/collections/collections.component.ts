import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../../core/services/api.service';
import { Collection } from '../../../core/models/collection.model';

@Component({
  selector: 'app-collections',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="collections-page">
      <div class="page-header">
        <div>
          <h1>API Collections</h1>
          <p class="subtitle">Organize and group API requests for automated test suites</p>
        </div>
        <button (click)="showCreateModal = true" class="btn-primary">+ New Collection</button>
      </div>

      <!-- Create Modal -->
      @if (showCreateModal) {
        <div class="modal-backdrop">
          <div class="modal-card">
            <h3>Create New Collection</h3>
            <div class="form-group">
              <label>Collection Name</label>
              <input type="text" [(ngModel)]="newCollectionName" placeholder="e.g. Payments API Suite" />
            </div>
            <div class="form-group">
              <label>Description (Optional)</label>
              <input type="text" [(ngModel)]="newCollectionDesc" placeholder="e.g. End-to-end payment workflows" />
            </div>
            <div class="modal-actions">
              <button (click)="showCreateModal = false" class="btn-secondary">Cancel</button>
              <button (click)="createCollection()" [disabled]="!newCollectionName.trim()" class="btn-primary">Create</button>
            </div>
          </div>
        </div>
      }

      <div class="collections-grid">
        @for (coll of collections(); track coll.collectionId) {
          <div class="collection-card">
            <div class="card-header">
              <div class="coll-icon">📁</div>
              <div class="coll-info">
                <h3>{{ coll.name }}</h3>
                <p>{{ coll.description || 'No description provided' }}</p>
              </div>
            </div>
            <div class="card-footer">
              <span class="request-count">{{ coll.requestCount }} Requests</span>
              <button (click)="deleteCollection(coll.collectionId)" class="btn-delete">Delete</button>
            </div>
          </div>
        } @empty {
          <div class="empty-state">
            <p>No collections yet. Create your first collection to start organizing endpoints.</p>
          </div>
        }
      </div>
    </div>
  `,
  styles: [`
    .collections-page { padding: 30px; max-width: 1200px; margin: 0 auto; }
    .page-header {
      display: flex; justify-content: space-between; align-items: center; margin-bottom: 24px;
      h1 { font-size: 24px; font-weight: 700; }
      .subtitle { font-size: 14px; color: var(--text-secondary); margin-top: 4px; }
    }
    .collections-grid {
      display: grid; grid-template-columns: repeat(auto-fill, minmax(320px, 1fr)); gap: 20px;
    }
    .collection-card {
      background-color: var(--bg-dark); border: 1px solid var(--border-color); border-radius: 8px; padding: 20px;
      display: flex; flex-direction: column; justify-content: space-between;
      .card-header {
        display: flex; gap: 12px;
        .coll-icon { font-size: 24px; }
        .coll-info {
          h3 { font-size: 16px; font-weight: 600; color: var(--text-primary); }
          p { font-size: 13px; color: var(--text-secondary); margin-top: 4px; }
        }
      }
      .card-footer {
        display: flex; justify-content: space-between; align-items: center; margin-top: 20px; pt-2;
        border-top: 1px solid var(--border-color);
        padding-top: 12px;
        .request-count { font-size: 12px; color: var(--text-muted); font-family: var(--font-code); }
        .btn-delete { background: transparent; color: var(--color-danger); border: none; font-size: 12px; cursor: pointer; }
      }
    }
    .modal-backdrop {
      position: fixed; inset: 0; background: rgba(0,0,0,0.7); display: flex; align-items: center; justify-content: center; z-index: 100;
    }
    .modal-card {
      background: var(--bg-dark); border: 1px solid var(--border-color); border-radius: 8px; padding: 24px; width: 420px;
      h3 { margin-bottom: 16px; }
      .form-group { margin-bottom: 14px; display: flex; flex-direction: column; gap: 6px; }
      .modal-actions { display: flex; justify-content: flex-end; gap: 10px; margin-top: 20px; }
    }
    .empty-state { grid-column: 1 / -1; padding: 60px; text-align: center; color: var(--text-muted); }
  `]
})
export class CollectionsComponent implements OnInit {
  private api = inject(ApiService);

  collections = signal<Collection[]>([]);
  showCreateModal = false;
  newCollectionName = '';
  newCollectionDesc = '';

  ngOnInit(): void {
    this.loadCollections();
  }

  loadCollections(): void {
    this.api.getCollections().subscribe({
      next: (data) => this.collections.set(data),
      error: () => {
        // Fallback demo collections
        this.collections.set([
          { collectionId: '1', name: 'User Management API', description: 'User auth and profile endpoints', createdAt: new Date().toISOString(), requestCount: 4 },
          { collectionId: '2', name: 'Products & Catalog', description: 'Public product query assertions', createdAt: new Date().toISOString(), requestCount: 7 }
        ]);
      }
    });
  }

  createCollection(): void {
    if (!this.newCollectionName.trim()) return;

    this.api.createCollection({ name: this.newCollectionName, description: this.newCollectionDesc }).subscribe({
      next: (created) => {
        this.collections.update(c => [...c, created]);
        this.showCreateModal = false;
        this.newCollectionName = '';
        this.newCollectionDesc = '';
      },
      error: () => {
        // Mock add for offline demo
        this.collections.update(c => [...c, {
          collectionId: Math.random().toString(),
          name: this.newCollectionName,
          description: this.newCollectionDesc,
          createdAt: new Date().toISOString(),
          requestCount: 0
        }]);
        this.showCreateModal = false;
        this.newCollectionName = '';
        this.newCollectionDesc = '';
      }
    });
  }

  deleteCollection(id: string): void {
    this.api.deleteCollection(id).subscribe({
      next: () => this.collections.update(c => c.filter(x => x.collectionId !== id)),
      error: () => this.collections.update(c => c.filter(x => x.collectionId !== id))
    });
  }
}
