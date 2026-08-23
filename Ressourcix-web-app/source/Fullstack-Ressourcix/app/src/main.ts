/**
 * main.ts
 *
 * Bootstraps Vuetify and other plugins then mounts the App`
 */

// Composables
import { createApp } from 'vue'

// Plugins
import { registerPlugins } from '@/plugins'
import router from '@/router'

// Assets
import ressourcixLogo from '@/assets/Ressourcix_Icon_OhneB2.png'

//Stores
import { useAuthStore } from '@/stores/auth'

// Components
import App from './App.vue'

// Styles
import 'unfonts.css'
import 'virtual:uno.css'
import './styles/main.scss'

const favicon = document.createElement('link')
favicon.rel = 'icon'
favicon.type = 'image/png'
favicon.href = ressourcixLogo
document.head.append(favicon)

const app = createApp(App)

registerPlugins(app)

async function bootstrap () {
  const authStore = useAuthStore()
  try {
    await authStore.checkSession()
  } catch (error) {
    // Backend nicht erreichbar o.ä. - App trotzdem mounten (zeigt z.B. die Login-Seite)
    console.warn('[bootstrap] checkSession fehlgeschlagen', error)
  }

  // Router erst jetzt installieren: die erste Navigation (und damit der Auth-Guard in
  // router/index.ts) läuft dadurch garantiert mit einem bereits geladenen authStore.
  app.use(router)
  await router.isReady()
  app.mount('#app')
}

bootstrap()
