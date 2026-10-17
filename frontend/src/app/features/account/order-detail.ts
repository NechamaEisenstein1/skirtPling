import { Component, inject, input } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { OrderService } from '../../core/services/order.service';
import { OrderView } from '../../shared/components/order-view';

@Component({
  selector: 'app-order-detail',
  imports: [RouterLink, OrderView],
  template: `
    <div class="container page">
      <p><a routerLink="/account/orders">← All orders</a></p>
      @if (order.value(); as o) {
        <app-order-view [order]="o" />
      } @else if (order.error()) {
        <div class="state"><h2>Order not found</h2></div>
      } @else {
        <div class="state"><div class="spinner"></div></div>
      }
    </div>
  `,
})
export class OrderDetail {
  private readonly orders = inject(OrderService);
  readonly id = input.required<string>();

  protected readonly order = rxResource({
    params: () => Number(this.id()),
    stream: ({ params }) => this.orders.getMyOrder(params),
  });
}
