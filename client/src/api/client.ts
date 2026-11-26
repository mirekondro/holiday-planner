import { getSession, setSession } from '../auth/session.ts'
import { Api, type HttpResponse } from './generated/Api.ts'

/**
 * Adds the JWT to every request. (The generated `secure` flag only covers endpoints with an explicit
 * [Authorize]; most of ours are protected by the server's fallback policy, so we always send it.)
 * If the server answers 401 while we think we are logged in, the token has expired or was rejected:
 * we drop the session and the protected routes send the user to the login page.
 */
const authFetch: typeof fetch = async (input, init) => {
  const session = getSession()
  const headers = new Headers(init?.headers)
  if (session) headers.set('Authorization', `Bearer ${session.token}`)

  const response = await fetch(input, { ...init, headers })
  if (response.status === 401 && session) setSession(null)
  return response
}

// One shared API client for the whole app.
// Dev: VITE_API_URL is empty and Vite proxies /api to the backend (vite.config.ts).
// Production: VITE_API_URL is the URL of the deployed API, set at build time.
export const api = new Api({ baseUrl: import.meta.env.VITE_API_URL ?? '', customFetch: authFetch })

/** The generated client throws the fetch Response (with .status) when the server answers 4xx/5xx. */
export function isHttpError(error: unknown): error is HttpResponse<unknown, unknown> {
  return error instanceof Response
}
