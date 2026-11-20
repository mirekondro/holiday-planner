import { describe, expect, it } from 'vitest'
import {
  addMonths,
  countAbsent,
  daysOfMonth,
  findAbsence,
  isWorkingDay,
  parseYearMonth,
} from './calendarUtils.ts'
import type { CalendarEmployee, PlannedAbsence } from './types.ts'

const anna: CalendarEmployee = { id: 'a', name: 'Anna' }
const bo: CalendarEmployee = { id: 'b', name: 'Bo' }

const absence = (employeeId: string, from: string, to: string): PlannedAbsence => ({
  id: `${employeeId}-${from}`,
  employeeId,
  from,
  to,
  account: 'Weeks1To5',
  status: 'Approved',
})

describe('parseYearMonth', () => {
  const today = new Date(2026, 10, 18) // 18 Nov 2026

  it('reads a valid YYYY-MM value', () => {
    expect(parseYearMonth('2027-02', today)).toEqual({ year: 2027, month: 2 })
  })

  it.each([null, '', '2026-13', '2026-00', 'hello'])('falls back to the current month for %s', (value) => {
    expect(parseYearMonth(value, today)).toEqual({ year: 2026, month: 11 })
  })
})

describe('addMonths', () => {
  it('moves across a year boundary in both directions', () => {
    expect(addMonths({ year: 2026, month: 12 }, 1)).toEqual({ year: 2027, month: 1 })
    expect(addMonths({ year: 2027, month: 1 }, -1)).toEqual({ year: 2026, month: 12 })
  })
})

describe('daysOfMonth', () => {
  it('handles month lengths including leap years', () => {
    expect(daysOfMonth({ year: 2026, month: 11 })).toHaveLength(30)
    expect(daysOfMonth({ year: 2028, month: 2 })).toHaveLength(29)
    expect(daysOfMonth({ year: 2027, month: 2 })).toHaveLength(28)
  })

  it('marks weekends and non-working days', () => {
    const days = daysOfMonth({ year: 2026, month: 12 }, [{ date: '2026-12-24', name: 'Christmas Eve' }])
    const day = (n: number) => days[n - 1]

    expect(day(5).isWeekend).toBe(true) // Saturday
    expect(day(7).isWeekend).toBe(false) // Monday
    expect(day(24).nonWorkingDay?.name).toBe('Christmas Eve')
    expect(isWorkingDay(day(24))).toBe(false)
    expect(isWorkingDay(day(23))).toBe(true)
  })
})

describe('findAbsence', () => {
  const absences = [absence('a', '2026-12-07', '2026-12-11')]

  it('includes the first and last day of the range', () => {
    expect(findAbsence(absences, 'a', '2026-12-07')).toBeDefined()
    expect(findAbsence(absences, 'a', '2026-12-11')).toBeDefined()
  })

  it('ignores days outside the range and other employees', () => {
    expect(findAbsence(absences, 'a', '2026-12-06')).toBeUndefined()
    expect(findAbsence(absences, 'a', '2026-12-12')).toBeUndefined()
    expect(findAbsence(absences, 'b', '2026-12-08')).toBeUndefined()
  })
})

describe('countAbsent', () => {
  const absences = [absence('a', '2026-12-01', '2026-12-31'), absence('b', '2026-12-07', '2026-12-08')]
  const days = daysOfMonth({ year: 2026, month: 12 }, [{ date: '2026-12-24', name: 'Christmas Eve' }])

  it('counts every employee on holiday on a working day', () => {
    expect(countAbsent([anna, bo], absences, days[6])).toBe(2) // Mon 7 Dec
    expect(countAbsent([anna, bo], absences, days[8])).toBe(1) // Wed 9 Dec
  })

  it('returns 0 on weekends and non-working days', () => {
    expect(countAbsent([anna, bo], absences, days[5])).toBe(0) // Sun 6 Dec
    expect(countAbsent([anna, bo], absences, days[23])).toBe(0) // 24 Dec
  })
})
