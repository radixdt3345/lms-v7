import { Navigate, Route, Routes } from 'react-router-dom';
import { useSelector } from 'react-redux';
import LoginPage from './pages/AuthenticationAndIdentity/LoginPage';
import UserManagementPage from './pages/AuthenticationAndIdentity/UserManagementPage';
import EmployeeManagementPage from './pages/EmployeeManagement/EmployeeManagementPage';
import MyProfilePage from './pages/EmployeeManagement/MyProfilePage';
import MyTeamPage from './pages/EmployeeManagement/MyTeamPage';
import LeaveTypeManagementPage from './pages/LeaveTypeManagement/LeaveTypeManagementPage';
import PublicHolidayManagementPage from './pages/PublicHolidayManagement/PublicHolidayManagementPage';
import AuditTrailPage from './pages/AuditTrail/AuditTrailPage';
import MyLeaveBalancePage from './pages/LeaveBalance/MyLeaveBalancePage';
import LeaveBalanceManagementPage from './pages/LeaveBalance/LeaveBalanceManagementPage';
import type { RootState } from './store';

const ProtectedRoute = ({ children }: { children: React.ReactElement }) => {
  const isAuthenticated = useSelector((s: RootState) => s.authentication.isAuthenticated);
  return isAuthenticated ? children : <Navigate to="/login" replace />;
};

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />

      {/* F-01: Auth & Identity */}
      <Route path="/admin/users" element={<ProtectedRoute><UserManagementPage /></ProtectedRoute>} />

      {/* F-02: Employee Management */}
      <Route path="/admin/employees" element={<ProtectedRoute><EmployeeManagementPage /></ProtectedRoute>} />
      <Route path="/profile" element={<ProtectedRoute><MyProfilePage /></ProtectedRoute>} />
      <Route path="/team" element={<ProtectedRoute><MyTeamPage /></ProtectedRoute>} />

      {/* F-04: Leave Type Management */}
      <Route path="/admin/leave-types" element={<ProtectedRoute><LeaveTypeManagementPage /></ProtectedRoute>} />

      {/* F-05: Leave Balance Management */}
      <Route path="/leave-balance" element={<ProtectedRoute><MyLeaveBalancePage /></ProtectedRoute>} />
      <Route path="/admin/leave-balances" element={<ProtectedRoute><LeaveBalanceManagementPage /></ProtectedRoute>} />

      {/* F-10: Public Holiday Management */}
      <Route path="/admin/holidays" element={<ProtectedRoute><PublicHolidayManagementPage /></ProtectedRoute>} />

      {/* F-13: Audit Trail */}
      <Route path="/admin/audit-trail" element={<ProtectedRoute><AuditTrailPage /></ProtectedRoute>} />

      <Route path="/" element={<Navigate to="/login" replace />} />
    </Routes>
  );
}
