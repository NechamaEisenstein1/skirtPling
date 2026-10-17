import { Component, computed, inject, input } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { OrderStatus } from '../../core/models/order.models';
import { AdminService } from '../../core/services/admin.service';
import { ConfirmService } from '../../core/services/confirm.service';
import { NotificationService } from '../../core/services/notification.service';
import { OrderView } from '../../shared/components/order-view';
import { apiErrorMessage } from '../../shared/utils/api-error';

/** Mirrors the transitions the API allows (OrderService.AllowedTransitions); the API remains the authority. */
const NEXT_STATUSES: Record<OrderStatus, OrderStatus[]> = {
  PendingPayment: ['Cancelled'],
  Paid: ['Shipped', 'Cancelled'],
  Shipped: ['Delivered'],
  Delivered: [],
  Cancelled: [],
};

@Component({
  selector: 'app-admin-order-detail',
  imports: [RouterLink, OrderView],
  template: `
    <p><a routerLink="/admin/orders">← All orders</a></p>
    @if (order.value(); as o) {
      @if (nextStatuses().length) {
        <div class="card actions">
          <span>Update status:</span>
          @for (status of nextStatuses(); track status) {
            <button type="button" class="btn btn-sm" [class.btn-danger]="status === 'Cancelled'" (click)="setStatus(status)">
              {{ status === 'Cancelled' ? 'Cancel order and restock' : 'Mark as ' + status }}
            </button>
          }
        </div>
      }
      <app-order-view [order]="o" />
    } @else if (order.error()) {
      <div class="state"><h2>Order not found</h2></div>
    } @else {
      <div class="state"><div class="spinner"></div></div>
    }
  `,
  styles: `.actions { display: flex; flex-wrap: wrap; align-items: center; gap: 12px; margin-bottom: 24px; padding: 16px 24px; }`,
})
export class AdminOrderDetail {
  private readonly admin = inject(AdminService);
  private readonly confirm = inject(ConfirmService);
  private readonly notifications = inject(NotificationService);

  readonly id = input.required<string>();
  protected readonly order = rxResource({
    params: () => Number(this.id()),
    stream: ({ params }) => this.admin.getOrder(params),
  });
  protected readonly nextStatuses = computed(() => {
    const status = this.order.value()?.status;
    return status ? NEXT_STATUSES[status] : [];
  });

  protected async setStatus(status: OrderStatus): Promise<void> {
    if (status === 'Cancelled') {
      const confirmed = await this.confirm.ask('Cancel this order?', 'The items go back into stock. Refunds must be handled with the payment provider.', 'Cancel order');
      if (!confirmed) return;
    }
    this.admin.updateOrderStatus(Number(this.id()), status).subscribe({
      next: (order) => {
        this.order.set(order);
        this.notifications.success(`Order ${order.orderNumber} is now ${order.status}.`);
      },
      error: (error) => this.notifications.error(apiErrorMessage(error)),
    });
  }
}
