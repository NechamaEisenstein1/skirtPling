import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { toHttpParams } from '../../shared/utils/http-params';
import { Brand, Category, ProductDetails, ProductListItem, ProductQuery } from '../models/catalog.models';
import { PagedResult } from '../models/paged-result';

/** Public, read-only catalog endpoints. */
@Injectable({ providedIn: 'root' })
export class CatalogService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  searchProducts(query: ProductQuery) {
    return this.http.get<PagedResult<ProductListItem>>(`${this.baseUrl}/products`, { params: toHttpParams(query) });
  }

  getProduct(id: number) {
    return this.http.get<ProductDetails>(`${this.baseUrl}/products/${id}`);
  }

  getBrands() {
    return this.http.get<Brand[]>(`${this.baseUrl}/brands`);
  }

  getCategories() {
    return this.http.get<Category[]>(`${this.baseUrl}/categories`);
  }
}
