// Shapes the calendar works with. When the API exists these will come from the
// generated client (src/api/generated) instead of being defined here.

export type HolidayAccount = 'Weeks1To5' | 'Week6'
export type AbsenceStatus = 'Approved' | 'Pending'

export interface CalendarEmployee {
  id: string // GUID
  name: string
}

export interface PlannedAbsence {
  id: string // GUID
  employeeId: string
  from: string // ISO date (YYYY-MM-DD), inclusive
  to: string // ISO date (YYYY-MM-DD), inclusive
  account: HolidayAccount
  status: AbsenceStatus
}

/** Public holiday or company day off (e.g. 24 December). Nobody works on these days. */
export interface NonWorkingDay {
  date: string // ISO date
  name: string
}
