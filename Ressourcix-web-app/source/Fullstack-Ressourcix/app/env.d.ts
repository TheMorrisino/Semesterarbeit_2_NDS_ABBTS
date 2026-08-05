/// <reference types="vite/client" />
/// <reference types="vite-plugin-vue-layouts-next/client" />

import "vue-router";

declare module "vue-router" {
  interface RouteMeta {
    // i18n-Key für den Seitentitel in der Topbar (App.vue), z.B. "app.nav.calender"
    titleKey?: string;
    public?: boolean;
  }
}
