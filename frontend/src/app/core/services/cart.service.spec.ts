import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { environment } from '../../../environments/environment';
import { apiInterceptor } from '../interceptors/api.interceptor';
import { Cart } from '../models/cart.models';
import { CartService } from './cart.service';
import { GuestCartIdService } from './guest-cart-id.service';

describe('CartService', () => {
  let service: CartService;
  let http: HttpTestingController;

  const cart: Cart = {
    items: [{ productId: 1, sku: 'SP-1', name: 'Skirt', imageUrl: null, unitPrice: 100, quantity: 2, availableStock: 5, lineTotal: 200 }],
    subtotal: 200,
    shippingCost: 29.9,
    total: 229.9,
    itemCount: 2,
  };

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideRouter([]), provideHttpClient(withInterceptors([apiInterceptor])), provideHttpClientTesting()],
    });
    service = TestBed.inject(CartService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('adds an item and exposes the server cart', () => {
    service.add(1, 2).subscribe();

    const request = http.expectOne(`${environment.apiUrl}/cart/items`);
    expect(request.request.body).toEqual({ productId: 1, quantity: 2 });
    request.flush(cart);

    expect(service.itemCount()).toBe(2);
    expect(service.cart().total).toBe(229.9);
  });

  it('sends the guest cart id with every API call', () => {
    service.load();

    const request = http.expectOne(`${environment.apiUrl}/cart`);
    expect(request.request.headers.get('X-Guest-Cart-Id')).toBe(TestBed.inject(GuestCartIdService).id);
    expect(request.request.headers.has('Authorization')).toBe(false);
    request.flush(cart);
  });
});
