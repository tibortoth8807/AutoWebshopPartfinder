import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { VehicleSelectorComponent } from '../components/vehicle-selector.component';
import { VinSearchComponent } from '../components/vin-search.component';

@Component({
  selector: 'pf-home-page',
  imports: [VinSearchComponent, VehicleSelectorComponent, RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="hero">
      <h1>Pontosan illő alkatrész – alvázszám alapján</h1>
      <p>Add meg az autód alvázszámát, és csak azokat az alkatrészeket mutatjuk, amelyek biztosan jók rá.</p>
    </section>

    <section class="cards">
      <article class="card">
        <h2>🔎 Keresés alvázszámmal</h2>
        <pf-vin-search />
      </article>

      <article class="card">
        <h2>🚗 Autó kiválasztása listából</h2>
        <pf-vehicle-selector />
        <p class="muted">Vagy <a routerLink="/alkatreszek">böngéssz a teljes kínálatban</a>.</p>
      </article>
    </section>

    <section class="usp">
      <div><strong>Robbantott ábrák</strong><span>Kattints az alkatrészre a rajzon</span></div>
      <div><strong>Gyártói adatok</strong><span>WMI, évjárat, ellenőrző jegy</span></div>
      <div><strong>Gyors szállítás</strong><span>Demó – nem valódi rendelés</span></div>
    </section>
  `,
})
export class HomePage {}
