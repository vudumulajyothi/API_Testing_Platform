import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  template: `
    <div class="auth-container">
      <div class="auth-card">
        <h2>Sign In to TestPilot</h2>
        <p class="subtitle">Enter your credentials to access your API testing workspace</p>

        @if (errorMessage) {
          <div class="alert alert-danger">{{ errorMessage }}</div>
        }

        <form [formGroup]="loginForm" (ngSubmit)="onSubmit()">
          <div class="form-group">
            <label for="email">Email Address</label>
            <input id="email" type="email" formControlName="email" placeholder="dev@example.com" />
          </div>

          <div class="form-group">
            <label for="password">Password</label>
            <input id="password" type="password" formControlName="password" placeholder="••••••••" />
          </div>

          <button type="submit" [disabled]="loginForm.invalid || isLoading" class="btn-primary w-full">
            {{ isLoading ? 'Signing In...' : 'Sign In' }}
          </button>
        </form>

        <p class="switch-link">
          Don't have an account? <a routerLink="/auth/register">Create one</a>
        </p>
      </div>
    </div>
  `,
  styles: [`
    .auth-container {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: calc(100vh - 54px);
      background-color: var(--bg-darker);
    }
    .auth-card {
      width: 100%;
      max-width: 400px;
      padding: 32px;
      background-color: var(--bg-dark);
      border: 1px solid var(--border-color);
      border-radius: 8px;
      h2 { font-size: 20px; font-weight: 600; margin-bottom: 6px; }
      .subtitle { font-size: 13px; color: var(--text-secondary); margin-bottom: 24px; }
    }
    .form-group {
      margin-bottom: 16px;
      display: flex;
      flex-direction: column;
      gap: 6px;
      label { font-size: 13px; font-weight: 500; color: var(--text-secondary); }
    }
    .w-full { width: 100%; margin-top: 8px; padding: 10px; }
    .switch-link { margin-top: 20px; font-size: 13px; text-align: center; color: var(--text-secondary); }
    .alert-danger {
      padding: 10px;
      background-color: rgba(218, 54, 51, 0.15);
      border: 1px solid var(--color-danger);
      color: #ff7b72;
      border-radius: 6px;
      font-size: 13px;
      margin-bottom: 16px;
    }
  `]
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  isLoading = false;
  errorMessage = '';

  loginForm: FormGroup = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  onSubmit(): void {
    if (this.loginForm.invalid) return;

    this.isLoading = true;
    this.errorMessage = '';

    this.authService.login(this.loginForm.value).subscribe({
      next: () => {
        this.isLoading = false;
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.detail || err.error?.message || 'Login failed. Please check your credentials.';
      }
    });
  }
}
