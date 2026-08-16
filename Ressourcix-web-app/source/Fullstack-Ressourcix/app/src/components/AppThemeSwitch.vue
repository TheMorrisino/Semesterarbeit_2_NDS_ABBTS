<template>
  <v-switch
    v-model="isDarkTheme"
    density="comfortable"
    false-icon="mdi-white-balance-sunny"
    hide-details
    true-icon="mdi-moon-waning-crescent"
  />
</template>

<script setup lang="ts">
  import { computed } from 'vue'
  import { useTheme } from 'vuetify'

  const theme = useTheme()

  const isDarkTheme = computed({
    get: () => theme.current.value.dark,
    set: v => (theme.global.name.value = v ? 'dark' : 'light'),
  })
</script>
