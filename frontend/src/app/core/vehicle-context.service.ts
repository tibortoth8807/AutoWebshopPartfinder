import { Injectable, effect, signal } from '@angular/core';
import { readJson, writeJson } from './local-storage';
import { Vehicle } from './models';

/** "My car": the vehicle every search is filtered for. Survives page reloads. */
@Injectable({ providedIn: 'root' })
export class VehicleContextService {
  private static readonly storageKey = 'pf.vehicle';

  readonly selected = signal<Vehicle | null>(readJson<Vehicle | null>(VehicleContextService.storageKey, null));

  constructor() {
    effect(() => writeJson(VehicleContextService.storageKey, this.selected()));
  }

  select(vehicle: Vehicle | null): void {
    this.selected.set(vehicle);
  }
}
