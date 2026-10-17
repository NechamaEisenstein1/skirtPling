import { CurrencyPipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CartItem } from '../../core/models/cart.models';
import { CartService } from '../../core/services/cart.service';
import { NotificationService } from '../../core/services/notification.service';
import { apiErrorMessage } from '../../shared/utils/api-error';

const FREE_SHIPPING_FROM = 300;
const MAX_PER_LINE = 20;

@Component({
  selector: 'app-cart',
  imports: [CurrencyPipe, RouterLink],
  templateUrl: './cart.html',
  styleUrl: './cart.scss',
})
export class CartPage {
  private readonly cartService = inject(CartService);
  private readonly notifications = inject(NotificationService);

  protected readonly cart = this.cartService.cart;
  protected readonly busyProductId = signal<number | null>(null);
  protected readonly amountToFreeShipping = computed(() => Math.max(0, FREE_SHIPPING_FROM - this.cart().subtotal));
  protected readonly hasStockProblem = computed(() => this.cart().items.some((i) => i.quantity > i.availableStock));

  protected quantityOptions(item: CartItem): number[] {
    const max = Math.max(item.quantity, Math.min(MAX_PER_LINE, item.availableStock));
    return Array.from({ length: max }, (_, i) => i + 1);
  }

  protected changeQuantity(item: CartItem, quantity: number): void {
    this.run(item.productId, this.cartService.updateQuantity(item.productId, quantity));
  }

  protected remove(item: CartItem): void {
    this.run(item.productId, this.cartService.remove(item.productId), `${item.name} was removed from your cart.`);
  }

  private run(productId: number, request: ReturnType<CartService['remove']>, successMessage?: string): void {
    this.busyProductId.set(productId);
    request.subscribe({
      next: () => {
        this.busyProductId.set(null);
        if (successMessage) this.notifications.success(successMessage);
      },
      error: (error) => {
        this.busyProductId.set(null);
        this.notifications.error(apiErrorMessage(error));
        this.cartService.load();
      },
    });
  }
}
