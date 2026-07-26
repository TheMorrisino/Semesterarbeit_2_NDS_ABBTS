<script setup lang="ts">
import { useAuthStore } from '@/stores/auth'
const auth = useAuthStore()
</script>
<template>
  <header class="tb">
    <strong>{{ $route.name }}</strong>
    <span class="user">{{ auth.name }} · {{ auth.rolle }}</span>
  </header>
</template>
<style scoped>
.tb { height: 62px; background: #fff; border-bottom: 1px solid var(--line);
  display: flex; align-items: center; justify-content: space-between; padding: 0 26px; }
.user { color: var(--ink-2); font-size: 13px; }
</style>
