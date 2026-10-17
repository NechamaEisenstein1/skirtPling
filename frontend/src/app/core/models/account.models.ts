export interface User {
  id: string;
  email: string;
  fullName: string;
  roles: string[];
}

export interface AuthResult {
  token: string;
  expiresAt: string;
  user: User;
}

export interface Profile {
  id: string;
  email: string;
  fullName: string;
  phoneNumber: string | null;
}

export interface Address {
  id: number;
  label: string;
  fullName: string;
  phone: string;
  street: string;
  city: string;
  postalCode: string;
  country: string;
}

export type AddressUpsert = Omit<Address, 'id'>;
