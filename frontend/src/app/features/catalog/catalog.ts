import { Component, inject } from '@angular/core';
import { rxResource, toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { map } from 'rxjs';
import { ProductQuery, ProductSort } from '../../core/models/catalog.models';
import { CatalogService } from '../../core/services/catalog.service';
import { Pagination } from '../../shared/components/pagination';
import { ProductCard } from '../../shared/components/product-card';
import { parseCatalogQuery, toCatalogParams } from './catalog-query';

@Component({
  selector: 'app-catalog',
  imports: [ProductCard, Pagination],
  templateUrl: './catalog.html',
  styleUrl: './catalog.scss',
})
export class Catalog {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly catalog = inject(CatalogService);

  protected readonly query = toSignal(this.route.queryParamMap.pipe(map(parseCatalogQuery)), { requireSync: true });
  protected readonly brands = rxResource({ stream: () => this.catalog.getBrands() });
  protected readonly categories = rxResource({ stream: () => this.catalog.getCategories() });
  protected readonly products = rxResource({
    params: () => this.query(),
    stream: ({ params }) => this.catalog.searchProducts(params),
  });

  protected readonly sortOptions: { value: ProductSort; label: string }[] = [
    { value: 'newest', label: 'Newest' },
    { value: 'price-asc', label: 'Price: low to high' },
    { value: 'price-desc', label: 'Price: high to low' },
    { value: 'name', label: 'Name' },
  ];

  protected toggle(key: 'brandIds' | 'categoryIds', id: number): void {
    const current = this.query()[key] ?? [];
    const next = current.includes(id) ? current.filter((x) => x !== id) : [...current, id];
    this.update({ [key]: next });
  }

  protected setPrice(minPrice: string, maxPrice: string): void {
    this.update({ minPrice: minPrice ? Number(minPrice) : undefined, maxPrice: maxPrice ? Number(maxPrice) : undefined });
  }

  protected setSort(sort: string): void {
    this.update({ sort: sort as ProductSort });
  }

  protected setInStockOnly(inStockOnly: boolean): void {
    this.update({ inStockOnly });
  }

  protected goToPage(page: number): void {
    this.update({ page });
  }

  protected clearFilters(): void {
    this.router.navigate([], { queryParams: {} });
  }

  /** Any filter change resets to page 1 unless a page is given explicitly. */
  private update(change: Partial<ProductQuery>): void {
    const next = { ...this.query(), page: 1, ...change };
    this.router.navigate([], { queryParams: toCatalogParams(next) });
  }
}
