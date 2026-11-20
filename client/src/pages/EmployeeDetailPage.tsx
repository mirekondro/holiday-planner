import { useParams } from 'react-router-dom'

export default function EmployeeDetailPage() {
  const { employeeId } = useParams<{ employeeId: string }>()

  return (
    <section>
      <h1>Employee</h1>
      <p className="placeholder">
        Details, work schedule history and balances for employee <code>{employeeId}</code> will be here.
      </p>
    </section>
  )
}
