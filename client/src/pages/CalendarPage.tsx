import { useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { addMonths, daysOfMonth, formatYearMonth, parseYearMonth, toIsoDate } from '../features/calendar/calendarUtils.ts'
import HolidayGrid from '../features/calendar/HolidayGrid.tsx'
import { mockAbsences, mockEmployees, mockNonWorkingDays } from '../features/calendar/mockData.ts'

const monthFormat = new Intl.DateTimeFormat('en-GB', { month: 'long', year: 'numeric' })

export default function CalendarPage() {
  // The shown month lives in the URL (?month=2026-12), so it survives refresh and can be shared.
  const [searchParams, setSearchParams] = useSearchParams()
  const [now] = useState(() => new Date()) // read the clock once, not on every render
  const today = toIsoDate(now.getFullYear(), now.getMonth() + 1, now.getDate())
  const shown = parseYearMonth(searchParams.get('month'), now)

  const days = daysOfMonth(shown, mockNonWorkingDays)

  const goTo = (delta: number) => setSearchParams({ month: formatYearMonth(addMonths(shown, delta)) })

  return (
    <section>
      <h1>Holiday calendar</h1>

      <div className="calendar-toolbar">
        <button type="button" onClick={() => goTo(-1)} aria-label="Previous month">
          ‹ Previous
        </button>
        <h2 aria-live="polite">{monthFormat.format(new Date(shown.year, shown.month - 1, 1))}</h2>
        <button type="button" onClick={() => goTo(1)} aria-label="Next month">
          Next ›
        </button>
        <button type="button" onClick={() => setSearchParams({})}>
          Today
        </button>
      </div>

      <HolidayGrid days={days} employees={mockEmployees} absences={mockAbsences} today={today} />

      <ul className="calendar-legend">
        <li>
          <span className="swatch" style={{ background: 'var(--holiday)' }} />F – holiday (weeks 1–5)
        </li>
        <li>
          <span className="swatch" style={{ background: 'var(--week6)' }} />6 – 6th holiday week
        </li>
        <li>
          <span className="swatch" style={{ background: 'var(--pending-a)' }} />
          Waiting for approval
        </li>
        <li>
          <span className="swatch" style={{ background: 'var(--off-bg)' }} />
          Weekend / public holiday / company day off
        </li>
      </ul>
    </section>
  )
}
