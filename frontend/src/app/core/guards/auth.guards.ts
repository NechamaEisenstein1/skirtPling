import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

/** Sends anonymous visitors to the login page, remembering where they wanted to go. */
export const signedInGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthService);
  return auth.isSignedIn() || inject(Router).createUrlTree(['/account/login'], { queryParams: { returnUrl: state.url } });
};

/** UI-level guard only: the API enforces the Admin role on every admin endpoint. */
export const adminGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (auth.isAdmin()) return true;
  if (auth.isSignedIn()) return router.createUrlTree(['/']);
  return router.createUrlTree(['/account/login'], { queryParams: { returnUrl: state.url } });
};
