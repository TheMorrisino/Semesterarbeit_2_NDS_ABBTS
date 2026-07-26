<template>
  <v-list-item
    lines="two"
    prepend-avatar="https://randomuser.me/api/portraits/lego/1.jpg"
    :subtitle="t('user.guestMode')"
    :title="t('user.notLoggedIn')"
  />
</template>

<script setup lang="ts">
import { useI18n } from "vue-i18n";

const { t } = useI18n();
</script>
