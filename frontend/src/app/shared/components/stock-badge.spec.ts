import { TestBed } from '@angular/core/testing';
import { StockBadge } from './stock-badge';

describe('StockBadge', () => {
  function render(stock: number): string {
    const fixture = TestBed.createComponent(StockBadge);
    fixture.componentRef.setInput('stock', stock);
    fixture.detectChanges();
    return (fixture.nativeElement as HTMLElement).textContent!.trim();
  }

  it('shows sold out, low stock and in stock', () => {
    expect(render(0)).toBe('Sold out');
    expect(render(3)).toBe('Only 3 left');
    expect(render(12)).toBe('In stock');
  });
});
