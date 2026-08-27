<template>
  <RouterView v-if="!authStore.isLoggedIn" name="loginView" />

  <v-app v-else id="inspire">
    <v-navigation-drawer v-model="drawer">
      <div class="d-flex align-center ga-5 px-4 py-3">
        <div style="width: 30px; height: 30px; overflow: hidden;">
          <v-img height="30" :src="ressourcixLogo" style="object-fit: contain; object-position: left center;" width="30" />
        </div>

        <span class="text-h5 font-weight-bold">Ressourcix</span>
      </div>

      <v-divider />

      <v-list-subheader class="pl-4">{{ t('app.subtitle') }}</v-list-subheader>
      <v-list-item class="pl-4" link :title="t('app.nav.dashboard')" to="/" />
      <v-list-item class="pl-4" link :title="t('app.nav.absences')" to="/absences" />
      <v-list-item class="pl-4" link :title="t('app.nav.teamview')" to="/teamview" />
      <v-divider />
      <v-list-subheader class="pl-4">{{ t('app.subtitle2') }}</v-list-subheader>
      <v-list-item class="pl-4" link :title="t('app.nav.calender')" to="/calender" />

      <v-list-item
        v-if="authStore.isAdmin"
        class="pl-4"
        link
        :title="t('app.nav.approval')"
        to="/approval"
      />

      <v-list-item class="pl-4" link :title="t('app.nav.employees')" to="/employees" />
      <v-list-item class="pl-4" link :title="t('app.nav.auditlog')" to="/auditlog" />
      <v-divider />

      <!-- mit Append wird das Konto immer im unteren Bereich angezeigt -->
      <template #append>
        <v-divider />
        <AppUserInfo />
      </template>

    </v-navigation-drawer>

    <v-app-bar :elevation="2">
      <v-app-bar-nav-icon @click="drawer = !drawer" />
      <v-app-bar-title class="font-weight-medium">{{ pageTitle }} </v-app-bar-title>

      <v-spacer />
      <AppThemeSwitch class="mr-3" />
    </v-app-bar>

    <v-main>
      <v-container fluid>
        <RouterView name="mainView" />
      </v-container>
    </v-main>
  </v-app>
</template>

<script lang="ts" setup>
  import { computed, ref } from 'vue'
  import { useI18n } from 'vue-i18n'
  import { useRoute } from 'vue-router'
  import ressourcixLogo from '@/assets/Ressourcix_Icon_OhneB2.png'
  import AppThemeSwitch from './components/AppThemeSwitch.vue'
  import AppUserInfo from './components/AppUserInfo.vue'
  import { useAuthStore } from './stores/auth'
  const authStore = useAuthStore()
  const drawer = ref<boolean | null>(null)
  const { t } = useI18n()
  const route = useRoute()

  const pageTitle = computed(() => (route.meta.titleKey ? t(route.meta.titleKey) : ''))
</script>
