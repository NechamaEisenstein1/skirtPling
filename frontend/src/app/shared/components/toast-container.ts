import { Component, inject } from '@angular/core';
import { NotificationService } from '../../core/services/notification.service';

@Component({
  selector: 'app-toast-container',
  template: `
    <div class="toasts" aria-live="polite">
      @for (toast of notifications.toasts(); track toast.id) {
        <div class="toast" [class.error]="toast.kind === 'error'" role="status">
          <span>{{ toast.message }}</span>
          <button type="button" aria-label="Dismiss" (click)="notifications.dismiss(toast.id)">×</button>
        </div>
      }
    </div>
  `,
  styles: `
    .toasts { position: fixed; right: 20px; bottom: 20px; display: flex; flex-direction: column; gap: 10px; z-index: 100; max-width: min(380px, calc(100vw - 40px)); }
    .toast { display: flex; gap: 12px; align-items: flex-start; justify-content: space-between; padding: 12px 16px;
      background: var(--ink); color: #fff; border-radius: 8px; box-shadow: var(--shadow); }
    .toast.error { background: var(--danger); }
    button { background: none; border: none; color: inherit; font-size: 1.2rem; line-height: 1; cursor: pointer; }
  `,
})
export class ToastContainer {
  protected readonly notifications = inject(NotificationService);
}
