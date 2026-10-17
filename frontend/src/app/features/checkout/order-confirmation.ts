import { CurrencyPipe } from '@angular/common';
import { Component, inject, input } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { OrderService } from '../../core/services/order.service';

@Component({
  selector: 'app-order-confirmation',
  imports: [CurrencyPipe, RouterLink],
  template: `
    <div class="container page">
      @if (order.value(); as o) {
        <div class="card confirmation">
          <span class="badge success">Payment approved</span>
          <h1>Thank you for your order!</h1>
          <p>Order <strong>{{ o.orderNumber }}</strong> is confirmed. We'll ship it to {{ o.shippingAddress.fullName }}, {{ o.shippingAddress.city }}.</p>
          <p class="muted">Total charged: {{ o.total | currency: 'ILS' }} · Payment reference {{ o.paymentReference }}</p>
          <div class="form-actions">
            <a class="btn" [routerLink]="['/account/orders', o.id]">View order</a>
            <a class="btn btn-outline" routerLink="/shop">Continue shopping</a>
          </div>
        </div>
      } @else if (order.error()) {
        <div class="state">We couldn't load this order. <a routerLink="/account/orders">See your orders</a></div>
      } @else {
        <div class="state"><div class="spinner"></div></div>
      }
    </div>
  `,
  styles: `.confirmation { max-width: 640px; margin: 0 auto; text-align: center; } .form-actions { justify-content: center; }`,
})
export class OrderConfirmation {
  private readonly orders = inject(OrderService);
  readonly id = input.required<string>();

  protected readonly order = rxResource({
    params: () => Number(this.id()),
    stream: ({ params }) => this.orders.getMyOrder(params),
  });
}
