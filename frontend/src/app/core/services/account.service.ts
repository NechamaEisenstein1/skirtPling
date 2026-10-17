import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Address, AddressUpsert, Profile } from '../models/account.models';

/** The signed-in user's profile and saved addresses. */
@Injectable({ providedIn: 'root' })
export class AccountService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/account`;

  getProfile() {
    return this.http.get<Profile>(`${this.baseUrl}/profile`);
  }

  updateProfile(fullName: string, phoneNumber: string | null) {
    return this.http.put<Profile>(`${this.baseUrl}/profile`, { fullName, phoneNumber });
  }

  getAddresses() {
    return this.http.get<Address[]>(`${this.baseUrl}/addresses`);
  }

  createAddress(address: AddressUpsert) {
    return this.http.post<Address>(`${this.baseUrl}/addresses`, address);
  }

  updateAddress(id: number, address: AddressUpsert) {
    return this.http.put<Address>(`${this.baseUrl}/addresses/${id}`, address);
  }

  deleteAddress(id: number) {
    return this.http.delete<void>(`${this.baseUrl}/addresses/${id}`);
  }
}
