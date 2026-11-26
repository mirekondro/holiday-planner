import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { api, isHttpError } from '../api/client.ts'
import type { EmployeeEntity } from '../api/generated/Api.ts'
import { paths } from '../paths.ts'

type State =
  | { status: 'loading' }
  | { status: 'loaded'; employees: EmployeeEntity[] }
  | { status: 'error'; message: string }

const dateFormat = new Intl.DateTimeFormat('en-GB', { dateStyle: 'medium' })

function errorMessage(error: unknown): string {
  if (isHttpError(error) && error.status === 401) return 'Please log in to see employees.'
  if (isHttpError(error) && error.status === 403) return 'You do not have access to employees.'
  return 'Could not load employees. Is the API running?'
}

export default function EmployeesPage() {
  const [state, setState] = useState<State>({ status: 'loading' })

  useEffect(() => {
    let cancelled = false // ignore the answer if the page was left before it arrived
    api.api
      .employeesGetAll()
      .then((employees) => !cancelled && setState({ status: 'loaded', employees }))
      .catch((error: unknown) => !cancelled && setState({ status: 'error', message: errorMessage(error) }))
    return () => {
      cancelled = true
    }
  }, [])

  return (
    <section>
      <h1>Employees</h1>

      {state.status === 'loading' && <p>Loading…</p>}

      {state.status === 'error' && (
        <p role="alert" className="placeholder">
          {state.message}
        </p>
      )}

      {state.status === 'loaded' && state.employees.length === 0 && <p>No employees yet.</p>}

      {state.status === 'loaded' && state.employees.length > 0 && (
        <table className="data-table">
          <thead>
            <tr>
              <th scope="col">Name</th>
              <th scope="col">Email</th>
              <th scope="col">Employed since</th>
            </tr>
          </thead>
          <tbody>
            {state.employees.map((employee) => (
              <tr key={employee.id}>
                <td>
                  <Link to={paths.employee(employee.id ?? '')}>
                    {employee.firstName} {employee.lastName}
                  </Link>
                </td>
                <td>{employee.email}</td>
                <td>{employee.employmentStart ? dateFormat.format(new Date(employee.employmentStart)) : '–'}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </section>
  )
}
