import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-not-found',
  imports: [RouterLink],
  template: `
    <div class="container state">
      <h2>Page not found</h2>
      <p>The page you are looking for does not exist or was moved.</p>
      <a class="btn" routerLink="/shop">Continue shopping</a>
    </div>
  `,
})
export class NotFound {}
