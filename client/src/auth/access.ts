import type { UserRole } from '../api/generated/Api.ts'
import { paths } from '../paths.ts'

// Who may open which page. Mirrors the authorization table in README.md.
// This only decides what the UI shows – the API enforces the same rules on every request.

export type Role = UserRole

export const allRoles: readonly Role[] = ['Employee', 'Accountant', 'SystemOwner']

export const pageRoles = {
  myHoliday: ['Employee'],
  calendar: allRoles,
  employees: allRoles,
  approvals: ['Accountant'],
  admin: ['SystemOwner'],
} as const satisfies Record<string, readonly Role[]>

export function canAccess(role: Role, allowed: readonly Role[]): boolean {
  return allowed.includes(role)
}

/** Roles allowed on a URL path, so login never sends someone back to a page they cannot open. */
export function rolesForPath(path: string): readonly Role[] {
  const match = (base: string) => path === base || path.startsWith(`${base}/`) || path.startsWith(`${base}?`)
  if (match(paths.myHoliday)) return pageRoles.myHoliday
  if (match(paths.approvals)) return pageRoles.approvals
  if (match(paths.admin)) return pageRoles.admin
  return allRoles
}

/** After login: back to the page the user came from if their role may open it, else their start page. */
export function pathAfterLogin(role: Role, from: string | undefined): string {
  if (from && from !== paths.login && canAccess(role, rolesForPath(from))) return from
  return homePath(role)
}

/** Where each role lands after login. */
export function homePath(role: Role): string {
  switch (role) {
    case 'Employee':
      return paths.myHoliday
    case 'Accountant':
      return paths.approvals
    case 'SystemOwner':
      return paths.admin
  }
}

export const organizationDomain = '@businessesbjerg.com'

/** Same rule as UserRules.IsOrganizationEmail on the server (the server checks again). */
export function isOrganizationEmail(email: string): boolean {
  const normalized = email.trim().toLowerCase()
  return (
    normalized.length > organizationDomain.length &&
    normalized.endsWith(organizationDomain) &&
    normalized.indexOf('@') === normalized.length - organizationDomain.length
  )
}
