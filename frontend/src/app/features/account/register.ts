import { Component, inject, input, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { apiErrorMessage } from '../../shared/utils/api-error';

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <div class="container page auth-page">
      <form class="card" [formGroup]="form" (ngSubmit)="submit()" novalidate>
        <h1>Create an account</h1>
        @if (error()) {
          <div class="form-error" role="alert">{{ error() }}</div>
        }
        <div class="field">
          <label for="fullName">Full name</label>
          <input id="fullName" formControlName="fullName" autocomplete="name" />
          @if (form.controls.fullName.invalid && form.controls.fullName.touched) {
            <span class="field-error">Please enter your name.</span>
          }
        </div>
        <div class="field">
          <label for="email">Email</label>
          <input id="email" type="email" formControlName="email" autocomplete="email" />
          @if (form.controls.email.invalid && form.controls.email.touched) {
            <span class="field-error">Please enter a valid email.</span>
          }
        </div>
        <div class="field">
          <label for="password">Password</label>
          <input id="password" type="password" formControlName="password" autocomplete="new-password" />
          <span class="muted hint">At least 8 characters, with an uppercase letter, a lowercase letter and a digit.</span>
          @if (form.controls.password.invalid && form.controls.password.touched) {
            <span class="field-error">Password does not meet the requirements.</span>
          }
        </div>
        <button class="btn btn-block" type="submit" [disabled]="submitting()">{{ submitting() ? 'Creating account…' : 'Create account' }}</button>
        <p class="muted">Already have an account? <a routerLink="/account/login" [queryParams]="{ returnUrl: returnUrl() }">Sign in</a></p>
      </form>
    </div>
  `,
  styleUrl: './auth-form.scss',
})
export class Register {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly returnUrl = input<string>('/');

  protected readonly submitting = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly form = inject(NonNullableFormBuilder).group({
    fullName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(150)]],
    email: ['', [Validators.required, Validators.email]],
    // Mirrors the ASP.NET Core Identity password policy configured in the API.
    password: ['', [Validators.required, Validators.minLength(8), Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$/)]],
  });

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.submitting.set(true);
    this.error.set(null);
    const { fullName, email, password } = this.form.getRawValue();
    this.auth.register(fullName, email, password).subscribe({
      next: () => this.router.navigateByUrl(this.returnUrl() || '/'),
      error: (error) => {
        this.submitting.set(false);
        this.error.set(apiErrorMessage(error));
      },
    });
  }
}
