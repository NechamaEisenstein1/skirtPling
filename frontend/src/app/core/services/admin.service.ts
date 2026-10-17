import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { toHttpParams } from '../../shared/utils/http-params';
import { AdminUser, DashboardStats, LegacyStock } from '../models/admin.models';
import { Order, OrderStatus, OrderSummary } from '../models/order.models';
import { PagedResult } from '../models/paged-result';

/** Admin dashboard, order management, user management and the legacy warehouse lookup. */
@Injectable({ providedIn: 'root' })
export class AdminService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/admin`;

  getDashboard() {
    return this.http.get<DashboardStats>(`${this.baseUrl}/dashboard`);
  }

  searchOrders(query: { status?: string; search?: string; page: number; pageSize: number }) {
    return this.http.get<PagedResult<OrderSummary>>(`${this.baseUrl}/orders`, { params: toHttpParams(query) });
  }

  getOrder(id: number) {
    return this.http.get<Order>(`${this.baseUrl}/orders/${id}`);
  }

  updateOrderStatus(id: number, status: OrderStatus) {
    return this.http.patch<Order>(`${this.baseUrl}/orders/${id}/status`, { status });
  }

  searchUsers(query: { search?: string; page: number; pageSize: number }) {
    return this.http.get<PagedResult<AdminUser>>(`${this.baseUrl}/users`, { params: toHttpParams(query) });
  }

  updateUserAccess(id: string, access: { isAdmin: boolean; isLocked: boolean }) {
    return this.http.put<AdminUser>(`${this.baseUrl}/users/${id}/access`, access);
  }

  getLegacyStatus() {
    return this.http.get<{ isAvailable: boolean }>(`${this.baseUrl}/legacy-inventory/status`);
  }

  compareLegacyStock(sku: string) {
    return this.http.get<LegacyStock>(`${this.baseUrl}/legacy-inventory/${encodeURIComponent(sku)}`);
  }
}
