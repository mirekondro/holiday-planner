import type { ReactNode } from 'react'
import { Link, Navigate, useLocation } from 'react-router-dom'
import { paths } from '../paths.ts'
import { canAccess, homePath, type Role } from './access.ts'
import { useSession } from './useAuth.ts'

interface RequireAuthProps {
  /** Roles allowed to see the page. Leave out for "any logged-in user". */
  roles?: readonly Role[]
  children: ReactNode
}

/** Guards a route in the UI. The real protection is on the API – this just avoids showing dead ends. */
export default function RequireAuth({ roles, children }: RequireAuthProps) {
  const session = useSession()
  const location = useLocation()

  if (!session) {
    // Remember where the user wanted to go, so login can send them back there.
    return <Navigate to={paths.login} replace state={{ from: location.pathname + location.search }} />
  }

  if (roles && !canAccess(session.user.role, roles)) {
    return (
      <section>
        <h1>No access</h1>
        <p className="placeholder">
          Your role ({session.user.role}) cannot open this page. <Link to={homePath(session.user.role)}>Go to your start page</Link>
        </p>
      </section>
    )
  }

  return children
}

/** "/" sends each role to its own start page. */
export function HomeRedirect() {
  const session = useSession()
  return <Navigate to={session ? homePath(session.user.role) : paths.login} replace />
}
