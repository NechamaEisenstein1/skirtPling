import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from '../services/auth.service';
import { GuestCartIdService } from '../services/guest-cart-id.service';

/** Adds the JWT and guest-cart id to API calls, and sends the user to sign in when a session has expired. */
export const apiInterceptor: HttpInterceptorFn = (req, next) => {
  if (!req.url.startsWith(environment.apiUrl)) return next(req);

  const auth = inject(AuthService);
  const router = inject(Router);
  const guestCartId = inject(GuestCartIdService);

  const headers: Record<string, string> = { 'X-Guest-Cart-Id': guestCartId.id };
  if (auth.token) headers['Authorization'] = `Bearer ${auth.token}`;

  return next(req.clone({ setHeaders: headers })).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && auth.token) {
        auth.logout();
        router.navigate(['/account/login'], { queryParams: { returnUrl: router.url } });
      }
      return throwError(() => error);
    }),
  );
};
