import { createRouter, createWebHashHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = createRouter({
  history: createWebHashHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/login',
      name: 'login',
      components: { loginView: () => import('../views/LoginView.vue') },
      meta: { titleKey: 'app.nav.login', public: true },
    },
    {
      path: '/change-password',
      name: 'change-password',
      components: { mainView: () => import('../views/ChangePasswordView.vue') },
      meta: { titleKey: 'app.nav.changePassword' },
    },
    {
      path: '/',
      name: 'dashboard',
      components: { mainView: () => import('../views/DashboardView.vue') },
      meta: { titleKey: 'app.nav.dashboard' },
    },
    {
      path: '/calender',
      name: 'calender',
      components: { mainView: () => import('../views/CalenderView.vue') },
      meta: { titleKey: 'app.nav.calender' },
    },
    {
      path: '/absences',
      name: 'absences',
      components: { mainView: () => import('../views/AbsencesView.vue') },
      meta: { titleKey: 'app.nav.absences' },
    },
    {
      path: '/approval',
      name: 'approval',
      components: { mainView: () => import('../views/ApprovalView.vue') },
      meta: { titleKey: 'app.nav.approval', requiresAdmin: true },
    },
    {
      path: '/teamview',
      name: 'teamview',
      components: { mainView: () => import('../views/TeamView.vue') },
      meta: { titleKey: 'app.nav.teamview' },
    },
    {
      path: '/employees',
      name: 'employees',
      components: { mainView: () => import('../views/EmployeesView.vue') },
      meta: { titleKey: 'app.nav.employees' },
    },
    {
      path: '/auditlog',
      name: 'auditlog',
      components: { mainView: () => import('../views/AuditLogView.vue') },
      meta: { titleKey: 'app.nav.auditlog' },
    },
  ],
})

router.beforeEach(to => {
  const authStore = useAuthStore()
  if (!to.meta.public && !authStore.isLoggedIn) {
    return { path: '/login' }
  }
  if (authStore.isLoggedIn && authStore.user?.mustChangePassword && to.name !== 'change-password') {
    return { path: '/change-password' }
  }
  if (to.meta.requiresAdmin && (authStore.user?.permissionLevel ?? 0) < 5) {
    return { path: '/' }
  }
  return true
})

export default router
