<!-- Balkendiagramm: Y-Achse = Anzahl gleichzeitig abwesender Mitarbeitender, X-Achse = Tage.
     Nutzt dieselbe grün-gelb-rot-Heatmap-Farbe wie der Kalender (src/utils/overlapHeatmap.ts). -->
<template>
  <div class="overlap-chart">
    <div class="chart-y-axis">
      <span>{{ maxValue }}</span>
      <span>0</span>
    </div>

    <div class="chart-main">
      <div class="chart-plot">
        <div class="chart-gridline chart-gridline--top" />
        <div class="chart-gridline chart-gridline--mid" />
        <div class="chart-gridline chart-gridline--base" />

        <div class="chart-bars">
          <div v-for="point in points" :key="point.iso" class="chart-col">
            <div
              class="chart-bar"
              :class="{ 'chart-bar--empty': point.value === 0 }"
              :style="{ height: barHeightPercent(point.value) + '%', backgroundColor: overlapColor(point.value) }"
              :title="t('overlapChart.tooltip', { label: point.label, value: point.value })"
            >
              <span v-if="point.value > 0" class="chart-value">{{ point.value }}</span>
            </div>
          </div>
        </div>
      </div>

      <div class="chart-labels">
        <span v-for="point in points" :key="point.iso" class="chart-label" :class="{ 'chart-label--weekend': point.isWeekend }">
          {{ point.label }}
        </span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
  import { computed } from 'vue'
  import { useI18n } from 'vue-i18n'
  import { overlapColor } from '@/utils/overlapHeatmap'

  export interface OverlapChartPoint {
    iso: string
    label: string
    value: number
    isWeekend?: boolean
  }

  const { t } = useI18n()
  const props = defineProps<{ points: OverlapChartPoint[] }>()

  // mind. 1, damit ein leeres Diagramm nicht durch 0 teilt
  const maxValue = computed(() => Math.max(1, ...props.points.map(point => point.value)))

  function barHeightPercent (value: number): number {
    return (value / maxValue.value) * 100
  }
</script>

<style scoped>
.overlap-chart {
  display: flex;
  gap: 10px;
  height: 180px;
}

.chart-y-axis {
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  font-family: "Roboto Mono", "SFMono-Regular", Consolas, monospace;
  font-size: 11px;
  color: rgba(var(--v-theme-on-surface), 0.55);
  padding-bottom: 22px;
}

.chart-main {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
}

.chart-plot {
  position: relative;
  flex: 1;
}

.chart-gridline {
  position: absolute;
  left: 0;
  right: 0;
  border-top: 1px dashed rgba(var(--v-theme-on-surface), 0.12);
}
.chart-gridline--top { top: 0; }
.chart-gridline--mid { top: 50%; }
.chart-gridline--base { bottom: 0; }

.chart-bars {
  position: absolute;
  inset: 0;
  display: flex;
  align-items: flex-end;
  gap: 4px;
}

.chart-col {
  flex: 1;
  min-width: 0;
  display: flex;
  justify-content: center;
  height: 100%;
  align-items: flex-end;
}

.chart-bar {
  width: 70%;
  min-height: 2px;
  border-radius: 4px 4px 0 0;
  display: flex;
  justify-content: center;
  transition: height 0.2s ease;
}

.chart-bar--empty {
  background: rgba(var(--v-theme-on-surface), 0.08) !important;
}

.chart-value {
  font-family: "Roboto Mono", "SFMono-Regular", Consolas, monospace;
  font-size: 10.5px;
  font-weight: 700;
  color: rgba(var(--v-theme-on-surface), 0.7);
  transform: translateY(-16px);
}

.chart-labels {
  display: flex;
  gap: 4px;
  margin-top: 6px;
  height: 16px;
}

.chart-label {
  flex: 1;
  min-width: 0;
  text-align: center;
  font-size: 10px;
  color: rgba(var(--v-theme-on-surface), 0.6);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.chart-label--weekend {
  font-weight: 700;
}
</style>
