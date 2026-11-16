import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { catchError, debounceTime, finalize, of, switchMap } from 'rxjs';
import { ExplodedDiagramComponent } from '../components/exploded-diagram.component';
import { ApiService } from '../core/api.service';
import { CartService } from '../core/cart.service';
import { Category, Part, PartQuery } from '../core/models';
import { VehicleContextService } from '../core/vehicle-context.service';

@Component({
  selector: 'pf-parts-page',
  imports: [ExplodedDiagramComponent, CurrencyPipe, RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (vehicle.selected(); as v) {
      <div class="banner">Csak a(z) <strong>{{ v.displayName }}</strong> típushoz illő alkatrészek.</div>
    } @else {
      <div class="banner warn">
        Nincs kiválasztott autó – minden alkatrészt mutatunk.
        <a routerLink="/">Válassz autót</a> a pontos találatokhoz.
      </div>
    }

    <div class="catalog">
      <aside class="sidebar">
        <h3>Kategóriák</h3>
        <ul class="tree">
          <li><button type="button" class="link" [class.active]="!category()" (click)="selectCategory(null)">Összes</button></li>
          @for (top of categories(); track top.id) {
            <li>
              <button type="button" class="link top" [class.active]="category() === top.slug" (click)="selectCategory(top.slug)">
                {{ top.name }}
              </button>
              <ul>
                @for (child of top.children; track child.id) {
                  <li>
                    <button type="button" class="link" [class.active]="category() === child.slug" (click)="selectCategory(child.slug)">
                      {{ child.name }}
                    </button>
                  </li>
                }
              </ul>
            </li>
          }
        </ul>
      </aside>

      <section class="results">
        <div class="toolbar">
          <input
            type="search"
            placeholder="Keresés név, márka vagy cikkszám szerint…"
            [value]="q()"
            (input)="setSearch($any($event.target).value)"
          />
          <select (change)="setSort($any($event.target).value)">
            <option value="name">Név szerint</option>
            <option value="price">Ár szerint növekvő</option>
            <option value="-price">Ár szerint csökkenő</option>
          </select>
        </div>

        @if (diagramId(); as id) {
          <pf-exploded-diagram [diagramId]="id" [selectedKey]="diagramKey()" (keySelected)="selectDiagramKey($event)" />
        }

        <p class="muted">
          @if (loading()) { Betöltés… } @else { {{ result()?.total ?? 0 }} találat }
          @if (diagramKey()) { · <button type="button" class="link" (click)="selectDiagramKey(null)">ábra szűrő törlése</button> }
        </p>

        <div class="grid">
          @for (part of result()?.items ?? []; track part.id) {
            <article class="part-card" [class.highlight]="part.diagramKey && part.diagramKey === diagramKey()">
              <div class="part-head">
                <span class="brand-name">{{ part.brand }}</span>
                @if (part.position) { <span class="tag">{{ part.position }}</span> }
              </div>
              <h4>{{ part.name }}</h4>
              <small class="muted">{{ part.categoryName }} · {{ part.sku }}</small>
              <div class="part-foot">
                <span class="price">{{ part.price | currency: 'HUF' : 'symbol-narrow' : '1.0-0' }}</span>
                <span [class.in-stock]="part.stock > 0" [class.out-of-stock]="part.stock === 0">
                  {{ part.stock > 0 ? 'Raktáron (' + part.stock + ' db)' : 'Rendelésre' }}
                </span>
              </div>
              <button type="button" class="btn primary" (click)="addToCart(part)">Kosárba</button>
            </article>
          } @empty {
            @if (!loading()) { <p>Nincs találat a megadott szűrőkkel.</p> }
          }
        </div>

        @if (totalPages() > 1) {
          <nav class="pager">
            <button type="button" class="btn" [disabled]="page() === 1" (click)="page.set(page() - 1)">‹ Előző</button>
            <span>{{ page() }} / {{ totalPages() }}</span>
            <button type="button" class="btn" [disabled]="page() === totalPages()" (click)="page.set(page() + 1)">Következő ›</button>
          </nav>
        }
      </section>
    </div>
  `,
})
export class PartsPage {
  private readonly api = inject(ApiService);
  private readonly cart = inject(CartService);
  protected readonly vehicle = inject(VehicleContextService);

  protected readonly categories = toSignal(this.api.categories(), { initialValue: [] as Category[] });

  protected readonly category = signal<string | null>(null);
  protected readonly diagramKey = signal<string | null>(null);
  protected readonly q = signal('');
  protected readonly sort = signal<PartQuery['sort']>('name');
  protected readonly page = signal(1);
  protected readonly loading = signal(false);
  private readonly pageSize = 24;

  /** Top-level category of the current selection - decides which diagram is shown. */
  private readonly topCategory = computed(() => {
    const slug = this.category();
    return this.categories().find((c) => c.slug === slug || c.children.some((child) => child.slug === slug)) ?? null;
  });

  protected readonly diagramId = computed(() => this.topCategory()?.diagramId ?? null);

  private readonly query = computed<PartQuery>(() => ({
    vehicleId: this.vehicle.selected()?.id ?? null,
    category: this.category(),
    diagramKey: this.diagramKey(),
    q: this.q().trim(),
    sort: this.sort(),
    page: this.page(),
    pageSize: this.pageSize,
  }));

  /** Every filter change re-queries the API; switchMap drops outdated responses. */
  protected readonly result = toSignal(
    toObservable(this.query).pipe(
      debounceTime(200),
      switchMap((query) => {
        this.loading.set(true);
        return this.api.parts(query).pipe(
          catchError(() => of(null)),
          finalize(() => this.loading.set(false)),
        );
      }),
    ),
    { initialValue: null },
  );

  protected readonly totalPages = computed(() => {
    const r = this.result();
    return r ? Math.max(1, Math.ceil(r.total / r.pageSize)) : 1;
  });

  protected selectCategory(slug: string | null): void {
    this.category.set(slug);
    this.diagramKey.set(null);
    this.page.set(1);
  }

  protected selectDiagramKey(key: string | null): void {
    this.diagramKey.set(key);
    this.page.set(1);
  }

  protected setSearch(value: string): void {
    this.q.set(value);
    this.page.set(1);
  }

  protected setSort(value: string): void {
    this.sort.set(value as PartQuery['sort']);
    this.page.set(1);
  }

  protected addToCart(part: Part): void {
    this.cart.add(part);
  }
}
