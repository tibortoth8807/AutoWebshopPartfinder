import { Injectable, computed, effect, signal } from '@angular/core';
import { readJson, writeJson } from './local-storage';
import { CartItem, Part } from './models';

/** Client-side cart (demo). Checkout / orders API is on the roadmap. */
@Injectable({ providedIn: 'root' })
export class CartService {
  private static readonly storageKey = 'pf.cart';

  readonly items = signal<CartItem[]>(readJson<CartItem[]>(CartService.storageKey, []));
  readonly count = computed(() => this.items().reduce((sum, item) => sum + item.quantity, 0));
  readonly total = computed(() => this.items().reduce((sum, item) => sum + item.quantity * item.part.price, 0));

  constructor() {
    effect(() => writeJson(CartService.storageKey, this.items()));
  }

  add(part: Part, quantity = 1): void {
    this.items.update((items) => {
      const existing = items.find((i) => i.part.id === part.id);
      return existing
        ? items.map((i) => (i.part.id === part.id ? { ...i, quantity: i.quantity + quantity } : i))
        : [...items, { part, quantity }];
    });
  }

  setQuantity(partId: number, quantity: number): void {
    this.items.update((items) =>
      quantity <= 0
        ? items.filter((i) => i.part.id !== partId)
        : items.map((i) => (i.part.id === partId ? { ...i, quantity } : i)),
    );
  }

  clear(): void {
    this.items.set([]);
  }
}
