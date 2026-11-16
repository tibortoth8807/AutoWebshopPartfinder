import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { DIAGRAMS } from './diagrams';

/**
 * Clickable exploded-view drawing. Clicking a part (or its number in the legend)
 * emits its key; clicking the selected one again clears the filter.
 */
@Component({
  selector: 'pf-exploded-diagram',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (diagram(); as d) {
      <figure class="diagram">
        <figcaption>{{ d.title }} <small>– kattints egy alkatrészre</small></figcaption>

        <svg [attr.viewBox]="d.viewBox" role="img" [attr.aria-label]="d.title">
          @for (h of d.hotspots; track h.key; let i = $index) {
            <g
              class="hotspot"
              [class.selected]="h.key === selectedKey()"
              [class.dimmed]="selectedKey() && h.key !== selectedKey()"
              (click)="toggle(h.key)"
              tabindex="0"
              (keydown.enter)="toggle(h.key)"
              [attr.aria-label]="h.label"
            >
              <title>{{ h.label }}</title>
              @for (s of h.shapes; track $index) {
                @switch (s.kind) {
                  @case ('circle') {
                    <circle [class.detail]="s.detail" [attr.cx]="s.cx" [attr.cy]="s.cy" [attr.r]="s.r" />
                  }
                  @case ('rect') {
                    <rect [class.detail]="s.detail" [attr.x]="s.x" [attr.y]="s.y" [attr.width]="s.width" [attr.height]="s.height" [attr.rx]="s.rx" />
                  }
                  @case ('path') {
                    <path [class.detail]="s.detail" [attr.d]="s.d" />
                  }
                }
              }
              <line class="leader" [attr.x1]="h.callout.x" [attr.y1]="h.callout.y" [attr.x2]="h.anchor.x" [attr.y2]="h.anchor.y" />
              <circle class="callout" [attr.cx]="h.callout.x" [attr.cy]="h.callout.y" r="13" />
              <text class="callout-text" [attr.x]="h.callout.x" [attr.y]="h.callout.y + 5">{{ i + 1 }}</text>
            </g>
          }
        </svg>

        <ol class="legend">
          @for (h of d.hotspots; track h.key) {
            <li>
              <button type="button" class="link" [class.active]="h.key === selectedKey()" (click)="toggle(h.key)">
                {{ h.label }}
              </button>
            </li>
          }
        </ol>
      </figure>
    }
  `,
  styles: `
    .diagram { margin: 0; background: #fff; border: 1px solid var(--border); border-radius: 12px; padding: 12px 16px; }
    figcaption { font-weight: 600; margin-bottom: 6px; }
    figcaption small { font-weight: 400; color: var(--muted); }
    svg { width: 100%; max-height: 340px; }
    .hotspot { cursor: pointer; outline: none; }
    .hotspot circle, .hotspot rect, .hotspot path { fill: #cbd5e1; stroke: #334155; stroke-width: 2; transition: fill .15s, opacity .15s; }
    .hotspot .detail { fill: none; stroke-width: 4; }
    .hotspot:hover circle:not(.callout), .hotspot:hover rect, .hotspot:hover path:not(.detail),
    .hotspot:focus-visible rect, .hotspot:focus-visible path:not(.detail) { fill: #99f6e4; }
    .hotspot.selected circle:not(.callout):not(.detail), .hotspot.selected rect, .hotspot.selected path:not(.detail) { fill: var(--brand-light); stroke: var(--brand); }
    .hotspot.selected .detail { stroke: var(--brand); }
    .hotspot.dimmed { opacity: .45; }
    .leader { stroke: #64748b; stroke-width: 1.5; stroke-dasharray: 4 3; }
    .hotspot .callout { fill: var(--accent); stroke: #fff; stroke-width: 2; }
    .callout-text { fill: #fff; font-size: 14px; font-weight: 700; text-anchor: middle; pointer-events: none; }
    .legend { display: flex; flex-wrap: wrap; gap: 4px 20px; margin: 8px 0 0; padding-left: 20px; }
    .legend .active { font-weight: 700; color: var(--brand); }
  `,
})
export class ExplodedDiagramComponent {
  readonly diagramId = input.required<string>();
  readonly selectedKey = input<string | null>(null);
  readonly keySelected = output<string | null>();

  protected readonly diagram = computed(() => DIAGRAMS[this.diagramId()] ?? null);

  protected toggle(key: string): void {
    this.keySelected.emit(key === this.selectedKey() ? null : key);
  }
}
