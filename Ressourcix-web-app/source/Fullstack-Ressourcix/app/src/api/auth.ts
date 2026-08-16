import { httpClient } from './httpClient'

export interface AuthUserDto {
  id: string
  username: string
  name: string
  permissionLevel: number
  mustChangePassword: boolean
}

export const authApi = {
  login: (username: string, password: string) =>
    httpClient.post<AuthUserDto>('/api/auth/login', { username, password }),
  me: () => httpClient.get<AuthUserDto>('/api/auth/me'),
  logout: () => httpClient.post<void>('/api/auth/logout', undefined),
  changePassword: (currentPassword: string, newPassword: string) =>
    httpClient.post<void>('/api/auth/change-password', { currentPassword, newPassword }),
}
