import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Checkout, Order, OrderSummary } from '../models/order.models';
import { PagedResult } from '../models/paged-result';

/** Checkout and the signed-in customer's own orders. */
@Injectable({ providedIn: 'root' })
export class OrderService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/orders`;

  checkout(checkout: Checkout) {
    return this.http.post<Order>(this.baseUrl, checkout);
  }

  getMyOrders(page: number, pageSize = 10) {
    return this.http.get<PagedResult<OrderSummary>>(this.baseUrl, { params: { page, pageSize } });
  }

  getMyOrder(id: number) {
    return this.http.get<Order>(`${this.baseUrl}/${id}`);
  }
}
