<template>
  <v-row justify="center">
    <v-col cols="12" sm="8" md="5">
      <v-card class="pa-8" rounded="xl" elevation="4">
        <div class="d-flex align-center ga-3 mb-4">
          <v-icon icon="mdi-lock-reset" size="28" />
          <span class="text-h6 font-weight-bold">{{ t('changePassword.title') }}</span>
        </div>
        <p class="text-body-2 text-medium-emphasis mb-6">{{ t('changePassword.subtitle') }}</p>

        <v-form ref="formRef" v-model="formValid" @submit.prevent="onSubmit">
          <v-text-field
            v-model="currentPassword"
            :label="t('changePassword.currentPassword')"
            type="password"
            variant="outlined"
            density="comfortable"
            class="mb-4"
            :rules="[(v: string) => !!v || t('changePassword.required')]"
          />
          <v-text-field
            v-model="newPassword"
            :label="t('changePassword.newPassword')"
            type="password"
            variant="outlined"
            density="comfortable"
            class="mb-4"
            :rules="[(v: string) => isStrongPassword(v) || t('changePassword.minLength')]"
          />
          <v-text-field
            v-model="confirmPassword"
            :label="t('changePassword.confirmPassword')"
            type="password"
            variant="outlined"
            density="comfortable"
            class="mb-6"
            :rules="[(v: string) => v === newPassword || t('changePassword.mismatch')]"
          />

          <v-alert v-if="errorMessage" type="error" density="compact" class="mb-4">
            {{ errorMessage }}
          </v-alert>

          <v-btn
            color="teal-700"
            block
            size="large"
            rounded="lg"
            type="submit"
            :disabled="!formValid"
            :loading="loading"
          >
            {{ t('changePassword.submit') }}
          </v-btn>
        </v-form>
      </v-card>
    </v-col>
  </v-row>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { useI18n } from "vue-i18n";
import { useRouter } from "vue-router";
import { useAuthStore } from "@/stores/auth";
import { ApiError } from "@/api/httpClient";

const { t } = useI18n();
const router = useRouter();
const authStore = useAuthStore();

const formRef = ref();
const formValid = ref(false);
const currentPassword = ref("");
const newPassword = ref("");
const confirmPassword = ref("");
const loading = ref(false);
const errorMessage = ref("");

// Muss mit IsStrongPassword() im Backend (Program.cs) übereinstimmen.
function isStrongPassword(value: string): boolean {
  return (
    !!value &&
    value.length >= 8 &&
    /[A-Z]/.test(value) &&
    /[a-z]/.test(value) &&
    /[0-9]/.test(value) &&
    /[^A-Za-z0-9]/.test(value)
  );
}

async function onSubmit() {
  if (!formValid.value) return;
  loading.value = true;
  errorMessage.value = "";
  try {
    await authStore.changePassword(currentPassword.value, newPassword.value);
    router.push("/");
  } catch (error) {
    // Der Server unterscheidet zwei 400-Fälle (falsches aktuelles Passwort vs. neues Passwort zu
    // schwach) mit je eigener Meldung - die geben wir jetzt 1:1 weiter statt sie zu vereinheitlichen.
    if (error instanceof ApiError && error.status === 400) {
      errorMessage.value = error.message;
    } else {
      console.error("[changePassword] unerwarteter Fehler", error);
      errorMessage.value = t("changePassword.genericError");
    }
  } finally {
    loading.value = false;
  }
}
</script>
