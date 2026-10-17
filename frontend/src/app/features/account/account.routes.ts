import { Routes } from '@angular/router';
import { signedInGuard } from '../../core/guards/auth.guards';

export const accountRoutes: Routes = [
  { path: 'login', loadComponent: () => import('./login').then((m) => m.Login), title: 'Sign in | SkirtPling' },
  { path: 'register', loadComponent: () => import('./register').then((m) => m.Register), title: 'Create account | SkirtPling' },
  {
    path: '',
    canActivate: [signedInGuard],
    loadComponent: () => import('./account-overview').then((m) => m.AccountOverview),
    title: 'My account | SkirtPling',
  },
  {
    path: 'orders',
    canActivate: [signedInGuard],
    loadComponent: () => import('./order-history').then((m) => m.OrderHistory),
    title: 'My orders | SkirtPling',
  },
  {
    path: 'orders/:id',
    canActivate: [signedInGuard],
    loadComponent: () => import('./order-detail').then((m) => m.OrderDetail),
    title: 'Order | SkirtPling',
  },
];
