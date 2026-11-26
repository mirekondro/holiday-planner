import { describe, expect, it } from 'vitest'
import { parseStoredSession } from './session.ts'

const now = new Date('2026-11-26T12:00:00Z')
const valid = {
  token: 'abc',
  expiresAt: '2026-11-26T13:00:00Z',
  user: { id: '1', email: 'anna@businessesbjerg.com', role: 'Employee', employeeId: '2' },
}

describe('parseStoredSession', () => {
  it('returns a valid, not yet expired session', () => {
    expect(parseStoredSession(JSON.stringify(valid), now)?.token).toBe('abc')
  })

  it('drops an expired session', () => {
    expect(parseStoredSession(JSON.stringify({ ...valid, expiresAt: '2026-11-26T11:59:59Z' }), now)).toBeNull()
  })

  it.each([null, '', 'not json', '{}', JSON.stringify({ ...valid, token: '' })])('ignores broken data: %s', (raw) => {
    expect(parseStoredSession(raw, now)).toBeNull()
  })
})
