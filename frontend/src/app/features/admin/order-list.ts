import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AdminService } from '../../core/services/admin.service';
import { OrderStatusBadge } from '../../shared/components/order-status-badge';
import { Pagination } from '../../shared/components/pagination';

@Component({
  selector: 'app-order-list',
  imports: [CurrencyPipe, DatePipe, RouterLink, OrderStatusBadge, Pagination],
  template: `
    <h1>Orders</h1>
    <form class="toolbar" (submit)="$event.preventDefault(); update({ search: term.value.trim() || undefined })">
      <input #term type="search" placeholder="Order number or email" aria-label="Search orders" />
      <select aria-label="Status" [value]="query().status ?? ''" (change)="update({ status: $any($event.target).value || undefined })">
        <option value="">All statuses</option>
        @for (status of statuses; track status) {
          <option [value]="status">{{ status }}</option>
        }
      </select>
      <button class="btn btn-outline btn-sm" type="submit">Search</button>
    </form>

    @if (orders.error()) {
      <div class="state">Couldn't load orders. <button class="btn-link" (click)="orders.reload()">Retry</button></div>
    } @else if (orders.value(); as result) {
      <div class="table-wrap">
        <table>
          <thead><tr><th>Order</th><th>Customer</th><th>Date</th><th>Status</th><th class="num">Items</th><th class="num">Total</th></tr></thead>
          <tbody>
            @for (order of result.items; track order.id) {
              <tr>
                <td><a [routerLink]="['/admin/orders', order.id]">{{ order.orderNumber }}</a></td>
                <td>{{ order.customerEmail }}</td>
                <td>{{ order.createdAt | date: 'short' }}</td>
                <td><app-order-status-badge [status]="order.status" /></td>
                <td class="num">{{ order.itemCount }}</td>
                <td class="num">{{ order.total | currency: 'ILS' }}</td>
              </tr>
            } @empty {
              <tr><td colspan="6" class="muted">No orders match.</td></tr>
            }
          </tbody>
        </table>
      </div>
      <app-pagination [page]="result.page" [pageSize]="result.pageSize" [totalCount]="result.totalCount" (pageChange)="update({ page: $event })" />
    } @else {
      <div class="state"><div class="spinner"></div></div>
    }
  `,
})
export class OrderList {
  private readonly admin = inject(AdminService);
  protected readonly statuses = ['PendingPayment', 'Paid', 'Shipped', 'Delivered', 'Cancelled'];

  protected readonly query = signal<{ status?: string; search?: string; page: number; pageSize: number }>({
    status: inject(ActivatedRoute).snapshot.queryParamMap.get('status') ?? undefined,
    page: 1,
    pageSize: 20,
  });
  protected readonly orders = rxResource({
    params: () => this.query(),
    stream: ({ params }) => this.admin.searchOrders(params),
  });

  protected update(change: { status?: string; search?: string; page?: number }): void {
    this.query.update((q) => ({ ...q, page: 1, ...change }));
  }
}
