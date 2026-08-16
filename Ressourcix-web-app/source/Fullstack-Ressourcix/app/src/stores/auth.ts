import { defineStore } from 'pinia'
import { authApi, type AuthUserDto } from '@/api/auth'
import { ApiError } from '@/api/httpClient'

export interface AuthUser {
  id: string
  username: string
  name: string
  permissionLevel: number
  mustChangePassword: boolean
}

function toAuthUser (dto: AuthUserDto): AuthUser {
  return { ...dto }
}

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: null as AuthUser | null,
    sessionChecked: false,
  }),
  getters: {
    isLoggedIn: state => state.user !== null,
  },
  actions: {
    async login (username: string, password: string) {
      const dto = await authApi.login(username, password)
      this.user = toAuthUser(dto)
    },
    async logout () {
      await authApi.logout()
      this.user = null
    },
    async checkSession () {
      try {
        const dto = await authApi.me()
        this.user = toAuthUser(dto)
      } catch (error) {
        if (!(error instanceof ApiError && error.status === 401)) {
          console.error('[auth] checkSession fehlgeschlagen', error)
          throw error
        }
        this.user = null
      } finally {
        this.sessionChecked = true
      }
    },
    async changePassword (currentPassword: string, newPassword: string) {
      await authApi.changePassword(currentPassword, newPassword)
      if (this.user) {
        this.user.mustChangePassword = false
      }
    },
  },
})
