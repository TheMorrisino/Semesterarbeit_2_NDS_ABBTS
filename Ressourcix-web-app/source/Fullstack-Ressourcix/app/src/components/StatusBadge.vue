<script setup lang="ts">
import { RequestStatus } from '@/stores/request'
defineProps<{ status: RequestStatus }>()
const color: Record<RequestStatus, string> = {
  [RequestStatus.Open]: '#EF9F27',
  [RequestStatus.Approved]: '#639922',
  [RequestStatus.Rejected]: '#E24B4A',
  [RequestStatus.Taken]: '#378ADD',
  [RequestStatus.Cancelled]: '#888780',
}
</script>
<template>
  <span class="badge" :style="{ color: color[status] }">{{ status }}</span>
</template>
<style scoped>
.badge { font-size: 12px; font-weight: 600; }
.badge::before { content: '●'; margin-right: 5px; }
</style>
