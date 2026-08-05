import { defineStore } from "pinia";

export type DemoRole = "employee" | "planner" | "hr" | "it";

export interface AuthUser {
  username: string;
  role: DemoRole;
}

const STORAGE_KEY = "ressourcix.auth";

function loadPersistedUser(): AuthUser | null {
  const raw = sessionStorage.getItem(STORAGE_KEY);
  if (!raw) return null;
  try {
    return JSON.parse(raw) as AuthUser;
  } catch {
    return null;
  }
}

export const useAuthStore = defineStore("auth", {
  state: () => ({
    user: loadPersistedUser() as AuthUser | null,
  }),
  getters: {
    isLoggedIn: (state) => state.user !== null,
    role: (state) => state.user?.role ?? null,
  },
  actions: {
    // TODO: durch echten API-Call ersetzen, sobald Backend-Auth existiert
    async login(username: string, _password: string, role: DemoRole) {
      const user: AuthUser = { username, role };
      this.user = user;
      sessionStorage.setItem(STORAGE_KEY, JSON.stringify(user));
    },
    logout() {
      this.user = null;
      sessionStorage.removeItem(STORAGE_KEY);
    },
  },
});