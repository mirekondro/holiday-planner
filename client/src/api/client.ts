import { Api, type HttpResponse } from './generated/Api.ts'

// One shared API client for the whole app.
// Dev: VITE_API_URL is empty and Vite proxies /api to the backend (vite.config.ts).
// Production: VITE_API_URL is the URL of the deployed API, set at build time.
export const api = new Api({ baseUrl: import.meta.env.VITE_API_URL ?? '' })

/** The generated client throws the fetch Response (with .status) when the server answers 4xx/5xx. */
export function isHttpError(error: unknown): error is HttpResponse<unknown, unknown> {
  return error instanceof Response
}
