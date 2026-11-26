import { NavLink, Outlet } from 'react-router-dom'
import { canAccess, pageRoles, type Role } from '../auth/access.ts'
import { logout, useSession } from '../auth/useAuth.ts'
import { paths } from '../paths.ts'
import './AppLayout.css'

const navItems: { to: string; label: string; roles: readonly Role[] }[] = [
  { to: paths.myHoliday, label: 'My holiday', roles: pageRoles.myHoliday },
  { to: paths.calendar, label: 'Calendar', roles: pageRoles.calendar },
  { to: paths.approvals, label: 'Approvals', roles: pageRoles.approvals },
  { to: paths.employees, label: 'Employees', roles: pageRoles.employees },
  { to: paths.admin, label: 'Admin', roles: pageRoles.admin },
]

export default function AppLayout() {
  const session = useSession()
  const role = session?.user.role

  return (
    <div className="app">
      <header className="app-header">
        <span className="app-brand">Holiday Planner</span>
        <nav aria-label="Main navigation">
          <ul className="app-nav">
            {navItems
              .filter((item) => role && canAccess(role, item.roles))
              .map((item) => (
                <li key={item.to}>
                  <NavLink to={item.to}>{item.label}</NavLink>
                </li>
              ))}
          </ul>
        </nav>
        {session && (
          <span className="app-user" title={session.user.role}>
            {session.user.email}
          </span>
        )}
        {/* Logging out clears the session; RequireAuth then sends us to the login page. */}
        <button type="button" className="app-logout" onClick={logout}>
          Log out
        </button>
      </header>
      <main className="app-main">
        <Outlet />
      </main>
    </div>
  )
}
