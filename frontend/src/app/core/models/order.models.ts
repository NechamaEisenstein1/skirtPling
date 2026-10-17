export type OrderStatus = 'PendingPayment' | 'Paid' | 'Shipped' | 'Delivered' | 'Cancelled';

export interface ShippingAddress {
  fullName: string;
  phone: string;
  street: string;
  city: string;
  postalCode: string;
  country: string;
}

export interface OrderItem {
  productId: number;
  sku: string;
  productName: string;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
}

export interface Order {
  id: number;
  orderNumber: string;
  status: OrderStatus;
  customerEmail: string;
  createdAt: string;
  shippingAddress: ShippingAddress;
  items: OrderItem[];
  subtotal: number;
  shippingCost: number;
  total: number;
  paymentReference: string | null;
}

export interface OrderSummary {
  id: number;
  orderNumber: string;
  status: OrderStatus;
  customerEmail: string;
  createdAt: string;
  itemCount: number;
  total: number;
}

export interface Checkout {
  shippingAddress: ShippingAddress;
  paymentToken: string;
}

/** Test tokens understood by the backend's sandbox payment gateway. No card data ever leaves the browser. */
export const SANDBOX_PAYMENT_TOKENS = {
  approved: 'tok_test_approved',
  declined: 'tok_test_declined',
} as const;
