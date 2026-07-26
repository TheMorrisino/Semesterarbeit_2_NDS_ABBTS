<template>
  <v-switch
    v-model="isDarkTheme"
    true-icon="mdi-moon-waning-crescent"
    false-icon="mdi-white-balance-sunny"
    density="comfortable"
    hide-details
  />
</template>

<script setup lang="ts">
import { computed } from "vue";
import { useTheme } from "vuetify";

const theme = useTheme();

const isDarkTheme = computed({
  get: () => theme.current.value.dark,
  set: (v) => (theme.global.name.value = v ? "dark" : "light"),
});
</script>
