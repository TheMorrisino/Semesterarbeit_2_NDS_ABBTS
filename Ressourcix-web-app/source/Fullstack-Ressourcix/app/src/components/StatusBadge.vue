<script setup lang="ts">
import type { AntragStatus } from '@/types'
defineProps<{ status: AntragStatus }>()
const farbe: Record<AntragStatus, string> = {
  Ausstehend: '#EF9F27', Genehmigt: '#639922', Abgelehnt: '#E24B4A',
  Bezogen: '#378ADD', Storniert: '#888780'
}
</script>
<template>
  <span class="badge" :style="{ color: farbe[status] }">{{ status }}</span>
</template>
<style scoped>
.badge { font-size: 12px; font-weight: 600; }
.badge::before { content: '●'; margin-right: 5px; }
</style>
