import { CurrencyPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Address } from '../../core/models/account.models';
import { SANDBOX_PAYMENT_TOKENS } from '../../core/models/order.models';
import { AccountService } from '../../core/services/account.service';
import { AuthService } from '../../core/services/auth.service';
import { CartService } from '../../core/services/cart.service';
import { OrderService } from '../../core/services/order.service';
import { apiErrorMessage } from '../../shared/utils/api-error';

@Component({
  selector: 'app-checkout',
  imports: [CurrencyPipe, ReactiveFormsModule, RouterLink],
  templateUrl: './checkout.html',
  styleUrl: './checkout.scss',
})
export class CheckoutPage {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly router = inject(Router);
  private readonly orders = inject(OrderService);
  private readonly cartService = inject(CartService);
  private readonly account = inject(AccountService);

  protected readonly cart = this.cartService.cart;
  protected readonly savedAddresses = rxResource({ stream: () => this.account.getAddresses() });
  protected readonly submitting = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly paymentTokens = SANDBOX_PAYMENT_TOKENS;

  protected readonly form = this.fb.group({
    shippingAddress: this.fb.group({
      fullName: [inject(AuthService).user()?.fullName ?? '', [Validators.required, Validators.maxLength(150)]],
      phone: ['', [Validators.required, Validators.pattern(/^[+\d][\d\s-]{6,}$/)]],
      street: ['', [Validators.required, Validators.maxLength(200)]],
      city: ['', [Validators.required, Validators.maxLength(100)]],
      postalCode: ['', [Validators.required, Validators.maxLength(20)]],
      country: ['Israel', [Validators.required, Validators.maxLength(100)]],
    }),
    paymentToken: [SANDBOX_PAYMENT_TOKENS.approved as string, Validators.required],
  });

  protected useAddress(address: Address): void {
    const { id: _id, label: _label, ...shipping } = address;
    this.form.controls.shippingAddress.setValue(shipping);
  }

  protected isInvalid(name: keyof typeof this.form.controls.shippingAddress.controls): boolean {
    const control = this.form.controls.shippingAddress.controls[name];
    return control.invalid && control.touched;
  }

  protected placeOrder(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.error.set(null);
    this.orders.checkout(this.form.getRawValue()).subscribe({
      next: (order) => {
        this.cartService.reset();
        this.router.navigate(['/checkout/confirmation', order.id]);
      },
      error: (error) => {
        this.submitting.set(false);
        this.error.set(apiErrorMessage(error));
        this.cartService.load();
      },
    });
  }
}
