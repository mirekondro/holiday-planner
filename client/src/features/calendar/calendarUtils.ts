import type { CalendarEmployee, NonWorkingDay, PlannedAbsence } from './types.ts'

/** Month in the calendar. `month` is 1–12 (not JavaScript's 0–11). */
export interface YearMonth {
  year: number
  month: number
}

export interface CalendarDay {
  date: string // ISO date (YYYY-MM-DD)
  dayOfMonth: number
  weekday: number // 0 = Sunday … 6 = Saturday
  isWeekend: boolean
  nonWorkingDay?: NonWorkingDay
}

const pad = (n: number) => String(n).padStart(2, '0')

export function toIsoDate(year: number, month: number, day: number): string {
  return `${year}-${pad(month)}-${pad(day)}`
}

export function formatYearMonth({ year, month }: YearMonth): string {
  return `${year}-${pad(month)}`
}

/** Reads "YYYY-MM" (e.g. from the URL). Falls back to the month of `today` when missing or invalid. */
export function parseYearMonth(value: string | null, today: Date): YearMonth {
  const match = value?.match(/^(\d{4})-(\d{2})$/)
  if (match) {
    const year = Number(match[1])
    const month = Number(match[2])
    if (month >= 1 && month <= 12) return { year, month }
  }
  return { year: today.getFullYear(), month: today.getMonth() + 1 }
}

export function addMonths({ year, month }: YearMonth, delta: number): YearMonth {
  const index = year * 12 + (month - 1) + delta
  return { year: Math.floor(index / 12), month: (index % 12) + 1 }
}

export function daysOfMonth({ year, month }: YearMonth, nonWorkingDays: NonWorkingDay[] = []): CalendarDay[] {
  const count = new Date(year, month, 0).getDate() // day 0 of next month = last day of this month
  const byDate = new Map(nonWorkingDays.map((d) => [d.date, d]))

  return Array.from({ length: count }, (_, i) => {
    const day = i + 1
    const date = toIsoDate(year, month, day)
    const weekday = new Date(year, month - 1, day).getDay()
    return {
      date,
      dayOfMonth: day,
      weekday,
      isWeekend: weekday === 0 || weekday === 6,
      nonWorkingDay: byDate.get(date),
    }
  })
}

export function isWorkingDay(day: CalendarDay): boolean {
  return !day.isWeekend && !day.nonWorkingDay
}

/** ISO dates compare correctly as strings, so no Date objects are needed here. */
export function findAbsence(
  absences: PlannedAbsence[],
  employeeId: string,
  date: string,
): PlannedAbsence | undefined {
  return absences.find((a) => a.employeeId === employeeId && a.from <= date && date <= a.to)
}

/** Number of employees with planned holiday on a working day (0 on weekends and days off). */
export function countAbsent(employees: CalendarEmployee[], absences: PlannedAbsence[], day: CalendarDay): number {
  if (!isWorkingDay(day)) return 0
  return employees.filter((e) => findAbsence(absences, e.id, day.date)).length
}
