import { Injectable, signal } from '@angular/core';

interface PendingConfirm {
  title: string;
  message: string;
  confirmLabel: string;
  resolve: (confirmed: boolean) => void;
}

/** Asks the user to confirm a destructive action. Rendered by ConfirmDialog in the app shell. */
@Injectable({ providedIn: 'root' })
export class ConfirmService {
  readonly pending = signal<PendingConfirm | null>(null);

  ask(title: string, message: string, confirmLabel = 'Delete'): Promise<boolean> {
    return new Promise((resolve) => this.pending.set({ title, message, confirmLabel, resolve }));
  }

  answer(confirmed: boolean): void {
    this.pending()?.resolve(confirmed);
    this.pending.set(null);
  }
}
