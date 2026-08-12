<template>
  <v-list-item
    v-if="authStore.isLoggedIn"
    lines="two"
    prepend-avatar="https://randomuser.me/api/portraits/lego/2.jpg"
    :subtitle="t('user.levelLabel', { level: authStore.user?.permissionLevel })"
    :title="authStore.user?.username"
  >
    <template #append>
      <v-btn icon="mdi-logout" size="small" variant="text" @click="onLogout" />
    </template>
  </v-list-item>

  <v-list-item
    v-else
    lines="two"
    prepend-avatar="https://randomuser.me/api/portraits/lego/2.jpg"
    :subtitle="t('user.guestMode')"
    :title="t('user.notLoggedIn')"
    to="/login"
  />
</template>

<script setup lang="ts">
import { useI18n } from "vue-i18n";
import { useRouter } from "vue-router";
import { useAuthStore } from "@/stores/auth";

const { t } = useI18n();
const router = useRouter();
const authStore = useAuthStore();

async function onLogout() {
  await authStore.logout();
  router.push("/login");
}
</script>