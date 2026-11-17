import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'dashboard',
    pathMatch: 'full'
  },
  {
    path: 'auth/login',
    loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'auth/register',
    loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent)
  },
  {
    path: 'dashboard',
    canActivate: [authGuard],
    loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
  },
  {
    path: 'request-builder',
    canActivate: [authGuard],
    loadComponent: () => import('./features/request-builder/request-builder.component').then(m => m.RequestBuilderComponent)
  },
  {
    path: 'collections',
    canActivate: [authGuard],
    loadComponent: () => import('./features/collections/collections.component').then(m => m.CollectionsComponent)
  },
  {
    path: 'test-runner',
    canActivate: [authGuard],
    loadComponent: () => import('./features/test-runner/test-runner.component').then(m => m.TestRunnerComponent)
  },
  {
    path: 'history',
    canActivate: [authGuard],
    loadComponent: () => import('./features/history/history.component').then(m => m.HistoryComponent)
  },
  {
    path: 'reports',
    canActivate: [authGuard],
    loadComponent: () => import('./features/reports/reports.component').then(m => m.ReportsComponent)
  },
  {
    path: '**',
    redirectTo: 'dashboard'
  }
];
