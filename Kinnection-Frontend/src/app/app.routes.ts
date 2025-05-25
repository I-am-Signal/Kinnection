import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    loadComponent: () => {
      return import('./pages/landing/landing.component').then(
        (m) => m.LandingComponent
      );
    },
  },
  {
    path: 'sign_up',
    pathMatch: 'full',
    loadComponent: () => {
      return import('./pages/auth/signup/signup.component').then(
        (m) => m.SignupComponent
      );
    },
  },
  {
    path: 'login',
    pathMatch: 'full',
    loadComponent: () => {
      return import('./pages/auth/login/login.component').then(
        (m) => m.LoginComponent
      );
    },
  },
  {
    path: 'mfa/:id',
    pathMatch: 'full',
    loadComponent: () => {
      return import('./pages/auth/mfa/mfa.component').then(
        (m) => m.MfaComponent
      );
    },
  },
];
