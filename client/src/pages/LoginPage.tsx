import { useState, type FormEvent } from 'react'
import { Navigate, useLocation, useNavigate } from 'react-router-dom'
import { isHttpError } from '../api/client.ts'
import { isOrganizationEmail, organizationDomain, pathAfterLogin } from '../auth/access.ts'
import { login, useSession } from '../auth/useAuth.ts'
import './LoginPage.css'

function loginErrorMessage(error: unknown): string {
  if (isHttpError(error)) {
    if (error.status === 401) return 'Invalid email or password.'
    if (error.status === 429) return 'Too many login attempts. Wait a minute and try again.'
    if (error.status === 400) return 'Please check your email and password.'
  }
  return 'Could not reach the server. Try again later.'
}

export default function LoginPage() {
  const session = useSession()
  const navigate = useNavigate()
  const location = useLocation()
  const from = (location.state as { from?: string } | null)?.from

  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [submitting, setSubmitting] = useState(false)

  // Already logged in (e.g. opened /login in a new tab) – go straight in.
  if (session) return <Navigate to={pathAfterLogin(session.user.role, from)} replace />

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setError(null)

    // Quick client-side checks for a better experience. The server validates everything again.
    if (!isOrganizationEmail(email)) {
      setError(`Use your organisation email (…${organizationDomain}).`)
      return
    }
    if (!password) {
      setError('Enter your password.')
      return
    }

    setSubmitting(true)
    try {
      const newSession = await login(email, password)
      navigate(pathAfterLogin(newSession.user.role, from), { replace: true })
    } catch (err) {
      setError(loginErrorMessage(err))
      setPassword('')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <main className="login">
      <form className="login-card" onSubmit={handleSubmit} noValidate>
        <h1>Holiday Planner</h1>
        <p className="login-subtitle">Business Esbjerg</p>

        <label htmlFor="email">Email</label>
        <input
          id="email"
          type="email"
          autoComplete="username"
          placeholder={`name${organizationDomain}`}
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          required
        />

        <label htmlFor="password">Password</label>
        <input
          id="password"
          type="password"
          autoComplete="current-password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
        />

        {error && (
          <p className="login-error" role="alert">
            {error}
          </p>
        )}

        <button type="submit" disabled={submitting}>
          {submitting ? 'Logging in…' : 'Log in'}
        </button>
      </form>
    </main>
  )
}
