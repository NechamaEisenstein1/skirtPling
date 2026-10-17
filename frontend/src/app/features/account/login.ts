import { Component, inject, input, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { apiErrorMessage } from '../../shared/utils/api-error';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <div class="container page auth-page">
      <form class="card" [formGroup]="form" (ngSubmit)="submit()" novalidate>
        <h1>Sign in</h1>
        @if (error()) {
          <div class="form-error" role="alert">{{ error() }}</div>
        }
        <div class="field">
          <label for="email">Email</label>
          <input id="email" type="email" formControlName="email" autocomplete="email" />
          @if (form.controls.email.invalid && form.controls.email.touched) {
            <span class="field-error">Please enter a valid email.</span>
          }
        </div>
        <div class="field">
          <label for="password">Password</label>
          <input id="password" type="password" formControlName="password" autocomplete="current-password" />
          @if (form.controls.password.invalid && form.controls.password.touched) {
            <span class="field-error">Please enter your password.</span>
          }
        </div>
        <button class="btn btn-block" type="submit" [disabled]="submitting()">{{ submitting() ? 'Signing in…' : 'Sign in' }}</button>
        <p class="muted">New here? <a routerLink="/account/register" [queryParams]="{ returnUrl: returnUrl() }">Create an account</a></p>
      </form>
    </div>
  `,
  styleUrl: './auth-form.scss',
})
export class Login {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  /** Where to go after signing in (set by the route guards). */
  readonly returnUrl = input<string>('/');

  protected readonly submitting = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly form = inject(NonNullableFormBuilder).group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
  });

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.submitting.set(true);
    this.error.set(null);
    const { email, password } = this.form.getRawValue();
    this.auth.login(email, password).subscribe({
      next: () => this.router.navigateByUrl(this.returnUrl() || '/'),
      error: (error) => {
        this.submitting.set(false);
        this.error.set(apiErrorMessage(error));
      },
    });
  }
}
