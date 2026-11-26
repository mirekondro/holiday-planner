import { createBrowserRouter } from 'react-router-dom'
import { pageRoles } from './auth/access.ts'
import RequireAuth, { HomeRedirect } from './auth/RequireAuth.tsx'
import AppLayout from './layouts/AppLayout.tsx'
import AdminPage from './pages/AdminPage.tsx'
import ApprovalsPage from './pages/ApprovalsPage.tsx'
import CalendarPage from './pages/CalendarPage.tsx'
import EmployeeDetailPage from './pages/EmployeeDetailPage.tsx'
import EmployeesPage from './pages/EmployeesPage.tsx'
import LoginPage from './pages/LoginPage.tsx'
import MyHolidayPage from './pages/MyHolidayPage.tsx'
import NotFoundPage from './pages/NotFoundPage.tsx'
import { paths } from './paths.ts'

export const router = createBrowserRouter([
  // Login is outside the layout – no navigation before you are signed in.
  { path: paths.login, element: <LoginPage /> },
  {
    path: '/',
    // Everything inside the layout needs a logged-in user; some pages also need a specific role.
    element: (
      <RequireAuth>
        <AppLayout />
      </RequireAuth>
    ),
    errorElement: <NotFoundPage />,
    children: [
      { index: true, element: <HomeRedirect /> },
      {
        path: 'my-holiday',
        element: (
          <RequireAuth roles={pageRoles.myHoliday}>
            <MyHolidayPage />
          </RequireAuth>
        ),
      },
      { path: 'calendar', element: <CalendarPage /> },
      { path: 'employees', element: <EmployeesPage /> },
      { path: 'employees/:employeeId', element: <EmployeeDetailPage /> },
      {
        path: 'approvals',
        element: (
          <RequireAuth roles={pageRoles.approvals}>
            <ApprovalsPage />
          </RequireAuth>
        ),
      },
      {
        path: 'admin',
        element: (
          <RequireAuth roles={pageRoles.admin}>
            <AdminPage />
          </RequireAuth>
        ),
      },
      { path: '*', element: <NotFoundPage /> },
    ],
  },
])
