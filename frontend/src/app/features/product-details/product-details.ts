import { CurrencyPipe } from '@angular/common';
import { Component, computed, effect, inject, input, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { Title } from '@angular/platform-browser';
import { Router, RouterLink } from '@angular/router';
import { CartService } from '../../core/services/cart.service';
import { CatalogService } from '../../core/services/catalog.service';
import { NotificationService } from '../../core/services/notification.service';
import { ProductCard } from '../../shared/components/product-card';
import { StockBadge } from '../../shared/components/stock-badge';
import { apiErrorMessage } from '../../shared/utils/api-error';

const MAX_PER_ORDER = 20;

@Component({
  selector: 'app-product-details',
  imports: [CurrencyPipe, RouterLink, ProductCard, StockBadge],
  templateUrl: './product-details.html',
  styleUrl: './product-details.scss',
})
export class ProductDetailsPage {
  private readonly catalog = inject(CatalogService);
  private readonly cart = inject(CartService);
  private readonly notifications = inject(NotificationService);
  private readonly router = inject(Router);
  private readonly title = inject(Title);

  /** Bound from the :id route parameter. */
  readonly id = input.required<string>();

  protected readonly product = rxResource({
    params: () => Number(this.id()),
    stream: ({ params }) => this.catalog.getProduct(params),
  });
  protected readonly selectedImage = signal(0);
  protected readonly quantity = signal(1);
  protected readonly adding = signal(false);
  protected readonly quantityOptions = computed(() => {
    const max = Math.min(MAX_PER_ORDER, this.product.value()?.stockQuantity ?? 0);
    return Array.from({ length: max }, (_, i) => i + 1);
  });

  constructor() {
    // A new product (e.g. a related product click) starts at the first image and quantity 1.
    effect(() => {
      const product = this.product.value();
      if (!product) return;
      this.selectedImage.set(0);
      this.quantity.set(1);
      this.title.setTitle(`${product.name} | SkirtPling`);
    });
  }

  protected addToCart(goToCheckout = false): void {
    const product = this.product.value();
    if (!product) return;

    this.adding.set(true);
    this.cart.add(product.id, this.quantity()).subscribe({
      next: () => {
        this.adding.set(false);
        if (goToCheckout) {
          this.router.navigateByUrl('/checkout');
        } else {
          this.notifications.success(`${product.name} was added to your cart.`);
        }
      },
      error: (error) => {
        this.adding.set(false);
        this.notifications.error(apiErrorMessage(error));
      },
    });
  }
}
