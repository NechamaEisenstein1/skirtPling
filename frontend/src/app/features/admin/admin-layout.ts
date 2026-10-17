import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-admin-layout',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <div class="container admin">
      <nav class="side" aria-label="Admin">
        <span class="title">Admin</span>
        <a routerLink="/admin" routerLinkActive="active" [routerLinkActiveOptions]="{ exact: true }">Dashboard</a>
        <a routerLink="/admin/products" routerLinkActive="active">Products</a>
        <a routerLink="/admin/catalog" routerLinkActive="active">Brands & categories</a>
        <a routerLink="/admin/orders" routerLinkActive="active">Orders</a>
        <a routerLink="/admin/users" routerLinkActive="active">Users</a>
        <a routerLink="/admin/legacy" routerLinkActive="active">Legacy warehouse</a>
      </nav>
      <section class="content">
        <router-outlet />
      </section>
    </div>
  `,
  styles: `
    .admin { display: grid; grid-template-columns: 210px 1fr; gap: 32px; padding-top: 32px; padding-bottom: 64px; }
    .side { display: flex; flex-direction: column; gap: 4px; position: sticky; top: 90px; align-self: start; }
    .title { font-family: var(--font-display); font-size: 1.3rem; margin-bottom: 12px; }
    .side a { padding: 8px 12px; border-radius: 8px; color: var(--ink); }
    .side a:hover { background: var(--accent-soft); text-decoration: none; }
    .side a.active { background: var(--ink); color: #fff; }
    .content { min-width: 0; }
    @media (max-width: 800px) {
      .admin { grid-template-columns: 1fr; }
      .side { position: static; flex-direction: row; flex-wrap: wrap; }
      .title { width: 100%; }
    }
  `,
})
export class AdminLayout {}
