import { NavLink, Outlet } from 'react-router-dom'
import { paths } from '../paths.ts'
import './AppLayout.css'

// Role-based visibility of links comes with authentication (M2).
const navItems = [
  { to: paths.myHoliday, label: 'My holiday' },
  { to: paths.calendar, label: 'Calendar' },
  { to: paths.approvals, label: 'Approvals' },
  { to: paths.employees, label: 'Employees' },
  { to: paths.admin, label: 'Admin' },
]

export default function AppLayout() {
  return (
    <div className="app">
      <header className="app-header">
        <span className="app-brand">Holiday Planner</span>
        <nav aria-label="Main navigation">
          <ul className="app-nav">
            {navItems.map((item) => (
              <li key={item.to}>
                <NavLink to={item.to}>{item.label}</NavLink>
              </li>
            ))}
          </ul>
        </nav>
        <NavLink to={paths.login} className="app-logout">
          Log out
        </NavLink>
      </header>
      <main className="app-main">
        <Outlet />
      </main>
    </div>
  )
}
