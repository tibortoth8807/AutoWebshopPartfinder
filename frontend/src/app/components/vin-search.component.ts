import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { ApiService } from '../core/api.service';
import { Vehicle, VinLookupResponse } from '../core/models';
import { VehicleContextService } from '../core/vehicle-context.service';

@Component({
  selector: 'pf-vin-search',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <form class="vin-form" (submit)="search($event)">
      <label for="vin">Alvázszám (VIN)</label>
      <div class="row">
        <input
          id="vin"
          class="vin-input"
          [value]="vin()"
          (input)="vin.set($any($event.target).value)"
          placeholder="pl. WVWZZZ1JZ3W386752"
          maxlength="25"
          autocomplete="off"
          spellcheck="false"
        />
        <button type="submit" class="btn primary" [disabled]="loading()">
          {{ loading() ? 'Keresés…' : 'Keresés' }}
        </button>
      </div>
      <p class="hint">
        A forgalmi engedély E rovatában található, 17 karakter.
        Próbáld ki:
        @for (sample of samples; track sample.vin) {
          <button type="button" class="link" (click)="vin.set(sample.vin)">{{ sample.label }}</button>
        }
      </p>
    </form>

    @if (error(); as message) {
      <p class="error">{{ message }}</p>
    }

    @if (result(); as r) {
      <div class="vin-result">
        <dl class="facts">
          <div><dt>Gyártó</dt><dd>{{ r.vin.manufacturer ?? 'ismeretlen (' + r.vin.wmi + ')' }}</dd></div>
          <div><dt>Ország</dt><dd>{{ r.vin.country ?? '–' }}</dd></div>
          <div><dt>Lehetséges évjárat</dt><dd>{{ r.vin.possibleModelYears.join(' / ') || '–' }}</dd></div>
          <div><dt>Ellenőrző jegy</dt><dd>{{ checkDigitText[r.vin.checkDigit] }}</dd></div>
          @if (r.nhtsa?.make) {
            <div><dt>NHTSA</dt><dd>{{ r.nhtsa?.make }} {{ r.nhtsa?.model }} {{ r.nhtsa?.modelYear }}</dd></div>
          }
        </dl>

        @if (r.matches.length > 0) {
          <h3>Válaszd ki a motorváltozatot</h3>
          <ul class="matches">
            @for (v of r.matches; track v.id) {
              <li>
                <span>
                  <strong>{{ v.make }} {{ v.generation }}</strong> {{ v.engine }}
                  <small>{{ v.powerKw }} kW · {{ v.fuel === 'Diesel' ? 'dízel' : 'benzin' }} · {{ v.yearFrom }}–{{ v.yearTo }}</small>
                </span>
                <button type="button" class="btn" (click)="choose(v)">Ezt választom</button>
              </li>
            }
          </ul>
        } @else {
          <p class="muted">Ehhez a járműhöz még nincs alkatrész a demó katalógusban.</p>
        }
      </div>
    }
  `,
})
export class VinSearchComponent {
  private readonly api = inject(ApiService);
  private readonly vehicleContext = inject(VehicleContextService);
  private readonly router = inject(Router);

  protected readonly vin = signal('');
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly result = signal<VinLookupResponse | null>(null);

  protected readonly samples = [
    { label: 'Golf IV', vin: 'WVWZZZ1JZ3W386752' },
    { label: 'Octavia II', vin: 'TMBZZZ1ZZ8P012345' },
    { label: 'Astra H', vin: 'W0L0AHL4865012345' },
    { label: 'US Honda', vin: '1HGCM82633A004352' },
  ];

  protected readonly checkDigitText: Record<string, string> = {
    Valid: 'rendben',
    NotUsed: 'nem használt (európai VIN)',
    Invalid: 'hibás',
  };

  protected search(event: Event): void {
    event.preventDefault();
    const vin = this.vin().trim();
    if (!vin) {
      return;
    }

    this.loading.set(true);
    this.error.set(null);
    this.result.set(null);

    this.api.lookupVin(vin).subscribe({
      next: (response) => {
        this.result.set(response);
        this.loading.set(false);
        if (response.matches.length === 1) {
          this.choose(response.matches[0]);
        }
      },
      error: (err: HttpErrorResponse) => {
        this.error.set(
          err.status === 400
            ? `Érvénytelen alvázszám: ${err.error?.detail ?? ''}`
            : 'A keresés most nem sikerült, próbáld újra később.',
        );
        this.loading.set(false);
      },
    });
  }

  protected choose(vehicle: Vehicle): void {
    this.vehicleContext.select(vehicle);
    void this.router.navigate(['/alkatreszek']);
  }
}
