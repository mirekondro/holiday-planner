import { countAbsent, findAbsence, isWorkingDay, type CalendarDay } from './calendarUtils.ts'
import type { CalendarEmployee, NonWorkingDay, PlannedAbsence } from './types.ts'
import './HolidayGrid.css'

interface HolidayGridProps {
  days: CalendarDay[]
  employees: CalendarEmployee[]
  absences: PlannedAbsence[]
  today: string // ISO date
}

const weekdayShort = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat']
const accountLabel: Record<PlannedAbsence['account'], string> = {
  Weeks1To5: 'Holiday (weeks 1–5)',
  Week6: '6th holiday week',
}

function dayClass(day: CalendarDay, today: string): string {
  return [!isWorkingDay(day) && 'off', day.date === today && 'today'].filter(Boolean).join(' ')
}

function cellContent(absence: PlannedAbsence | undefined, day: CalendarDay, nonWorking?: NonWorkingDay) {
  if (!absence || !isWorkingDay(day)) return { label: '', className: '', title: nonWorking?.name }
  const pending = absence.status === 'Pending'
  return {
    label: absence.account === 'Week6' ? '6' : 'F',
    className: `absence ${absence.account === 'Week6' ? 'week6' : 'weeks1to5'} ${pending ? 'pending' : ''}`,
    title: `${accountLabel[absence.account]}${pending ? ' – waiting for approval' : ''}`,
  }
}

export default function HolidayGrid({ days, employees, absences, today }: HolidayGridProps) {
  return (
    <div className="holiday-grid-scroll">
      <table className="holiday-grid">
        <thead>
          <tr>
            <th scope="col" className="name-col">
              Employee
            </th>
            {days.map((day) => (
              <th key={day.date} scope="col" className={dayClass(day, today)} title={day.nonWorkingDay?.name}>
                <span className="weekday">{weekdayShort[day.weekday]}</span>
                <span>{day.dayOfMonth}</span>
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {employees.map((employee) => (
            <tr key={employee.id}>
              <th scope="row" className="name-col">
                {employee.name}
              </th>
              {days.map((day) => {
                const cell = cellContent(findAbsence(absences, employee.id, day.date), day, day.nonWorkingDay)
                return (
                  <td
                    key={day.date}
                    className={`${dayClass(day, today)} ${cell.className}`}
                    title={cell.title ? `${employee.name}: ${cell.title}` : undefined}
                  >
                    {cell.label}
                  </td>
                )
              })}
            </tr>
          ))}
        </tbody>
        <tfoot>
          <tr>
            <th scope="row" className="name-col">
              Absent
            </th>
            {days.map((day) => {
              const absent = countAbsent(employees, absences, day)
              return (
                <td key={day.date} className={`${dayClass(day, today)} ${absent > 0 ? 'has-absent' : ''}`}>
                  {isWorkingDay(day) ? absent : ''}
                </td>
              )
            })}
          </tr>
        </tfoot>
      </table>
    </div>
  )
}
