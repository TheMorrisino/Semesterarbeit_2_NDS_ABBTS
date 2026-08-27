<template>
  <v-row justify="center">
    <v-col cols="12" md="5" sm="8">
      <v-card class="pa-8" elevation="4" rounded="xl">
        <div class="d-flex align-center ga-3 mb-4">
          <v-icon icon="mdi-lock-reset" size="28" />
          <span class="text-h6 font-weight-bold">{{ t('changePassword.title') }}</span>
        </div>

        <p class="text-body-2 text-medium-emphasis mb-6">{{ t('changePassword.subtitle') }}</p>

        <v-form ref="formRef" v-model="formValid" @submit.prevent="onSubmit">
          <v-text-field
            v-model="currentPassword"
            class="mb-4"
            density="comfortable"
            :label="t('changePassword.currentPassword')"
            :rules="[(v: string) => !!v || t('changePassword.required')]"
            type="password"
            variant="outlined"
          />

          <v-text-field
            v-model="newPassword"
            class="mb-4"
            density="comfortable"
            :label="t('changePassword.newPassword')"
            :rules="[(v: string) => isStrongPassword(v) || t('changePassword.minLength')]"
            type="password"
            variant="outlined"
          />

          <v-text-field
            v-model="confirmPassword"
            class="mb-6"
            density="comfortable"
            :label="t('changePassword.confirmPassword')"
            :rules="[(v: string) => v === newPassword || t('changePassword.mismatch')]"
            type="password"
            variant="outlined"
          />

          <v-alert v-if="errorMessage" class="mb-4" density="compact" type="error">
            {{ errorMessage }}
          </v-alert>

          <v-btn
            block
            color="teal-700"
            :disabled="!formValid"
            :loading="loading"
            rounded="lg"
            size="large"
            type="submit"
          >
            {{ t('changePassword.submit') }}
          </v-btn>
        </v-form>
      </v-card>
    </v-col>
  </v-row>
</template>

<script setup lang="ts">
  import { ref } from 'vue'
  import { useI18n } from 'vue-i18n'
  import { useRouter } from 'vue-router'
  import { ApiError } from '@/api/httpClient'
  import { useAuthStore } from '@/stores/auth'

  const { t } = useI18n()
  const router = useRouter()
  const authStore = useAuthStore()

  const formRef = ref()
  const formValid = ref(false)
  const currentPassword = ref('')
  const newPassword = ref('')
  const confirmPassword = ref('')
  const loading = ref(false)
  const errorMessage = ref('')

  // Muss mit IsStrongPassword() im Backend (Program.cs) übereinstimmen.
  function isStrongPassword (value: string): boolean {
    return (
      !!value
      && value.length >= 8
      && /[A-Z]/.test(value)
      && /[a-z]/.test(value)
      && /\d/.test(value)
      && /[^A-Z0-9]/i.test(value)
    )
  }

  async function onSubmit () {
    if (!formValid.value) return
    loading.value = true
    errorMessage.value = ''
    try {
      await authStore.changePassword(currentPassword.value, newPassword.value)
      router.push('/')
    } catch (error) {
      // Der Server unterscheidet zwei 400-Fälle (falsches aktuelles Passwort vs. neues Passwort zu
      // schwach) mit je eigener Meldung - die geben wir jetzt 1:1 weiter statt sie zu vereinheitlichen.
      if (error instanceof ApiError && error.status === 400) {
        errorMessage.value = error.message
      } else {
        console.error('[changePassword] unerwarteter Fehler', error)
        errorMessage.value = t('changePassword.genericError')
      }
    } finally {
      loading.value = false
    }
  }
</script>
