export const paths = {
  login: '/login',
  myHoliday: '/my-holiday',
  calendar: '/calendar',
  approvals: '/approvals',
  employees: '/employees',
  employee: (employeeId: string) => `/employees/${employeeId}`,
  admin: '/admin',
} as const
