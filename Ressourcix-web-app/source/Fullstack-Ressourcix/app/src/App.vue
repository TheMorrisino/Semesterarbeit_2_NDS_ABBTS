<template>
  <v-app id="inspire">
<v-navigation-drawer v-model="drawer">
  <v-list-item :title="t('app.name')" class="pl-4"></v-list-item>
  <AppUserInfo />
  <v-divider />

  <v-list-subheader class="pl-4">{{ t('app.subtitle') }}</v-list-subheader>
  <v-list-item link :title="t('app.nav.home')" to="/" class="pl-4"></v-list-item>
  <v-list-item link :title="t('app.nav.gallery')" to="/gallery" class="pl-4"></v-list-item>
  <v-list-item link :title="t('app.nav.abwesenheiten')" to="/abwesenheiten" class="pl-4"></v-list-item>
  <v-list-item link :title="t('app.nav.Kalender')" to="/kalender" class="pl-4"></v-list-item>

  
  <v-divider />
  <v-list-subheader class="pl-4">{{ t('app.subtitle2') }}</v-list-subheader>

  <v-list-item link :title="t('app.nav.employees')" to="/mitarbeitende" class="pl-4"></v-list-item>
  <v-list-item link :title="t('app.nav.teamübersicht')" to="/teamübersicht" class="pl-4"></v-list-item>
    <v-list-item link :title="t('app.nav.auditlog')" to="/auditlog" class="pl-4"></v-list-item>

  <v-divider />
</v-navigation-drawer>
    <v-app-bar :elevation="2">
      <v-app-bar-nav-icon @click="drawer = !drawer"></v-app-bar-nav-icon>
      <v-app-bar-title>{{ t("app.title") }}</v-app-bar-title>
      <AppThemeSwitch    />
      <v-img :src="ressourcixLogo" max-width="32" max-height="32" class="mr-2" />

    </v-app-bar>
    <v-main>
      <v-container>
        <RouterView />
      </v-container>
    </v-main>
  </v-app>
</template>

<script lang="ts" setup>
import { ref } from "vue";
import { useI18n } from "vue-i18n";
import AppThemeSwitch from "./components/AppThemeSwitch.vue";
import AppUserInfo from "./components/AppUserInfo.vue";
import ressourcixLogo from '@/assets/Ressourcix_Icon_OhneB2.png'

const drawer = ref<boolean | null>(null);
const { t } = useI18n();
</script>
