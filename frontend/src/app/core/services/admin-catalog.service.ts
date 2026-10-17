import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { toHttpParams } from '../../shared/utils/http-params';
import { Brand, Category, ProductDetails, ProductListItem, ProductQuery, ProductUpsert } from '../models/catalog.models';
import { PagedResult } from '../models/paged-result';

/** Admin write access to products, brands and categories. */
@Injectable({ providedIn: 'root' })
export class AdminCatalogService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/admin`;

  searchProducts(query: ProductQuery) {
    return this.http.get<PagedResult<ProductListItem>>(`${this.baseUrl}/products`, { params: toHttpParams(query) });
  }

  getProduct(id: number) {
    return this.http.get<ProductDetails>(`${this.baseUrl}/products/${id}`);
  }

  createProduct(product: ProductUpsert) {
    return this.http.post<ProductDetails>(`${this.baseUrl}/products`, product);
  }

  updateProduct(id: number, product: ProductUpsert) {
    return this.http.put<ProductDetails>(`${this.baseUrl}/products/${id}`, product);
  }

  updateStock(id: number, stockQuantity: number) {
    return this.http.patch<void>(`${this.baseUrl}/products/${id}/stock`, { stockQuantity });
  }

  deleteProduct(id: number) {
    return this.http.delete<void>(`${this.baseUrl}/products/${id}`);
  }

  saveBrand(brand: { name: string; description: string }, id?: number) {
    return id
      ? this.http.put<Brand>(`${this.baseUrl}/brands/${id}`, brand)
      : this.http.post<Brand>(`${this.baseUrl}/brands`, brand);
  }

  deleteBrand(id: number) {
    return this.http.delete<void>(`${this.baseUrl}/brands/${id}`);
  }

  saveCategory(category: { name: string }, id?: number) {
    return id
      ? this.http.put<Category>(`${this.baseUrl}/categories/${id}`, category)
      : this.http.post<Category>(`${this.baseUrl}/categories`, category);
  }

  deleteCategory(id: number) {
    return this.http.delete<void>(`${this.baseUrl}/categories/${id}`);
  }
}
