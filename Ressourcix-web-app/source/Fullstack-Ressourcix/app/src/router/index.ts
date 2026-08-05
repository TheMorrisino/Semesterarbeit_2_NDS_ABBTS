import { createRouter, createWebHashHistory } from "vue-router";
import { useAuthStore } from "@/stores/auth";
import HomeView from "@/views/DashboardView.vue";

const router = createRouter({
  history: createWebHashHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: "/login",
      name: "login",
      component: () => import("../views/LoginView.vue"),
      meta: { titleKey: "app.nav.login", public: true },
    },
    {
      path: "/",
      name: "dashboard",
      component: () => import("../views/DashboardView.vue"),
      meta: { titleKey: "app.nav.dashboard" },
    },
    {
      path: "/calender",
      name: "calender",
      component: () => import("../views/CalenderView.vue"),
      meta: { titleKey: "app.nav.calender" },
    },
    {
      path: "/absences",
      name: "absences",
      component: () => import("../views/AbsencesView.vue"),
      meta: { titleKey: "app.nav.absences" },
    },
    {
    path: '/approval',
    name: 'approval',
    component: () => import('../views/ApprovalView.vue'),
    meta: { titleKey: "app.nav.approval" },
    },
    {
    path: '/teamview',
    name: 'teamview',
    component: () => import('../views/TeamView.vue'),
    meta: { titleKey: "app.nav.teamview" },
    },
    {
    path: '/employees',
    name: 'employees',
    component: () => import('../views/EmployeesView.vue'),
    meta: { titleKey: "app.nav.employees" },
    },
    {
    path: '/auditlog',
    name: 'auditlog',
    component: () => import('../views/AuditLogView.vue'),
    meta: { titleKey: "app.nav.auditlog" },
    },
    {
    path: '/messages',
    name: 'messages',
    component: () => import('../views/MessagesView.vue'),
    meta: { titleKey: "app.nav.messages" },
    },
    {
    path: '/logout',
    name: 'logout',
    component: () => import('../views/LogoutView.vue'),
    meta: { titleKey: "app.nav.logout" },
    },
  ],
});

router.beforeEach((to) => {
  const authStore = useAuthStore();
  console.log("Navigating to", to.path, "| loggedIn:", authStore.isLoggedIn);
  if (!to.meta.public && !authStore.isLoggedIn) {
    return { path: "/login" };
  }
  return true;
});

export default router;
