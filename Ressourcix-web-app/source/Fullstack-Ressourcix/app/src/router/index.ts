import { createRouter, createWebHashHistory } from "vue-router";
import HomeView from "@/views/DashboardView.vue";

const router = createRouter({
  history: createWebHashHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: "/",
      name: "dashboard",
      component: () => import("../views/DashboardView.vue"),
    },
    {
      path: "/calender",
      name: "calender",
      // route level code-splitting
      // this generates a separate chunk (About.[hash].js) for this route
      // which is lazy-loaded when the route is visited.
      component: () => import("../views/CalenderView.vue"),
    },
    {
      path: "/absences",
      name: "absences",
      component: () => import("../views/AbsencesView.vue"),
    },
    {
    path: '/approval',
    name: 'approval',
    component: () => import('../views/ApprovalView.vue'),
    },
    {
    path: '/teamview',
    name: 'teamview',
    component: () => import('../views/TeamView.vue'),
    },
    {
    path: '/employees',
    name: 'employees',
    component: () => import('../views/EmployeesView.vue'),
    },
    {
    path: '/auditlog',
    name: 'auditlog',
    component: () => import('../views/AuditLogView.vue'),
    },
    {
    path: '/messages',
    name: 'messages',
    component: () => import('../views/MessagesView.vue'),
    },
    {
    path: '/logout',
    name: 'logout',
    component: () => import('../views/LogoutView.vue'),
    },
  ],
});

export default router;
