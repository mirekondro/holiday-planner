import { describe, expect, it } from 'vitest'
import { canAccess, homePath, isOrganizationEmail, pageRoles, pathAfterLogin } from './access.ts'

describe('canAccess', () => {
  it('only lets the accountant see approvals', () => {
    expect(canAccess('Accountant', pageRoles.approvals)).toBe(true)
    expect(canAccess('Employee', pageRoles.approvals)).toBe(false)
    expect(canAccess('SystemOwner', pageRoles.approvals)).toBe(false)
  })

  it('only lets the system owner see admin', () => {
    expect(canAccess('SystemOwner', pageRoles.admin)).toBe(true)
    expect(canAccess('Accountant', pageRoles.admin)).toBe(false)
  })

  it('lets everyone see the calendar', () => {
    expect(canAccess('Employee', pageRoles.calendar)).toBe(true)
    expect(canAccess('Accountant', pageRoles.calendar)).toBe(true)
    expect(canAccess('SystemOwner', pageRoles.calendar)).toBe(true)
  })
})

describe('homePath', () => {
  it('sends each role to its own start page', () => {
    expect(homePath('Employee')).toBe('/my-holiday')
    expect(homePath('Accountant')).toBe('/approvals')
    expect(homePath('SystemOwner')).toBe('/admin')
  })
})

describe('isOrganizationEmail', () => {
  it.each(['anna@businessesbjerg.com', ' Anna@BusinessEsbjerg.com '])('accepts %s', (email) => {
    expect(isOrganizationEmail(email)).toBe(true)
  })

  it.each(['anna@gmail.com', '@businessesbjerg.com', 'a@b@businessesbjerg.com', 'a@businessesbjerg.com.dk', ''])(
    'rejects %s',
    (email) => {
      expect(isOrganizationEmail(email)).toBe(false)
    },
  )
})

describe('pathAfterLogin', () => {
  it('goes back to the page the user came from', () => {
    expect(pathAfterLogin('Employee', '/calendar?month=2026-12')).toBe('/calendar?month=2026-12')
  })

  it('ignores a page the new role cannot open', () => {
    expect(pathAfterLogin('SystemOwner', '/my-holiday')).toBe('/admin')
    expect(pathAfterLogin('Employee', '/admin')).toBe('/my-holiday')
  })

  it('uses the start page when there is nowhere to go back to', () => {
    expect(pathAfterLogin('Accountant', undefined)).toBe('/approvals')
    expect(pathAfterLogin('Accountant', '/login')).toBe('/approvals')
  })
})
