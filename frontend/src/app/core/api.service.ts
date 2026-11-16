import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Category, PagedResult, Part, PartQuery, Vehicle, VinLookupResponse } from './models';

/** Thin typed wrapper around the PartFinder API. Relative URLs: dev proxy locally, nginx in Docker. */
@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);

  lookupVin(vin: string) {
    return this.http.get<VinLookupResponse>(`/api/vin/${encodeURIComponent(vin)}`);
  }

  vehicles() {
    return this.http.get<Vehicle[]>('/api/vehicles');
  }

  categories() {
    return this.http.get<Category[]>('/api/categories');
  }

  parts(query: PartQuery) {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(query)) {
      if (value !== null && value !== undefined && value !== '') {
        params = params.set(key, String(value));
      }
    }
    return this.http.get<PagedResult<Part>>('/api/parts', { params });
  }
}
