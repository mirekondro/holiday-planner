import { Link } from 'react-router-dom'
import { paths } from '../paths.ts'

export default function NotFoundPage() {
  return (
    <section className="app-main">
      <h1>Page not found</h1>
      <p>
        <Link to={paths.myHoliday}>Back to my holiday</Link>
      </p>
    </section>
  )
}
