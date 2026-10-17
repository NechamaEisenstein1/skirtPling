import { CurrencyPipe, KeyValuePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { OrderStatus } from '../../core/models/order.models';
import { AdminService } from '../../core/services/admin.service';
import { OrderStatusBadge } from '../../shared/components/order-status-badge';

@Component({
  selector: 'app-dashboard',
  imports: [CurrencyPipe, KeyValuePipe, RouterLink, OrderStatusBadge],
  template: `
    <h1>Dashboard</h1>
    @if (stats.value(); as s) {
      <div class="tiles">
        <div class="card tile"><span class="muted">Revenue</span><strong>{{ s.totalRevenue | currency: 'ILS' }}</strong></div>
        <div class="card tile"><span class="muted">Paid orders</span><strong>{{ s.paidOrders }}</strong></div>
        <div class="card tile"><span class="muted">Products</span><strong>{{ s.totalProducts }}</strong></div>
        <a class="card tile" routerLink="/admin/products" [queryParams]="{ sort: 'stock' }">
          <span class="muted">Low stock (≤ 5)</span><strong [class.warn]="s.lowStockProducts > 0">{{ s.lowStockProducts }}</strong>
        </a>
      </div>

      <div class="panels">
        <section class="card">
          <h2>Top sellers</h2>
          @if (s.topProducts.length === 0) {
            <p class="muted">No sales yet.</p>
          } @else {
            <table>
              <thead><tr><th>Product</th><th class="num">Units</th><th class="num">Revenue</th></tr></thead>
              <tbody>
                @for (p of s.topProducts; track p.productId) {
                  <tr>
                    <td><a [routerLink]="['/admin/products', p.productId]">{{ p.productName }}</a></td>
                    <td class="num">{{ p.unitsSold }}</td>
                    <td class="num">{{ p.revenue | currency: 'ILS' }}</td>
                  </tr>
                }
              </tbody>
            </table>
          }
        </section>
        <section class="card">
          <h2>Orders by status</h2>
          @for (entry of s.ordersByStatus | keyvalue; track entry.key) {
            <a class="status-row" routerLink="/admin/orders" [queryParams]="{ status: entry.key }">
              <app-order-status-badge [status]="asStatus(entry.key)" /><strong>{{ entry.value }}</strong>
            </a>
          } @empty {
            <p class="muted">No orders yet.</p>
          }
        </section>
      </div>
    } @else if (stats.error()) {
      <div class="state">Couldn't load statistics. <button class="btn-link" (click)="stats.reload()">Retry</button></div>
    } @else {
      <div class="state"><div class="spinner"></div></div>
    }
  `,
  styles: `
    .tiles { display: grid; grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); gap: 16px; margin-bottom: 24px; }
    .tile { display: flex; flex-direction: column; gap: 6px; color: inherit; }
    a.tile:hover { text-decoration: none; border-color: var(--accent); }
    .tile strong { font-size: 1.6rem; font-family: var(--font-display); }
    .warn { color: var(--warning); }
    .panels { display: grid; grid-template-columns: 2fr 1fr; gap: 24px; }
    .status-row { display: flex; justify-content: space-between; padding: 8px 0; color: inherit; border-bottom: 1px solid var(--line); }
    @media (max-width: 900px) { .panels { grid-template-columns: 1fr; } }
  `,
})
export class Dashboard {
  private readonly admin = inject(AdminService);
  protected readonly stats = rxResource({ stream: () => this.admin.getDashboard() });

  protected asStatus(value: string): OrderStatus {
    return value as OrderStatus;
  }
}
