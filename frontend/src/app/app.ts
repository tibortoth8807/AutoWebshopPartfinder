import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { CartService } from './core/cart.service';
import { VehicleContextService } from './core/vehicle-context.service';

@Component({
  selector: 'pf-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <header class="topbar">
      <div class="container topbar-inner">
        <a routerLink="/" class="brand">
          <img src="logo.svg" alt="" width="34" height="34" />
          <span>Part<strong>Finder</strong></span>
        </a>

        <nav class="nav">
          <a routerLink="/alkatreszek" routerLinkActive="active">Alkatrészek</a>
        </nav>

        <div class="vehicle-chip">
          @if (vehicle.selected(); as v) {
            <span class="chip-label">Az autóm:</span>
            <strong>{{ v.displayName }}</strong>
            <button type="button" class="link" (click)="vehicle.select(null)">Csere</button>
          } @else {
            <a routerLink="/">Válassz autót</a>
          }
        </div>

        <a routerLink="/kosar" class="cart" aria-label="Kosár">
          🛒 <span class="badge">{{ cart.count() }}</span>
        </a>
      </div>
    </header>

    <main class="container">
      <router-outlet />
    </main>

    <footer class="footer">
      <div class="container">
        PartFinder – portfólió demó. Minden márka, cikkszám és ár kitalált; a VIN-szabályok egyszerűsítettek.
      </div>
    </footer>
  `,
})
export class App {
  protected readonly vehicle = inject(VehicleContextService);
  protected readonly cart = inject(CartService);
}
