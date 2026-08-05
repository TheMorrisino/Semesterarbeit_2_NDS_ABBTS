<template>
  <RouterView v-if="!authStore.isLoggedIn" name="loginView" />
  <v-app v-else id="inspire">
<v-navigation-drawer v-model="drawer">
<div class="d-flex align-center ga-5 px-4 py-3">
  <div style="width: 30px; height: 30px; overflow: hidden;">
    <v-img :src="ressourcixLogo" width="30"height="30" style="object-fit: contain; object-position: left center;"/>
  </div>
  <span class="text-h5 font-weight-bold">Ressourcix</span>
</div>
  <v-divider />

  <v-list-subheader class="pl-4">{{ t('app.subtitle') }}</v-list-subheader>
  <v-list-item link :title="t('app.nav.dashboard')" to="/" class="pl-4"></v-list-item>
  <v-list-item link :title="t('app.nav.calender')" to="/calender" class="pl-4"></v-list-item>
  <v-list-item link :title="t('app.nav.absences')" to="/absences" class="pl-4"></v-list-item>
  <v-list-item link :title="t('app.nav.approval')" to="/approval" class="pl-4"></v-list-item>
  <v-divider />
  <v-list-subheader class="pl-4">{{ t('app.subtitle2') }}</v-list-subheader>
  <v-list-item link :title="t('app.nav.teamview')" to="/teamview" class="pl-4"></v-list-item>
  <v-list-item link :title="t('app.nav.employees')" to="/employees" class="pl-4"></v-list-item>
  <v-list-item link :title="t('app.nav.auditlog')" to="/auditlog" class="pl-4"></v-list-item>
  <v-divider />
  <v-list-subheader class="pl-4">{{ t('app.subtitle3') }}</v-list-subheader>
  <v-list-item link :title="t('app.nav.messages')" to="/messages" class="pl-4"></v-list-item>
  <v-list-item link :title="t('app.nav.logout')" to="/logout" class="pl-4"></v-list-item>

  <v-divider />
  <!-- <v-list-item :title="t('app.name')" class="pl-4"></v-list-item> -->

  <!-- mit Append wird das Konto immer im unteren Bereich angezeigt -->
  <template #append>
    <v-divider />
    <AppUserInfo />
  </template>

</v-navigation-drawer>

    <v-app-bar :elevation="2">
      <v-app-bar-nav-icon @click="drawer = !drawer"></v-app-bar-nav-icon>
      <v-app-bar-title class="font-weight-medium">{{ pageTitle }} </v-app-bar-title>

  <v-spacer />
  <AppThemeSwitch class="mr-3" />
    </v-app-bar>
    <v-main>
      <v-container>
        <RouterView name="mainView" />
      </v-container>
    </v-main>
  </v-app>
</template>

<script lang="ts" setup>
import { ref, computed } from "vue";
import { useI18n } from "vue-i18n";
import { useRoute } from "vue-router";
import AppThemeSwitch from "./components/AppThemeSwitch.vue";
import AppUserInfo from "./components/AppUserInfo.vue";
import ressourcixLogo from '@/assets/Ressourcix_Icon_OhneB2.png'
import { useAuthStore } from "./stores/auth";
const authStore = useAuthStore();
const drawer = ref<boolean | null>(null);
const { t } = useI18n();
const route = useRoute();

// aktualisiert sich automatisch bei jedem Routenwechsel, da route.meta reaktiv ist
const pageTitle = computed(() => (route.meta.titleKey ? t(route.meta.titleKey) : ""));
</script>
