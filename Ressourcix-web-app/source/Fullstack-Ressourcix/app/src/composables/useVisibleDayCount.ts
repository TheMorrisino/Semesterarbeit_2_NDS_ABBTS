import type { Ref } from 'vue'
import { onMounted, onUnmounted, ref, watch } from 'vue'

// Grid-Virtualisierung für CalenderView: es wird nie mehr generiert, als in den Container passt,
// damit overflow-x deaktiviert bleiben kann und Navigation ausschliesslich über die Toolbar-Pfeile läuft.
export function useVisibleDayCount (nameColumnWidthPx: Ref<number>, dayColumnWidthPx: number, fallback: number) {
  const scrollHost = ref<HTMLDivElement | null>(null)
  const visibleDayCount = ref(fallback)
  let resizeObserver: ResizeObserver | null = null

  function updateVisibleDayCount () {
    const el = scrollHost.value
    if (!el) {
      return
    }
    const usableWidth = el.clientWidth - nameColumnWidthPx.value
    visibleDayCount.value = Math.max(1, Math.floor(usableWidth / dayColumnWidthPx))
  }

  // Neu berechnen, sobald sich die Namensspaltenbreite ändert (xs-Breakpoint überschritten),
  // unabhängig davon, ob der Container selbst im gleichen Moment eine Grössenänderung meldet.
  watch(nameColumnWidthPx, updateVisibleDayCount)

  onMounted(() => {
    updateVisibleDayCount()
    if (scrollHost.value) {
      resizeObserver = new ResizeObserver(() => updateVisibleDayCount())
      resizeObserver.observe(scrollHost.value)
    }
  })

  onUnmounted(() => {
    resizeObserver?.disconnect()
  })

  return { scrollHost, visibleDayCount, updateVisibleDayCount }
}
