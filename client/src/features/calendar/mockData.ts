import type { CalendarEmployee, NonWorkingDay, PlannedAbsence } from './types.ts'

// TEMPORARY: replaced by API calls once the backend has employees, holiday requests
// and the public holiday calendar.

export const mockEmployees: CalendarEmployee[] = [
  { id: '0b6f1c8e-3f2a-4d1b-9a7e-1c2d3e4f5a01', name: 'Lianna L. Christensen' },
  { id: '0b6f1c8e-3f2a-4d1b-9a7e-1c2d3e4f5a02', name: 'Mikael K. Simonsen' },
  { id: '0b6f1c8e-3f2a-4d1b-9a7e-1c2d3e4f5a03', name: 'Niels Bock' },
  { id: '0b6f1c8e-3f2a-4d1b-9a7e-1c2d3e4f5a04', name: 'Noah Bock' },
  { id: '0b6f1c8e-3f2a-4d1b-9a7e-1c2d3e4f5a05', name: 'Susanne Nordenbæk' },
  { id: '0b6f1c8e-3f2a-4d1b-9a7e-1c2d3e4f5a06', name: 'Søs Josefsen' },
  { id: '0b6f1c8e-3f2a-4d1b-9a7e-1c2d3e4f5a07', name: 'Gert Laustsen' },
  { id: '0b6f1c8e-3f2a-4d1b-9a7e-1c2d3e4f5a08', name: 'Gitte Sydbøge' },
]

const [lianna, mikael, niels, noah, susanne, sos, gert, gitte] = mockEmployees.map((e) => e.id)

let nextId = 1
const absence = (
  employeeId: string,
  from: string,
  to: string,
  account: PlannedAbsence['account'] = 'Weeks1To5',
  status: PlannedAbsence['status'] = 'Approved',
): PlannedAbsence => ({
  id: `a1b2c3d4-0000-4000-8000-${String(nextId++).padStart(12, '0')}`,
  employeeId,
  from,
  to,
  account,
  status,
})

export const mockAbsences: PlannedAbsence[] = [
  // November 2026
  absence(lianna, '2026-11-02', '2026-11-06'),
  absence(noah, '2026-11-16', '2026-11-17', 'Week6'),
  absence(susanne, '2026-11-23', '2026-11-27', 'Weeks1To5', 'Pending'),
  absence(gert, '2026-11-26', '2026-11-27'),
  // December 2026
  absence(mikael, '2026-12-14', '2026-12-31'),
  absence(niels, '2026-12-21', '2026-12-30'),
  absence(noah, '2026-12-28', '2026-12-30', 'Week6'),
  absence(sos, '2026-12-07', '2026-12-09'),
  absence(gitte, '2026-12-21', '2026-12-23', 'Weeks1To5', 'Pending'),
  absence(lianna, '2026-12-28', '2027-01-08'),
  // January 2027
  absence(gert, '2027-01-11', '2027-01-15', 'Week6'),
  absence(susanne, '2027-01-25', '2027-01-29'),
]

export const mockNonWorkingDays: NonWorkingDay[] = [
  { date: '2026-12-24', name: 'Christmas Eve (company day off)' },
  { date: '2026-12-25', name: 'Christmas Day' },
  { date: '2026-12-26', name: 'Boxing Day' },
  { date: '2026-12-31', name: "New Year's Eve (company day off)" },
  { date: '2027-01-01', name: "New Year's Day" },
]
