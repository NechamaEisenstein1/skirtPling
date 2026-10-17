import { Routes } from '@angular/router';
import { AdminLayout } from './admin-layout';

export const adminRoutes: Routes = [
  {
    path: '',
    component: AdminLayout,
    children: [
      { path: '', loadComponent: () => import('./dashboard').then((m) => m.Dashboard), title: 'Dashboard | Admin' },
      { path: 'products', loadComponent: () => import('./product-list').then((m) => m.ProductList), title: 'Products | Admin' },
      { path: 'products/new', loadComponent: () => import('./product-form').then((m) => m.ProductForm), title: 'New product | Admin' },
      { path: 'products/:id', loadComponent: () => import('./product-form').then((m) => m.ProductForm), title: 'Edit product | Admin' },
      { path: 'catalog', loadComponent: () => import('./brands-categories').then((m) => m.BrandsCategories), title: 'Brands & categories | Admin' },
      { path: 'orders', loadComponent: () => import('./order-list').then((m) => m.OrderList), title: 'Orders | Admin' },
      { path: 'orders/:id', loadComponent: () => import('./admin-order-detail').then((m) => m.AdminOrderDetail), title: 'Order | Admin' },
      { path: 'users', loadComponent: () => import('./user-list').then((m) => m.UserList), title: 'Users | Admin' },
      { path: 'legacy', loadComponent: () => import('./legacy-inventory').then((m) => m.LegacyInventory), title: 'Legacy warehouse | Admin' },
    ],
  },
];
