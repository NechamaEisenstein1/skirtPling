import { Component, inject, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Address, AddressUpsert } from '../../core/models/account.models';
import { AccountService } from '../../core/services/account.service';
import { ConfirmService } from '../../core/services/confirm.service';
import { NotificationService } from '../../core/services/notification.service';
import { apiErrorMessage } from '../../shared/utils/api-error';
import { AddressForm } from './address-form';

@Component({
  selector: 'app-account-overview',
  imports: [ReactiveFormsModule, RouterLink, AddressForm],
  templateUrl: './account-overview.html',
  styleUrl: './account-overview.scss',
})
export class AccountOverview {
  private readonly account = inject(AccountService);
  private readonly notifications = inject(NotificationService);
  private readonly confirm = inject(ConfirmService);

  protected readonly addresses = rxResource({ stream: () => this.account.getAddresses() });
  /** null = no form open, 'new' = adding, otherwise the address being edited. */
  protected readonly editing = signal<Address | 'new' | null>(null);
  protected readonly busy = signal(false);

  protected readonly profileForm = inject(NonNullableFormBuilder).group({
    email: [{ value: '', disabled: true }],
    fullName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(150)]],
    phoneNumber: [''],
  });

  constructor() {
    this.account.getProfile().subscribe((profile) =>
      this.profileForm.setValue({ email: profile.email, fullName: profile.fullName, phoneNumber: profile.phoneNumber ?? '' }),
    );
  }

  protected saveProfile(): void {
    if (this.profileForm.invalid) {
      this.profileForm.markAllAsTouched();
      return;
    }
    const { fullName, phoneNumber } = this.profileForm.getRawValue();
    this.account.updateProfile(fullName, phoneNumber || null).subscribe({
      next: () => this.notifications.success('Your profile was updated.'),
      error: (error) => this.notifications.error(apiErrorMessage(error)),
    });
  }

  protected editAddress(address: Address): void {
    this.editing.set(address);
  }

  protected saveAddress(values: AddressUpsert): void {
    const editing = this.editing();
    const request = editing && editing !== 'new' ? this.account.updateAddress(editing.id, values) : this.account.createAddress(values);
    this.busy.set(true);
    request.subscribe({
      next: () => {
        this.busy.set(false);
        this.editing.set(null);
        this.addresses.reload();
        this.notifications.success('Address saved.');
      },
      error: (error) => {
        this.busy.set(false);
        this.notifications.error(apiErrorMessage(error));
      },
    });
  }

  protected async deleteAddress(address: Address): Promise<void> {
    if (!(await this.confirm.ask('Delete address?', `“${address.label}” will be removed from your saved addresses.`))) return;
    this.account.deleteAddress(address.id).subscribe({
      next: () => {
        this.addresses.reload();
        this.notifications.success('Address deleted.');
      },
      error: (error) => this.notifications.error(apiErrorMessage(error)),
    });
  }
}
