import { CurrencyPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ProductListItem, ProductQuery, ProductSort } from '../../core/models/catalog.models';
import { AdminCatalogService } from '../../core/services/admin-catalog.service';
import { ConfirmService } from '../../core/services/confirm.service';
import { NotificationService } from '../../core/services/notification.service';
import { Pagination } from '../../shared/components/pagination';
import { StockBadge } from '../../shared/components/stock-badge';
import { apiErrorMessage } from '../../shared/utils/api-error';

@Component({
  selector: 'app-product-list',
  imports: [CurrencyPipe, RouterLink, Pagination, StockBadge],
  template: `
    <div class="page-header">
      <h1>Products</h1>
      <a class="btn btn-accent" routerLink="/admin/products/new">Add product</a>
    </div>

    <form class="toolbar" (submit)="$event.preventDefault(); search(term.value)">
      <input #term type="search" placeholder="Search name, SKU or brand" aria-label="Search products" [value]="query().search ?? ''" />
      <select aria-label="Sort" [value]="query().sort" (change)="setSort($any($event.target).value)">
        <option value="newest">Newest</option>
        <option value="name">Name</option>
        <option value="stock">Lowest stock first</option>
        <option value="price-desc">Highest price</option>
      </select>
      <button class="btn btn-outline btn-sm" type="submit">Search</button>
    </form>

    @if (products.error()) {
      <div class="state">Couldn't load products. <button class="btn-link" (click)="products.reload()">Retry</button></div>
    } @else if (products.value(); as result) {
      <div class="table-wrap">
        <table>
          <thead>
            <tr><th></th><th>Product</th><th>Brand / category</th><th class="num">Price</th><th>Stock</th><th>Status</th><th></th></tr>
          </thead>
          <tbody>
            @for (p of result.items; track p.id) {
              <tr>
                <td><img class="thumb" [src]="p.imageUrl ?? '/images/placeholder.svg'" alt="" /></td>
                <td><a [routerLink]="['/admin/products', p.id]">{{ p.name }}</a><br /><span class="muted small">{{ p.sku }}</span></td>
                <td class="small">{{ p.brandName }}<br /><span class="muted">{{ p.categoryName }}</span></td>
                <td class="num">{{ p.price | currency: 'ILS' }}</td>
                <td>
                  <form class="stock" (submit)="$event.preventDefault(); saveStock(p, stock.value)">
                    <input #stock type="number" min="0" [value]="p.stockQuantity" [attr.aria-label]="'Stock for ' + p.name" />
                    <button class="btn-link" type="submit">Save</button>
                  </form>
                  <app-stock-badge [stock]="p.stockQuantity" />
                </td>
                <td><span class="badge" [class.success]="p.isActive">{{ p.isActive ? 'Active' : 'Hidden' }}</span></td>
                <td>
                  <a class="btn-link" [routerLink]="['/admin/products', p.id]">Edit</a>
                  <button class="btn-link danger" type="button" (click)="remove(p)">Delete</button>
                </td>
              </tr>
            } @empty {
              <tr><td colspan="7" class="muted">No products found.</td></tr>
            }
          </tbody>
        </table>
      </div>
      <app-pagination [page]="result.page" [pageSize]="result.pageSize" [totalCount]="result.totalCount" (pageChange)="setPage($event)" />
    } @else {
      <div class="state"><div class="spinner"></div></div>
    }
  `,
  styles: `
    .thumb { width: 44px; height: 55px; object-fit: cover; border-radius: 6px; }
    .small { font-size: .85rem; }
    .stock { display: flex; align-items: center; gap: 4px; margin-bottom: 4px; }
    .stock input { width: 76px; padding: 6px 8px; }
  `,
})
export class ProductList {
  private readonly catalog = inject(AdminCatalogService);
  private readonly notifications = inject(NotificationService);
  private readonly confirm = inject(ConfirmService);

  protected readonly query = signal<ProductQuery>({
    sort: (inject(ActivatedRoute).snapshot.queryParamMap.get('sort') as ProductSort) ?? 'newest',
    page: 1,
    pageSize: 15,
  });
  protected readonly products = rxResource({
    params: () => this.query(),
    stream: ({ params }) => this.catalog.searchProducts(params),
  });

  protected search(search: string): void {
    this.query.update((q) => ({ ...q, search: search.trim() || undefined, page: 1 }));
  }

  protected setSort(sort: ProductSort): void {
    this.query.update((q) => ({ ...q, sort, page: 1 }));
  }

  protected setPage(page: number): void {
    this.query.update((q) => ({ ...q, page }));
  }

  protected saveStock(product: ProductListItem, value: string): void {
    const stock = Number(value);
    if (!Number.isInteger(stock) || stock < 0) {
      this.notifications.error('Stock must be a whole number of 0 or more.');
      return;
    }
    this.catalog.updateStock(product.id, stock).subscribe({
      next: () => {
        this.notifications.success(`Stock for ${product.name} set to ${stock}.`);
        this.products.reload();
      },
      error: (error) => this.notifications.error(apiErrorMessage(error)),
    });
  }

  protected async remove(product: ProductListItem): Promise<void> {
    const confirmed = await this.confirm.ask('Delete product?', `“${product.name}” will be permanently removed from the catalog.`);
    if (!confirmed) return;
    this.catalog.deleteProduct(product.id).subscribe({
      next: () => {
        this.notifications.success(`${product.name} was deleted.`);
        this.products.reload();
      },
      error: (error) => this.notifications.error(apiErrorMessage(error)),
    });
  }
}
