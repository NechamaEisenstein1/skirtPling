import { Component, computed, input, output } from '@angular/core';

@Component({
  selector: 'app-pagination',
  template: `
    @if (totalPages() > 1) {
      <nav class="pagination" aria-label="Pagination">
        <button class="btn btn-outline btn-sm" type="button" [disabled]="page() <= 1" (click)="pageChange.emit(page() - 1)">Previous</button>
        <span class="muted">Page {{ page() }} of {{ totalPages() }}</span>
        <button class="btn btn-outline btn-sm" type="button" [disabled]="page() >= totalPages()" (click)="pageChange.emit(page() + 1)">Next</button>
      </nav>
    }
  `,
  styles: `.pagination { display: flex; align-items: center; justify-content: center; gap: 16px; margin-top: 28px; }`,
})
export class Pagination {
  readonly page = input.required<number>();
  readonly pageSize = input.required<number>();
  readonly totalCount = input.required<number>();
  readonly pageChange = output<number>();

  protected readonly totalPages = computed(() => Math.max(1, Math.ceil(this.totalCount() / this.pageSize())));
}
