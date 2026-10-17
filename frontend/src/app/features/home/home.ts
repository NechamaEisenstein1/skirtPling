import { Component, inject } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { CatalogService } from '../../core/services/catalog.service';
import { ProductCard } from '../../shared/components/product-card';

@Component({
  selector: 'app-home',
  imports: [RouterLink, ProductCard],
  templateUrl: './home.html',
  styleUrl: './home.scss',
})
export class Home {
  private readonly catalog = inject(CatalogService);

  protected readonly categories = rxResource({ stream: () => this.catalog.getCategories() });
  protected readonly brands = rxResource({ stream: () => this.catalog.getBrands() });
  protected readonly newArrivals = rxResource({
    stream: () => this.catalog.searchProducts({ sort: 'newest', pageSize: 4, inStockOnly: true }),
  });
}
