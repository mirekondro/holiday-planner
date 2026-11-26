import { useSyncExternalStore } from 'react'
import { api } from '../api/client.ts'
import { getSession, setSession, subscribe, type Session } from './session.ts'

export async function login(email: string, password: string): Promise<Session> {
  const response = await api.api.authLogin({ email: email.trim(), password })
  const { token, expiresAt, user } = response
  if (!token || !expiresAt || !user?.id || !user.email || !user.role)
    throw new Error('Unexpected login response from the server.')

  const session: Session = {
    token,
    expiresAt,
    user: { id: user.id, email: user.email, role: user.role, employeeId: user.employeeId ?? null },
  }
  setSession(session)
  return session
}

export function logout() {
  setSession(null)
}

/** The logged-in user, or null. Re-renders the component when someone logs in or out. */
export function useSession(): Session | null {
  return useSyncExternalStore(subscribe, getSession)
}
