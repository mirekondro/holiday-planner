import { createBrowserRouter, Navigate } from 'react-router-dom'
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
    element: <AppLayout />,
    errorElement: <NotFoundPage />,
    children: [
      { index: true, element: <Navigate to={paths.myHoliday} replace /> },
      { path: 'my-holiday', element: <MyHolidayPage /> },
      { path: 'calendar', element: <CalendarPage /> },
      { path: 'approvals', element: <ApprovalsPage /> },
      { path: 'employees', element: <EmployeesPage /> },
      { path: 'employees/:employeeId', element: <EmployeeDetailPage /> },
      { path: 'admin', element: <AdminPage /> },
      { path: '*', element: <NotFoundPage /> },
    ],
  },
])
