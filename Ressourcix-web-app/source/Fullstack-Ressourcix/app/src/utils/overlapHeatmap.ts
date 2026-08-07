// Gemeinsame Überschneidungs-Heatmap-Logik (grün -> gelb -> rot, fliessender Verlauf),
// verwendet von CalenderView.vue und dem Dashboard-Überschneidungsdiagramm.

// Schwellwerte – später über eine Einstellungs-UI konfigurierbar
export const OVERLAP_FREE_THRESHOLD = 1;
export const OVERLAP_CRITICAL_THRESHOLD = 5;

const COLOR_GREEN: [number, number, number] = [210, 245, 210];
const COLOR_YELLOW: [number, number, number] = [255, 250, 200];
const COLOR_RED: [number, number, number] = [255, 200, 200];

function rgbString(color: [number, number, number]): string {
  return `rgb(${color[0]}, ${color[1]}, ${color[2]})`;
}

function lerpColor(from: [number, number, number], to: [number, number, number], t: number): string {
  const r = Math.round(from[0] + (to[0] - from[0]) * t);
  const g = Math.round(from[1] + (to[1] - from[1]) * t);
  const b = Math.round(from[2] + (to[2] - from[2]) * t);
  return `rgb(${r}, ${g}, ${b})`;
}

export function overlapColor(count: number): string {
  if (count <= OVERLAP_FREE_THRESHOLD) return rgbString(COLOR_GREEN);
  if (count >= OVERLAP_CRITICAL_THRESHOLD) return rgbString(COLOR_RED);

  const mid = (OVERLAP_FREE_THRESHOLD + OVERLAP_CRITICAL_THRESHOLD) / 2;
  if (count <= mid) {
    const t = (count - OVERLAP_FREE_THRESHOLD) / (mid - OVERLAP_FREE_THRESHOLD);
    return lerpColor(COLOR_GREEN, COLOR_YELLOW, t);
  }
  const t = (count - mid) / (OVERLAP_CRITICAL_THRESHOLD - mid);
  return lerpColor(COLOR_YELLOW, COLOR_RED, t);
}
