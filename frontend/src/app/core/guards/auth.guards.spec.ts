import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree, provideRouter } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { adminGuard, signedInGuard } from './auth.guards';

describe('route guards', () => {
  const isSignedIn = signal(false);
  const isAdmin = signal(false);
  const state = { url: '/admin/products' } as RouterStateSnapshot;
  const route = {} as ActivatedRouteSnapshot;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideRouter([]), { provide: AuthService, useValue: { isSignedIn, isAdmin } }],
    });
  });

  const run = (guard: typeof adminGuard) => TestBed.runInInjectionContext(() => guard(route, state));

  it('sends anonymous visitors to login with a return url', () => {
    isSignedIn.set(false);
    const result = run(signedInGuard) as UrlTree;
    expect(result.toString()).toBe('/account/login?returnUrl=%2Fadmin%2Fproducts');
  });

  it('keeps customers out of the admin area', () => {
    isSignedIn.set(true);
    isAdmin.set(false);
    expect((run(adminGuard) as UrlTree).toString()).toBe('/');
  });

  it('lets admins in', () => {
    isSignedIn.set(true);
    isAdmin.set(true);
    expect(run(adminGuard)).toBe(true);
  });
});
