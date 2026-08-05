<template>
  <div class="login-page d-flex align-center justify-center">
    <v-card class="login-card pa-8" rounded="xl" elevation="8">
      <!-- Logo + Titel -->
      <div class="d-flex align-center ga-3 mb-2">
        <v-avatar color="teal-700" size="40" rounded="lg">
          <span class="text-h6 font-weight-bold text-white">R</span>
        </v-avatar>
        <span class="text-h5 font-weight-bold">Ressourcix</span>
      </div>

      <p class="text-body-2 text-medium-emphasis mb-6">
        {{ t('login.subtitle') }}
      </p>

      <!-- Benutzername -->
      <label class="text-subtitle-2 font-weight-medium mb-1 d-block">
        {{ t('login.username') }}
      </label>
      <v-text-field
        v-model="username"
        density="comfortable"
        variant="outlined"
        rounded="lg"
        hide-details
        class="mb-4"
      />

      <!-- Passwort -->
      <label class="text-subtitle-2 font-weight-medium mb-1 d-block">
        {{ t('login.password') }}
      </label>
      <v-text-field
        v-model="password"
        type="password"
        density="comfortable"
        variant="outlined"
        rounded="lg"
        hide-details
        class="mb-6"
      />

      <!-- Rollen-Auswahl -->
      <label class="text-subtitle-2 font-weight-medium mb-2 d-block">
        {{ t('login.loginAs') }}
      </label>

      <v-row dense class="mb-6">
        <v-col v-for="role in roles" :key="role.value" cols="6">
          <v-card
            :class="['role-card pa-3', { 'role-card--selected': selectedRole === role.value }]"
            variant="outlined"
            rounded="lg"
            @click="selectedRole = role.value"
          >
            <v-icon :icon="role.icon" size="20" class="mb-2" />
            <div class="text-body-2 font-weight-medium">{{ t(role.titleKey) }}</div>
            <div class="text-caption text-medium-emphasis">{{ t(role.subtitleKey) }}</div>
          </v-card>
        </v-col>
      </v-row>

      <!-- Login-Button -->
        <v-btn
        color="teal-700"
        block
        size="large"
        rounded="lg"
        :disabled="!canSubmit"
        :loading="loading"
        @click="onLogin"
        >
        {{ t('login.submit') }}
        </v-btn>

      <!-- Footer -->
      <div class="d-flex align-center justify-center ga-1 mt-4 text-caption text-medium-emphasis">
        <v-icon icon="mdi-lock-outline" size="14" />
        {{ t('login.secureConnection') }}
      </div>
    </v-card>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from "vue";
import { useI18n } from "vue-i18n";
import { useRouter } from "vue-router";
import { useAuthStore, type DemoRole } from "@/stores/auth";

const { t } = useI18n();
const router = useRouter();
const authStore = useAuthStore();

const roles: { value: DemoRole; icon: string; titleKey: string; subtitleKey: string }[] = [
  { value: "employee", icon: "mdi-account-outline", titleKey: "login.roles.employee.title", subtitleKey: "login.roles.employee.subtitle" },
  { value: "planner", icon: "mdi-clipboard-check-outline", titleKey: "login.roles.planner.title", subtitleKey: "login.roles.planner.subtitle" },
  { value: "hr", icon: "mdi-account-group-outline", titleKey: "login.roles.hr.title", subtitleKey: "login.roles.hr.subtitle" },
  { value: "it", icon: "mdi-cog-outline", titleKey: "login.roles.it.title", subtitleKey: "login.roles.it.subtitle" },
];

const username = ref("");
const password = ref("");
const selectedRole = ref<DemoRole>("employee");
const loading = ref(false);

const canSubmit = computed(() => username.value.trim() !== "" && password.value !== "" && !loading.value);

async function onLogin() {
  if (!canSubmit.value) return;
  loading.value = true;
  try {
    await authStore.login(username.value.trim(), password.value, selectedRole.value);
    router.push("/");
  } finally {
    loading.value = false;
  }
}
</script>