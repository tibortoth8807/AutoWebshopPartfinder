export type FuelType = 'Petrol' | 'Diesel';

export interface Vehicle {
  id: number;
  make: string;
  model: string;
  generation: string;
  engine: string;
  fuel: FuelType;
  powerKw: number;
  yearFrom: number;
  yearTo: number;
  displayName: string;
}

export interface DecodedVin {
  vin: string;
  wmi: string;
  vds: string;
  vis: string;
  manufacturer: string | null;
  country: string | null;
  checkDigit: 'Valid' | 'Invalid' | 'NotUsed';
  modelYearCode: string;
  possibleModelYears: number[];
  plantCode: string | null;
  serialNumber: string;
}

export interface NhtsaVinInfo {
  make: string | null;
  model: string | null;
  modelYear: string | null;
  bodyClass: string | null;
  fuelType: string | null;
  errorText: string | null;
}

export interface VinLookupResponse {
  vin: DecodedVin;
  matches: Vehicle[];
  nhtsa: NhtsaVinInfo | null;
}

export interface Category {
  id: number;
  name: string;
  slug: string;
  diagramId: string | null;
  children: Category[];
}

export interface Part {
  id: number;
  sku: string;
  name: string;
  brand: string;
  description: string | null;
  categorySlug: string;
  categoryName: string;
  price: number;
  stock: number;
  position: string | null;
  diagramKey: string | null;
}

export interface PagedResult<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
}

export interface PartQuery {
  vehicleId: number | null;
  category: string | null;
  diagramKey: string | null;
  q: string;
  sort: 'name' | 'price' | '-price';
  page: number;
  pageSize: number;
}

export interface CartItem {
  part: Part;
  quantity: number;
}
