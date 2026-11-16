/**
 * Own, simplified exploded-view diagrams (no licensed catalog artwork).
 * Each hotspot key matches Part.diagramKey in the API, so clicking a part on the drawing filters the list.
 */
export interface DiagramShape {
  kind: 'circle' | 'rect' | 'path';
  cx?: number;
  cy?: number;
  r?: number;
  x?: number;
  y?: number;
  width?: number;
  height?: number;
  rx?: number;
  d?: string;
  /** Outline-only detail (hub, bolt hole, tube) instead of a filled body. */
  detail?: boolean;
}

export interface Hotspot {
  key: string;
  label: string;
  /** Position of the numbered callout bubble and the point its leader line points to. */
  callout: { x: number; y: number };
  anchor: { x: number; y: number };
  shapes: DiagramShape[];
}

export interface Diagram {
  id: string;
  title: string;
  viewBox: string;
  hotspots: Hotspot[];
}

export const DIAGRAMS: Record<string, Diagram> = {
  brakes: {
    id: 'brakes',
    title: 'Fékrendszer – robbantott ábra',
    viewBox: '0 0 640 380',
    hotspots: [
      {
        key: 'brake-disc-front',
        label: 'Féktárcsa (első)',
        callout: { x: 40, y: 40 },
        anchor: { x: 125, y: 105 },
        shapes: [
          { kind: 'circle', cx: 210, cy: 190, r: 120 },
          { kind: 'circle', cx: 210, cy: 190, r: 45, detail: true },
          { kind: 'circle', cx: 238, cy: 190, r: 6, detail: true },
          { kind: 'circle', cx: 219, cy: 217, r: 6, detail: true },
          { kind: 'circle', cx: 187, cy: 206, r: 6, detail: true },
          { kind: 'circle', cx: 187, cy: 174, r: 6, detail: true },
          { kind: 'circle', cx: 219, cy: 163, r: 6, detail: true },
        ],
      },
      {
        key: 'brake-caliper-front',
        label: 'Féknyereg (első)',
        callout: { x: 300, y: 330 },
        anchor: { x: 330, y: 270 },
        shapes: [
          {
            kind: 'path',
            d: 'M300 110 h70 a20 20 0 0 1 20 20 v120 a20 20 0 0 1 -20 20 h-70 a20 20 0 0 1 -20 -20 v-120 a20 20 0 0 1 20 -20 z',
          },
          { kind: 'circle', cx: 335, cy: 150, r: 9, detail: true },
          { kind: 'circle', cx: 335, cy: 230, r: 9, detail: true },
        ],
      },
      {
        key: 'brake-pad-front',
        label: 'Fékbetét (első)',
        callout: { x: 440, y: 105 },
        anchor: { x: 436, y: 130 },
        shapes: [
          { kind: 'rect', x: 410, y: 130, width: 22, height: 110, rx: 6 },
          { kind: 'rect', x: 440, y: 130, width: 22, height: 110, rx: 6 },
        ],
      },
      {
        key: 'brake-hose',
        label: 'Fékcső',
        callout: { x: 610, y: 80 },
        anchor: { x: 590, y: 42 },
        shapes: [{ kind: 'path', d: 'M370 110 C 380 60, 440 40, 600 40', detail: true }],
      },
      {
        key: 'brake-disc-rear',
        label: 'Féktárcsa (hátsó)',
        callout: { x: 430, y: 355 },
        anchor: { x: 478, y: 330 },
        shapes: [
          { kind: 'circle', cx: 520, cy: 300, r: 60 },
          { kind: 'circle', cx: 520, cy: 300, r: 22, detail: true },
        ],
      },
      {
        key: 'brake-pad-rear',
        label: 'Fékbetét (hátsó)',
        callout: { x: 615, y: 240 },
        anchor: { x: 609, y: 265 },
        shapes: [
          { kind: 'rect', x: 590, y: 265, width: 14, height: 70, rx: 4 },
          { kind: 'rect', x: 610, y: 265, width: 14, height: 70, rx: 4 },
        ],
      },
    ],
  },

  suspension: {
    id: 'suspension',
    title: 'Futómű (McPherson) – robbantott ábra',
    viewBox: '0 0 640 380',
    hotspots: [
      {
        key: 'strut-mount',
        label: 'Toronycsapágy',
        callout: { x: 470, y: 38 },
        anchor: { x: 390, y: 38 },
        shapes: [
          { kind: 'rect', x: 250, y: 20, width: 140, height: 36, rx: 18 },
          { kind: 'circle', cx: 320, cy: 38, r: 10, detail: true },
        ],
      },
      {
        key: 'shock-absorber',
        label: 'Lengéscsillapító',
        callout: { x: 470, y: 250 },
        anchor: { x: 342, y: 250 },
        shapes: [
          { kind: 'rect', x: 313, y: 64, width: 14, height: 120, rx: 4 },
          { kind: 'rect', x: 298, y: 175, width: 44, height: 130, rx: 8 },
        ],
      },
      {
        key: 'coil-spring',
        label: 'Tekercsrugó',
        callout: { x: 170, y: 110 },
        anchor: { x: 260, y: 115 },
        shapes: [
          {
            kind: 'path',
            d: 'M260 85 L380 100 L260 115 L380 130 L260 145 L380 160 L260 175 L380 190',
            detail: true,
          },
        ],
      },
      {
        key: 'control-arm',
        label: 'Lengőkar',
        callout: { x: 60, y: 250 },
        anchor: { x: 100, y: 300 },
        shapes: [
          { kind: 'path', d: 'M100 300 L300 335 L100 360 Z' },
          { kind: 'circle', cx: 100, cy: 300, r: 12, detail: true },
          { kind: 'circle', cx: 100, cy: 360, r: 12, detail: true },
          { kind: 'circle', cx: 300, cy: 335, r: 10, detail: true },
        ],
      },
      {
        key: 'stabilizer-link',
        label: 'Stabilizátor pálca',
        callout: { x: 560, y: 330 },
        anchor: { x: 470, y: 330 },
        shapes: [
          { kind: 'path', d: 'M420 200 L470 330', detail: true },
          { kind: 'circle', cx: 420, cy: 200, r: 9 },
          { kind: 'circle', cx: 470, cy: 330, r: 9 },
        ],
      },
    ],
  },
};
