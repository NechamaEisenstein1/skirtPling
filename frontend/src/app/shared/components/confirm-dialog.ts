import { Component, inject } from '@angular/core';
import { ConfirmService } from '../../core/services/confirm.service';

@Component({
  selector: 'app-confirm-dialog',
  template: `
    @if (confirm.pending(); as pending) {
      <div class="backdrop" (click)="confirm.answer(false)">
        <div class="dialog" role="alertdialog" aria-modal="true" aria-labelledby="confirm-title" (click)="$event.stopPropagation()">
          <h2 id="confirm-title">{{ pending.title }}</h2>
          <p>{{ pending.message }}</p>
          <div class="form-actions">
            <button class="btn btn-outline" type="button" (click)="confirm.answer(false)">Cancel</button>
            <button class="btn btn-danger" type="button" (click)="confirm.answer(true)">{{ pending.confirmLabel }}</button>
          </div>
        </div>
      </div>
    }
  `,
  styles: `
    .backdrop { position: fixed; inset: 0; background: rgb(29 36 51 / 45%); display: grid; place-items: center; z-index: 200; padding: 16px; }
    .dialog { background: var(--surface); border-radius: var(--radius); padding: 24px; max-width: 420px; width: 100%; box-shadow: var(--shadow); }
    .form-actions { justify-content: flex-end; }
  `,
})
export class ConfirmDialog {
  protected readonly confirm = inject(ConfirmService);
}
