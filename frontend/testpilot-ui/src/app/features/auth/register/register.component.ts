import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  template: `
    <div class="auth-container">
      <div class="auth-card">
        <h2>Create an Account</h2>
        <p class="subtitle">Set up your workspace for automated API testing</p>

        @if (errorMessage) {
          <div class="alert alert-danger">{{ errorMessage }}</div>
        }

        <form [formGroup]="registerForm" (ngSubmit)="onSubmit()">
          <div class="form-group">
            <label for="name">Full Name</label>
            <input id="name" type="text" formControlName="name" placeholder="John Doe" />
          </div>

          <div class="form-group">
            <label for="email">Email Address</label>
            <input id="email" type="email" formControlName="email" placeholder="dev@example.com" />
          </div>

          <div class="form-group">
            <label for="password">Password</label>
            <input id="password" type="password" formControlName="password" placeholder="••••••••" />
          </div>

          <button type="submit" [disabled]="registerForm.invalid || isLoading" class="btn-primary w-full">
            {{ isLoading ? 'Registering...' : 'Register' }}
          </button>
        </form>

        <p class="switch-link">
          Already have an account? <a routerLink="/auth/login">Sign in</a>
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
export class RegisterComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  isLoading = false;
  errorMessage = '';

  registerForm: FormGroup = this.fb.group({
    name: ['', [Validators.required, Validators.minLength(2)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  onSubmit(): void {
    if (this.registerForm.invalid) return;

    this.isLoading = true;
    this.errorMessage = '';

    this.authService.register(this.registerForm.value).subscribe({
      next: () => {
        this.isLoading = false;
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.detail || err.error?.message || 'Registration failed. Email may already be in use.';
      }
    });
  }
}
