import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { ApiService } from '../core/api.service';
import { VehicleContextService } from '../core/vehicle-context.service';

/** Classic make → model → engine selector for users who don't have the VIN at hand. */
@Component({
  selector: 'pf-vehicle-selector',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="selector">
      <label>
        Márka
        <select (change)="selectMake($any($event.target).value)">
          <option value="">– válassz –</option>
          @for (m of makes(); track m) {
            <option [value]="m" [selected]="m === make()">{{ m }}</option>
          }
        </select>
      </label>

      <label>
        Modell
        <select [disabled]="!make()" (change)="selectGeneration($any($event.target).value)">
          <option value="">– válassz –</option>
          @for (g of generations(); track g) {
            <option [value]="g" [selected]="g === generation()">{{ g }}</option>
          }
        </select>
      </label>

      <label>
        Motor
        <select [disabled]="!generation()" (change)="vehicleId.set(+$any($event.target).value || null)">
          <option value="">– válassz –</option>
          @for (v of engines(); track v.id) {
            <option [value]="v.id">{{ v.engine }} ({{ v.powerKw }} kW, {{ v.yearFrom }}–{{ v.yearTo }})</option>
          }
        </select>
      </label>

      <button type="button" class="btn primary" [disabled]="!selectedVehicle()" (click)="confirm()">Alkatrészek</button>
    </div>
  `,
})
export class VehicleSelectorComponent {
  private readonly api = inject(ApiService);
  private readonly vehicleContext = inject(VehicleContextService);
  private readonly router = inject(Router);

  private readonly vehicles = toSignal(this.api.vehicles(), { initialValue: [] });

  protected readonly make = signal<string | null>(null);
  protected readonly generation = signal<string | null>(null);
  protected readonly vehicleId = signal<number | null>(null);

  protected readonly makes = computed(() => [...new Set(this.vehicles().map((v) => v.make))].sort());
  protected readonly generations = computed(() => [
    ...new Set(this.vehicles().filter((v) => v.make === this.make()).map((v) => v.generation)),
  ]);
  protected readonly engines = computed(() =>
    this.vehicles().filter((v) => v.make === this.make() && v.generation === this.generation()),
  );
  protected readonly selectedVehicle = computed(() => this.vehicles().find((v) => v.id === this.vehicleId()) ?? null);

  protected selectMake(make: string): void {
    this.make.set(make || null);
    this.generation.set(null);
    this.vehicleId.set(null);
  }

  protected selectGeneration(generation: string): void {
    this.generation.set(generation || null);
    this.vehicleId.set(null);
  }

  protected confirm(): void {
    const vehicle = this.selectedVehicle();
    if (vehicle) {
      this.vehicleContext.select(vehicle);
      void this.router.navigate(['/alkatreszek']);
    }
  }
}
