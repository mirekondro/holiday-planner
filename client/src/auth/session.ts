import type { Role } from './access.ts'

export interface CurrentUser {
  id: string
  email: string
  role: Role
  employeeId: string | null
}

export interface Session {
  token: string
  expiresAt: string // ISO date-time
  user: CurrentUser
}

// The session lives in sessionStorage: it survives a page refresh but is gone when the tab closes,
// which limits how long a token lying in the browser can be abused. An httpOnly cookie would hide it
// from JavaScript entirely; see "Further work".
const storageKey = 'holiday-planner.session'

/** Reads a stored session. Returns null for anything broken or expired. Pure, so it is unit tested. */
export function parseStoredSession(raw: string | null, now: Date): Session | null {
  if (!raw) return null
  try {
    const session = JSON.parse(raw) as Partial<Session>
    if (!session.token || !session.expiresAt || !session.user?.role) return null
    if (new Date(session.expiresAt).getTime() <= now.getTime()) return null
    return session as Session
  } catch {
    return null
  }
}

function readStorage(): string | null {
  try {
    return sessionStorage.getItem(storageKey)
  } catch {
    return null // storage can be unavailable (privacy mode, tests)
  }
}

function writeStorage(value: string | null) {
  try {
    if (value === null) sessionStorage.removeItem(storageKey)
    else sessionStorage.setItem(storageKey, value)
  } catch {
    // ignore – the session still lives in memory for this page
  }
}

// --- A tiny store that React components subscribe to with useSyncExternalStore -------------------

let current: Session | null = parseStoredSession(readStorage(), new Date())
const listeners = new Set<() => void>()

export function getSession(): Session | null {
  return current
}

export function setSession(session: Session | null) {
  current = session
  writeStorage(session ? JSON.stringify(session) : null)
  listeners.forEach((listener) => listener())
}

export function subscribe(listener: () => void): () => void {
  listeners.add(listener)
  return () => listeners.delete(listener)
}
