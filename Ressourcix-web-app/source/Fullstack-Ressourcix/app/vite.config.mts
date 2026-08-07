import Fonts from "unplugin-fonts/vite";
import UnoCSS from "unocss/vite";
import Vue from "@vitejs/plugin-vue";
import Vuetify, { transformAssetUrls } from "vite-plugin-vuetify";
import { defineConfig } from "vite";
import { fileURLToPath, URL } from "node:url";

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [
    Vue({
      template: { transformAssetUrls },
    }), // https://github.com/vuetifyjs/vuetify-loader/tree/master/packages/vite-plugin#readme
    Vuetify({
      autoImport: true,
      styles: {
        configFile: "src/styles/settings.scss",
      },
    }),
    Fonts({
      fontsource: {
        families: [
          {
            name: "Roboto Mono",
            weights: [400, 700],
          },
          {
            name: "Roboto",
            weights: [100, 300, 400, 500, 700, 900],
            styles: ["normal", "italic"],
          },
        ],
      },
    }),
    UnoCSS(),
  ],
  define: { "process.env": {} },
  resolve: {
    alias: {
      "@": fileURLToPath(new URL("src", import.meta.url)),
    },
    extensions: [".js", ".json", ".jsx", ".mjs", ".ts", ".tsx", ".vue"],
  },
  server: {
    port: 3000,
    strictPort: true,
    host: true,
    warmup: {
      clientFiles: [
        "./src/views/LoginView.vue",
        "./src/views/DashboardView.vue",
        "./src/views/CalenderView.vue",
        "./src/views/AbsencesView.vue",
        "./src/views/ApprovalView.vue",
        "./src/views/TeamView.vue",
        "./src/views/EmployeesView.vue",
        "./src/views/AuditLogView.vue",
        "./src/views/MessagesView.vue",
        "./src/views/LogoutView.vue",
      ],
    },
  },
});
