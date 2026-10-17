import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { OrderService } from '../../core/services/order.service';
import { OrderStatusBadge } from '../../shared/components/order-status-badge';
import { Pagination } from '../../shared/components/pagination';

@Component({
  selector: 'app-order-history',
  imports: [CurrencyPipe, DatePipe, RouterLink, OrderStatusBadge, Pagination],
  template: `
    <div class="container page">
      <div class="page-header">
        <h1>My orders</h1>
        <a routerLink="/account">Back to my account</a>
      </div>
      @if (orders.isLoading() && !orders.value()) {
        <div class="state"><div class="spinner"></div></div>
      } @else if (orders.error()) {
        <div class="state">We couldn't load your orders. <button class="btn-link" (click)="orders.reload()">Try again</button></div>
      } @else if (orders.value(); as result) {
        @if (result.items.length === 0) {
          <div class="state">
            <h2>No orders yet</h2>
            <a class="btn" routerLink="/shop">Start shopping</a>
          </div>
        } @else {
          <div class="table-wrap">
            <table>
              <thead><tr><th>Order</th><th>Date</th><th>Status</th><th class="num">Items</th><th class="num">Total</th></tr></thead>
              <tbody>
                @for (order of result.items; track order.id) {
                  <tr>
                    <td><a [routerLink]="['/account/orders', order.id]">{{ order.orderNumber }}</a></td>
                    <td>{{ order.createdAt | date: 'mediumDate' }}</td>
                    <td><app-order-status-badge [status]="order.status" /></td>
                    <td class="num">{{ order.itemCount }}</td>
                    <td class="num">{{ order.total | currency: 'ILS' }}</td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
          <app-pagination [page]="page()" [pageSize]="result.pageSize" [totalCount]="result.totalCount" (pageChange)="page.set($event)" />
        }
      }
    </div>
  `,
})
export class OrderHistory {
  private readonly orderService = inject(OrderService);
  protected readonly page = signal(1);
  protected readonly orders = rxResource({
    params: () => this.page(),
    stream: ({ params }) => this.orderService.getMyOrders(params),
  });
}
