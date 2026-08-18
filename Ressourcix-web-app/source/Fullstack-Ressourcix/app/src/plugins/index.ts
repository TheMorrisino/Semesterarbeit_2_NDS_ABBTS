// Types
import type { App } from 'vue'
import { createPinia } from 'pinia'
/**
 * plugins/index.ts
 *
 * Automatically included in `./src/main.ts`
 */
import i18n from './i18n'
// Plugins
import vuetify from './vuetify'

// Router wird bewusst NICHT hier registriert: er muss erst installiert werden, nachdem
// main.ts den Session-Status geladen hat, sonst läuft die erste Navigation (inkl.
// Auth-Guard in router/index.ts) mit einem noch leeren authStore und lässt z.B. /login
// durch, obwohl die Session bereits gültig ist (sichtbar erst, nachdem checkSession() später
// durchläuft, ohne dass die Route neu ausgewertet wird - leere Seite nach F5).
export function registerPlugins (app: App) {
  app.use(vuetify)
  app.use(createPinia())
  app.use(i18n)
}
