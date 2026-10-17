import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Cart } from '../models/cart.models';

const EMPTY_CART: Cart = { items: [], subtotal: 0, shippingCost: 0, total: 0, itemCount: 0 };

/**
 * The current cart, shared by the header badge, cart page and checkout.
 * The server owns prices and stock; this service only mirrors its latest answer.
 */
@Injectable({ providedIn: 'root' })
export class CartService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/cart`;
  private readonly state = signal<Cart>(EMPTY_CART);

  readonly cart = this.state.asReadonly();
  readonly itemCount = computed(() => this.state().itemCount);

  load(): void {
    this.http.get<Cart>(this.baseUrl).subscribe({
      next: (cart) => this.state.set(cart),
      error: () => this.state.set(EMPTY_CART),
    });
  }

  add(productId: number, quantity = 1) {
    return this.http.post<Cart>(`${this.baseUrl}/items`, { productId, quantity }).pipe(tap((c) => this.state.set(c)));
  }

  updateQuantity(productId: number, quantity: number) {
    return this.http.put<Cart>(`${this.baseUrl}/items/${productId}`, { quantity }).pipe(tap((c) => this.state.set(c)));
  }

  remove(productId: number) {
    return this.http.delete<Cart>(`${this.baseUrl}/items/${productId}`).pipe(tap((c) => this.state.set(c)));
  }

  mergeGuestCart(): void {
    this.http.post<Cart>(`${this.baseUrl}/merge`, null).subscribe((cart) => this.state.set(cart));
  }

  /** Called after a successful checkout; the server has already emptied the cart. */
  reset(): void {
    this.state.set(EMPTY_CART);
  }
}
