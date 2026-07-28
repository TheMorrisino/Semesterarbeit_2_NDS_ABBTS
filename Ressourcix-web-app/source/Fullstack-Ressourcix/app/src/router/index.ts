import { createRouter, createWebHashHistory } from "vue-router";
import HomeView from "@/views/HomeView.vue";

const router = createRouter({
  history: createWebHashHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: "/",
      name: "home",
      component: HomeView,
    },
    {
      path: "/abwesenheiten",
      name: "abwesenheiten",
      // route level code-splitting
      // this generates a separate chunk (About.[hash].js) for this route
      // which is lazy-loaded when the route is visited.
      component: () => import("../views/AbwesenheitenView.vue"),
    },
    {
      path: "/gallery",
      name: "gallery",
      component: () => import("../views/ImageGallery.vue"),
    },
    {
    path: '/mitarbeitende',
    name: 'mitarbeitende',
    component: () => import('../views/MitarbeitendeView.vue'),
    },
        {
    path: '/teamübersicht',
    name: 'teamübersicht',
    component: () => import('../views/TeamübersichtView.vue'),
    },
    
        {
    path: '/auditlog',
    name: 'auditlog',
    component: () => import('../views/AuditlogView.vue'),
    },
    
    
  ],
});

export default router;
