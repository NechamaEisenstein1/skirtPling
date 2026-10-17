import { Component, effect, inject, input, output } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Address, AddressUpsert } from '../../core/models/account.models';

/** Add/edit form for a saved address. The parent decides what to do with the result. */
@Component({
  selector: 'app-address-form',
  imports: [ReactiveFormsModule],
  template: `
    <form [formGroup]="form" (ngSubmit)="submit()" novalidate>
      <div class="form-grid">
        <div class="field"><label for="label">Label</label><input id="label" formControlName="label" placeholder="Home, Work…" /></div>
        <div class="field"><label for="a-name">Full name</label><input id="a-name" formControlName="fullName" /></div>
        <div class="field"><label for="a-phone">Phone</label><input id="a-phone" formControlName="phone" /></div>
        <div class="field full"><label for="a-street">Street and number</label><input id="a-street" formControlName="street" /></div>
        <div class="field"><label for="a-city">City</label><input id="a-city" formControlName="city" /></div>
        <div class="field"><label for="a-postal">Postal code</label><input id="a-postal" formControlName="postalCode" /></div>
        <div class="field"><label for="a-country">Country</label><input id="a-country" formControlName="country" /></div>
      </div>
      @if (form.invalid && form.touched) {
        <p class="field-error">Please fill in all fields.</p>
      }
      <div class="form-actions">
        <button class="btn" type="submit" [disabled]="busy()">Save address</button>
        <button class="btn btn-outline" type="button" (click)="cancelled.emit()">Cancel</button>
      </div>
    </form>
  `,
})
export class AddressForm {
  readonly address = input<Address | null>(null);
  readonly busy = input(false);
  readonly saved = output<AddressUpsert>();
  readonly cancelled = output<void>();

  protected readonly form = inject(NonNullableFormBuilder).group({
    label: ['', [Validators.required, Validators.maxLength(50)]],
    fullName: ['', [Validators.required, Validators.maxLength(150)]],
    phone: ['', [Validators.required, Validators.pattern(/^[+\d][\d\s-]{6,}$/)]],
    street: ['', [Validators.required, Validators.maxLength(200)]],
    city: ['', [Validators.required, Validators.maxLength(100)]],
    postalCode: ['', [Validators.required, Validators.maxLength(20)]],
    country: ['Israel', [Validators.required, Validators.maxLength(100)]],
  });

  constructor() {
    effect(() => {
      const address = this.address();
      if (address) {
        const { id: _id, ...values } = address;
        this.form.setValue(values);
      }
    });
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.saved.emit(this.form.getRawValue());
  }
}
