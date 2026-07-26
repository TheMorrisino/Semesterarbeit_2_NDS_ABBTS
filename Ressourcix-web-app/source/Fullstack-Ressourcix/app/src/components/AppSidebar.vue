<script setup lang="ts">
import { computed } from 'vue'
import { RouterLink } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const auth = useAuthStore()
const nav = computed(() => [
  { to: '/', label: 'Dashboard', zeige: true },
  { to: '/kalender', label: 'Ferienkalender', zeige: true },
  { to: '/meine-abwesenheiten', label: 'Meine Abwesenheiten', zeige: true },
  { to: '/genehmigungen', label: 'Genehmigungen', zeige: auth.istPlanner },
  { to: '/mitarbeitende', label: 'Mitarbeitende', zeige: auth.istHr },
  { to: '/audit', label: 'Audit-Log', zeige: auth.istIt }
])
</script>

<template>
  <aside class="sb">
    <div class="brand">Ressourcix</div>
    <nav>
      <template v-for="item in nav" :key="item.to">
        <RouterLink v-if="item.zeige" :to="item.to">{{ item.label }}</RouterLink>
      </template>
    </nav>
  </aside>
</template>

<style scoped>
.sb { width: 236px; background: #0b3b30; color: #cfe4dc; padding: 16px; }
.brand { font-weight: 600; color: #fff; margin-bottom: 16px; }
nav a { display: block; padding: 9px 12px; border-radius: 8px; color: #bcd4cb; text-decoration: none; }
nav a.router-link-active { background: var(--teal-500); color: #fff; }
</style>
