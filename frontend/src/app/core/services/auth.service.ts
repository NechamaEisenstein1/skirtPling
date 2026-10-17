import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResult, User } from '../models/account.models';
import { CartService } from './cart.service';

const STORAGE_KEY = 'sp.auth';

/**
 * Holds the signed-in user and their JWT. The token is kept in localStorage for simplicity;
 * the backend still checks every request, so hiding UI here is a convenience, not security.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly cart = inject(CartService);
  private readonly session = signal<AuthResult | null>(this.restoreSession());

  readonly user = computed<User | null>(() => this.session()?.user ?? null);
  readonly isSignedIn = computed(() => this.session() !== null);
  readonly isAdmin = computed(() => this.user()?.roles.includes('Admin') ?? false);

  get token(): string | null {
    return this.session()?.token ?? null;
  }

  login(email: string, password: string) {
    return this.http
      .post<AuthResult>(`${environment.apiUrl}/auth/login`, { email, password })
      .pipe(tap((result) => this.startSession(result)));
  }

  register(fullName: string, email: string, password: string) {
    return this.http
      .post<AuthResult>(`${environment.apiUrl}/auth/register`, { fullName, email, password })
      .pipe(tap((result) => this.startSession(result)));
  }

  logout(): void {
    this.session.set(null);
    localStorage.removeItem(STORAGE_KEY);
    this.cart.load();
    this.router.navigateByUrl('/');
  }

  private startSession(result: AuthResult): void {
    this.session.set(result);
    localStorage.setItem(STORAGE_KEY, JSON.stringify(result));
    // Anything the visitor added as a guest moves into their account cart.
    this.cart.mergeGuestCart();
  }

  private restoreSession(): AuthResult | null {
    try {
      const saved = JSON.parse(localStorage.getItem(STORAGE_KEY) ?? 'null') as AuthResult | null;
      return saved && new Date(saved.expiresAt) > new Date() ? saved : null;
    } catch {
      return null;
    }
  }
}
