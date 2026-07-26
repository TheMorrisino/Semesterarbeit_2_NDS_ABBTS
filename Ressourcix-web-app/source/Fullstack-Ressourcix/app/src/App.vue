<template>
  <v-app id="inspire">
    <v-navigation-drawer v-model="drawer">
      <v-list-item
        :title="t('app.name')"
        :subtitle="t('app.subtitle')"
      ></v-list-item>
      <v-divider />
      <AppUserInfo />
      <v-divider />
      <v-container>
        <AppThemeSwitch />
      </v-container>
      <v-divider />
      <v-list-item link :title="t('app.nav.home')" to="/"></v-list-item>
      <v-list-item
        link
        :title="t('app.nav.gallery')"
        to="/gallery"
      ></v-list-item>
      <v-list-item link :title="t('app.nav.about')" to="/about"></v-list-item>
    </v-navigation-drawer>
    <v-app-bar>
      <v-app-bar-nav-icon @click="drawer = !drawer"></v-app-bar-nav-icon>
      <v-app-bar-title>{{ t("app.title") }}</v-app-bar-title>
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

const drawer = ref<boolean | null>(null);
const { t } = useI18n();
</script>
