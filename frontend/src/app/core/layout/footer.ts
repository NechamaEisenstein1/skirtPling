import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-footer',
  imports: [RouterLink],
  template: `
    <footer class="footer">
      <div class="container cols">
        <div>
          <div class="logo">Skirt<span>Pling</span></div>
          <p class="muted">Skirts from brands you love, delivered across Israel.</p>
        </div>
        <div>
          <h3>Shop</h3>
          <a routerLink="/shop">All skirts</a>
          <a routerLink="/shop" [queryParams]="{ inStockOnly: true }">In stock now</a>
        </div>
        <div>
          <h3>Help</h3>
          <span class="muted">Free shipping from ₪300</span>
          <span class="muted">Demo store: no real payments</span>
        </div>
      </div>
    </footer>
  `,
  styles: `
    .footer { background: var(--ink); color: #d6d9e0; padding: 40px 0; margin-top: 48px; }
    .cols { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 24px; }
    .logo { font-family: var(--font-display); font-size: 1.4rem; color: #fff; span { color: #e3a28f; } }
    h3 { color: #fff; font-size: 1rem; }
    a, span { display: block; margin-bottom: 6px; }
    .logo span { display: inline; }
    a { color: #d6d9e0; }
    .muted { color: #a3a9b6; }
  `,
})
export class Footer {}
