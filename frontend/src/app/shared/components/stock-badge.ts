import { Component, computed, input } from '@angular/core';

/** Shows availability the same way everywhere: in stock, low stock, or sold out. */
@Component({
  selector: 'app-stock-badge',
  template: `<span class="badge" [class]="view().css">{{ view().label }}</span>`,
})
export class StockBadge {
  static readonly lowStockThreshold = 5;
  readonly stock = input.required<number>();

  protected readonly view = computed(() => {
    const stock = this.stock();
    if (stock <= 0) return { label: 'Sold out', css: 'badge danger' };
    if (stock <= StockBadge.lowStockThreshold) return { label: `Only ${stock} left`, css: 'badge warning' };
    return { label: 'In stock', css: 'badge success' };
  });
}
