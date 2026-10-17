import { DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { LegacyStock } from '../../core/models/admin.models';
import { AdminService } from '../../core/services/admin.service';
import { apiErrorMessage } from '../../shared/utils/api-error';

/** Compares shop stock with the physical count in the legacy .NET Framework warehouse system (via WCF). */
@Component({
  selector: 'app-legacy-inventory',
  imports: [DatePipe],
  template: `
    <h1>Legacy warehouse</h1>
    <p class="muted">
      Physical stock counts come from the old warehouse system, a .NET Framework WCF service.
      The API calls it on your behalf; the browser only talks REST.
    </p>

    @if (status.value(); as s) {
      @if (!s.isAvailable) {
        <div class="form-error">
          The legacy warehouse is not configured. Set <code>LegacyWarehouse:Endpoint</code> in the API and start the WCF service
          (see docs/legacy-integration.md).
        </div>
      }
    }

    <form class="toolbar" (submit)="$event.preventDefault(); lookup(sku.value)">
      <input #sku placeholder="SKU, e.g. SP-PL-001" aria-label="SKU" required />
      <button class="btn btn-sm" type="submit" [disabled]="loading()">{{ loading() ? 'Checking…' : 'Compare stock' }}</button>
    </form>

    @if (error()) {
      <div class="form-error" role="alert">{{ error() }}</div>
    }
    @if (result(); as r) {
      <div class="card result">
        <h2>{{ r.sku }}</h2>
        <dl>
          <dt>Shop stock</dt><dd>{{ r.shopStock }}</dd>
          <dt>Warehouse on hand</dt><dd>{{ r.warehouseOnHand }} ({{ r.warehouseCode }})</dd>
          <dt>Last counted</dt><dd>{{ r.lastCountedAt | date: 'medium' }}</dd>
          <dt>Difference</dt>
          <dd><span class="badge" [class.success]="r.difference === 0" [class.warning]="r.difference !== 0">{{ r.difference > 0 ? '+' : '' }}{{ r.difference }}</span></dd>
        </dl>
      </div>
    }
  `,
  styles: `
    .result { max-width: 480px; }
    dl { display: grid; grid-template-columns: 160px 1fr; gap: 8px; margin: 0; }
    dt { color: var(--muted); }
    dd { margin: 0; }
  `,
})
export class LegacyInventory {
  private readonly admin = inject(AdminService);

  protected readonly status = rxResource({ stream: () => this.admin.getLegacyStatus() });
  protected readonly result = signal<LegacyStock | null>(null);
  protected readonly error = signal<string | null>(null);
  protected readonly loading = signal(false);

  protected lookup(sku: string): void {
    if (!sku.trim()) return;
    this.loading.set(true);
    this.error.set(null);
    this.result.set(null);
    this.admin.compareLegacyStock(sku.trim()).subscribe({
      next: (stock) => {
        this.loading.set(false);
        this.result.set(stock);
      },
      error: (error) => {
        this.loading.set(false);
        this.error.set(apiErrorMessage(error));
      },
    });
  }
}
