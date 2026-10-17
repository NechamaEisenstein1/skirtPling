import { Routes } from '@angular/router';
import { adminGuard, signedInGuard } from './core/guards/auth.guards';

export const routes: Routes = [
  { path: '', loadComponent: () => import('./features/home/home').then((m) => m.Home), title: 'SkirtPling' },
  { path: 'shop', loadComponent: () => import('./features/catalog/catalog').then((m) => m.Catalog), title: 'Shop | SkirtPling' },
  {
    path: 'products/:id',
    loadComponent: () => import('./features/product-details/product-details').then((m) => m.ProductDetailsPage),
  },
  { path: 'cart', loadComponent: () => import('./features/cart/cart').then((m) => m.CartPage), title: 'Your cart | SkirtPling' },
  {
    path: 'checkout',
    canActivate: [signedInGuard],
    loadComponent: () => import('./features/checkout/checkout').then((m) => m.CheckoutPage),
    title: 'Checkout | SkirtPling',
  },
  {
    path: 'checkout/confirmation/:id',
    canActivate: [signedInGuard],
    loadComponent: () => import('./features/checkout/order-confirmation').then((m) => m.OrderConfirmation),
    title: 'Thank you | SkirtPling',
  },
  { path: 'account', loadChildren: () => import('./features/account/account.routes').then((m) => m.accountRoutes) },
  { path: 'admin', canActivate: [adminGuard], loadChildren: () => import('./features/admin/admin.routes').then((m) => m.adminRoutes) },
  { path: '**', loadComponent: () => import('./shared/components/not-found').then((m) => m.NotFound), title: 'Not found | SkirtPling' },
];
