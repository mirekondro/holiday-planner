import { useNavigate } from 'react-router-dom'
import { paths } from '../paths.ts'

// Placeholder – real login (JWT) comes in M2.
export default function LoginPage() {
  const navigate = useNavigate()

  return (
    <main className="app-main">
      <h1>Log in</h1>
      <p className="placeholder">The login form will be here.</p>
      <p>
        <button type="button" onClick={() => navigate(paths.myHoliday)}>
          Continue without login (dev)
        </button>
      </p>
    </main>
  )
}
