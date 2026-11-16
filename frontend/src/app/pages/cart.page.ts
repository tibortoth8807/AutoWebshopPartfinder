import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CartService } from '../core/cart.service';

@Component({
  selector: 'pf-cart-page',
  imports: [CurrencyPipe, RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <h1>Kosár</h1>

    @if (cart.items().length === 0) {
      <p>A kosarad üres. <a routerLink="/alkatreszek">Nézz körül az alkatrészek között.</a></p>
    } @else {
      <table class="cart-table">
        <thead>
          <tr><th>Termék</th><th>Egységár</th><th>Mennyiség</th><th>Összesen</th><th></th></tr>
        </thead>
        <tbody>
          @for (item of cart.items(); track item.part.id) {
            <tr>
              <td>
                <strong>{{ item.part.brand }}</strong> {{ item.part.name }}
                <br /><small class="muted">{{ item.part.sku }}</small>
              </td>
              <td>{{ item.part.price | currency: 'HUF' : 'symbol-narrow' : '1.0-0' }}</td>
              <td>
                <input
                  type="number"
                  min="0"
                  class="qty"
                  [value]="item.quantity"
                  (change)="cart.setQuantity(item.part.id, +$any($event.target).value)"
                />
              </td>
              <td>{{ item.part.price * item.quantity | currency: 'HUF' : 'symbol-narrow' : '1.0-0' }}</td>
              <td><button type="button" class="link" (click)="cart.setQuantity(item.part.id, 0)">Törlés</button></td>
            </tr>
          }
        </tbody>
      </table>

      <div class="cart-summary">
        <span>Végösszeg: <strong>{{ cart.total() | currency: 'HUF' : 'symbol-narrow' : '1.0-0' }}</strong></span>
        <button type="button" class="btn" (click)="cart.clear()">Kosár ürítése</button>
        <button type="button" class="btn primary" disabled title="A rendelés a következő fejlesztési lépés">Megrendelés (hamarosan)</button>
      </div>
    }
  `,
})
export class CartPage {
  protected readonly cart = inject(CartService);
}
