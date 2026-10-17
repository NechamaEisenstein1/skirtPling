import { Component, OnInit, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Footer } from './core/layout/footer';
import { Header } from './core/layout/header';
import { CartService } from './core/services/cart.service';
import { ConfirmDialog } from './shared/components/confirm-dialog';
import { ToastContainer } from './shared/components/toast-container';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Header, Footer, ToastContainer, ConfirmDialog],
  template: `
    <app-header />
    <main>
      <router-outlet />
    </main>
    <app-footer />
    <app-toast-container />
    <app-confirm-dialog />
  `,
  styles: `
    :host { display: flex; flex-direction: column; min-height: 100vh; }
    main { flex: 1; }
  `,
})
export class App implements OnInit {
  private readonly cart = inject(CartService);

  ngOnInit(): void {
    this.cart.load();
  }
}
