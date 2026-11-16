import { Routes } from '@angular/router';

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
    loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
  },
  {
    path: 'request-builder',
    loadComponent: () => import('./features/request-builder/request-builder.component').then(m => m.RequestBuilderComponent)
  },
  {
    path: 'collections',
    loadComponent: () => import('./features/collections/collections.component').then(m => m.CollectionsComponent)
  },
  {
    path: 'test-runner',
    loadComponent: () => import('./features/test-runner/test-runner.component').then(m => m.TestRunnerComponent)
  },
  {
    path: 'history',
    loadComponent: () => import('./features/history/history.component').then(m => m.HistoryComponent)
  },
  {
    path: 'reports',
    loadComponent: () => import('./features/reports/reports.component').then(m => m.ReportsComponent)
  },
  {
    path: '**',
    redirectTo: 'dashboard'
  }
];
