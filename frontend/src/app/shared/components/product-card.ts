import { CurrencyPipe } from '@angular/common';
import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ProductListItem } from '../../core/models/catalog.models';
import { StockBadge } from './stock-badge';

@Component({
  selector: 'app-product-card',
  imports: [RouterLink, CurrencyPipe, StockBadge],
  template: `
    <a class="card" [routerLink]="['/products', product().id]">
      <div class="image">
        <img [src]="product().imageUrl ?? '/images/placeholder.svg'" [alt]="product().name" loading="lazy" />
      </div>
      <div class="body">
        <span class="brand">{{ product().brandName }}</span>
        <h3>{{ product().name }}</h3>
        <div class="meta">
          <strong>{{ product().price | currency: 'ILS' }}</strong>
          <app-stock-badge [stock]="product().stockQuantity" />
        </div>
      </div>
    </a>
  `,
  styles: `
    .card { display: flex; flex-direction: column; height: 100%; background: var(--surface); border: 1px solid var(--line);
      border-radius: var(--radius); overflow: hidden; color: inherit; transition: box-shadow .2s, transform .2s; }
    .card:hover { text-decoration: none; box-shadow: var(--shadow); transform: translateY(-2px); }
    .image { aspect-ratio: 4 / 5; background: var(--accent-soft); }
    .image img { width: 100%; height: 100%; object-fit: cover; }
    .body { padding: 14px 16px 18px; display: flex; flex-direction: column; gap: 4px; flex: 1; }
    .brand { font-size: .75rem; text-transform: uppercase; letter-spacing: .08em; color: var(--muted); }
    h3 { font-family: var(--font-body); font-size: 1rem; font-weight: 500; margin: 0; }
    .meta { margin-top: auto; display: flex; justify-content: space-between; align-items: center; padding-top: 8px; }
  `,
})
export class ProductCard {
  readonly product = input.required<ProductListItem>();
}
