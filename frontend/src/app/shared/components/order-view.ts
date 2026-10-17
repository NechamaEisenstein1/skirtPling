import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, input } from '@angular/core';
import { Order } from '../../core/models/order.models';
import { OrderStatusBadge } from './order-status-badge';

/** Read-only order details, shared by the customer's order page and the admin order page. */
@Component({
  selector: 'app-order-view',
  imports: [CurrencyPipe, DatePipe, OrderStatusBadge],
  template: `
    <div class="order">
      <div class="card">
        <div class="head">
          <div>
            <h2>Order {{ order().orderNumber }}</h2>
            <span class="muted">Placed {{ order().createdAt | date: 'medium' }} · {{ order().customerEmail }}</span>
          </div>
          <app-order-status-badge [status]="order().status" />
        </div>
        <div class="table-wrap">
          <table>
            <thead>
              <tr><th>Product</th><th>SKU</th><th class="num">Price</th><th class="num">Qty</th><th class="num">Total</th></tr>
            </thead>
            <tbody>
              @for (item of order().items; track item.productId) {
                <tr>
                  <td>{{ item.productName }}</td>
                  <td class="muted">{{ item.sku }}</td>
                  <td class="num">{{ item.unitPrice | currency: 'ILS' }}</td>
                  <td class="num">{{ item.quantity }}</td>
                  <td class="num">{{ item.lineTotal | currency: 'ILS' }}</td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      </div>
      <div class="side">
        <div class="card">
          <h3>Shipping to</h3>
          <p>
            {{ order().shippingAddress.fullName }}<br />
            {{ order().shippingAddress.street }}<br />
            {{ order().shippingAddress.city }} {{ order().shippingAddress.postalCode }}<br />
            {{ order().shippingAddress.country }}<br />
            <span class="muted">{{ order().shippingAddress.phone }}</span>
          </p>
        </div>
        <div class="card totals">
          <div><span>Subtotal</span><span>{{ order().subtotal | currency: 'ILS' }}</span></div>
          <div><span>Shipping</span><span>{{ order().shippingCost | currency: 'ILS' }}</span></div>
          <div class="total"><span>Total</span><span>{{ order().total | currency: 'ILS' }}</span></div>
          @if (order().paymentReference) {
            <span class="muted ref">Payment ref: {{ order().paymentReference }}</span>
          }
        </div>
      </div>
    </div>
  `,
  styles: `
    .order { display: grid; grid-template-columns: 1fr 300px; gap: 24px; align-items: start; }
    .head { display: flex; justify-content: space-between; align-items: flex-start; gap: 12px; margin-bottom: 16px; }
    .table-wrap { border: none; }
    .side { display: flex; flex-direction: column; gap: 16px; }
    .totals div { display: flex; justify-content: space-between; margin-bottom: 8px; }
    .totals .total { font-weight: 600; border-top: 1px solid var(--line); padding-top: 10px; }
    .ref { font-size: .8rem; word-break: break-all; }
    @media (max-width: 800px) { .order { grid-template-columns: 1fr; } }
  `,
})
export class OrderView {
  readonly order = input.required<Order>();
}
