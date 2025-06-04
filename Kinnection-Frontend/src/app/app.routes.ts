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
  {
    path: 'forgot',
    pathMatch: 'full',
    loadComponent: () => {
      return import('./pages/auth/forgot/forgot.component').then(
        (m) => m.ForgotComponent
      );
    },
  },
  {
    path: 'reset/:reset-token',
    pathMatch: 'full',
    loadComponent: () => {
      return import('./pages/auth/reset/reset.component').then(
        (m) => m.ResetComponent
      );
    },
  },
  {
    path: 'dashboard/:id',
    pathMatch: 'full',
    loadComponent() {
      return import('./pages/userdash/userdash.component').then(
        (m) => m.UserdashComponent
      );
    },
  },
  {
    path: 'logout',
    pathMatch: 'full',
    loadComponent: () => {
      return import('./pages/auth/logout/logout.component').then(
        (m) => m.LogoutComponent
      );
    },
  },
  {
    path: 'account_details/:id',
    pathMatch: 'full',
    loadComponent: () => {
      return import('./pages/account-details/account-details.component').then(
        (m) => m.AccountDetailsComponent
      );
    }
  }
];
