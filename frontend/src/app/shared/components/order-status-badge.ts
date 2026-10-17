import { Component, computed, input } from '@angular/core';
import { OrderStatus } from '../../core/models/order.models';

const STATUS_VIEW: Record<OrderStatus, { label: string; css: string }> = {
  PendingPayment: { label: 'Awaiting payment', css: 'badge warning' },
  Paid: { label: 'Paid', css: 'badge accent' },
  Shipped: { label: 'Shipped', css: 'badge' },
  Delivered: { label: 'Delivered', css: 'badge success' },
  Cancelled: { label: 'Cancelled', css: 'badge danger' },
};

@Component({
  selector: 'app-order-status-badge',
  template: `<span [class]="view().css">{{ view().label }}</span>`,
})
export class OrderStatusBadge {
  readonly status = input.required<OrderStatus>();
  protected readonly view = computed(() => STATUS_VIEW[this.status()]);
}
