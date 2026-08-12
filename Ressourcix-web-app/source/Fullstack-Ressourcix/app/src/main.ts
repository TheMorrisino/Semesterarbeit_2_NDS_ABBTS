/**
 * main.ts
 *
 * Bootstraps Vuetify and other plugins then mounts the App`
 */

// Composables
import { createApp } from 'vue'

// Plugins
import { registerPlugins } from '@/plugins'

// Components
import App from './App.vue'
import { useAuthStore } from '@/stores/auth'

// Styles
import 'unfonts.css'
import 'virtual:uno.css'
import './styles/main.scss'

const app = createApp(App)

registerPlugins(app)

async function bootstrap() {
  const authStore = useAuthStore()
  try {
    await authStore.checkSession()
  } catch {
    // Backend nicht erreichbar o.ä. - App trotzdem mounten (zeigt z.B. die Login-Seite)
  }
  app.mount('#app')
}

bootstrap()
