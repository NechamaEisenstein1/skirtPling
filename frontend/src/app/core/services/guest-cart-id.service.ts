import { Injectable } from '@angular/core';

const STORAGE_KEY = 'sp.guestCartId';

/** A random id that lets a visitor keep a server-side cart before signing in. */
@Injectable({ providedIn: 'root' })
export class GuestCartIdService {
  readonly id: string = localStorage.getItem(STORAGE_KEY) ?? this.createId();

  private createId(): string {
    const id = crypto.randomUUID();
    localStorage.setItem(STORAGE_KEY, id);
    return id;
  }
}
