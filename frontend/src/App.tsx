import EmployeeDashboardPage from "./pages/Dashboard/EmployeeDashboardPage";
import ApprovalQueuePage from "./pages/Approvals/ApprovalQueuePage";
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
import MyLeaveApplicationsPage from './pages/LeaveApplication/MyLeaveApplicationsPage';
import LeaveApplicationManagementPage from './pages/LeaveApplication/LeaveApplicationManagementPage';
import BackgroundJobsPage from './pages/BackgroundJobs/BackgroundJobsPage';
import ReportsPage from './pages/Reports/ReportsPage';
import MyCompOffPage from './pages/CompOff/MyCompOffPage';
import CompOffManagementPage from './pages/CompOff/CompOffManagementPage';
import type { RootState } from './store';

const ProtectedRoute = ({ children }: { children: React.ReactElement }) => {
  const isAuthenticated = useSelector((s: RootState) => s.authentication.isAuthenticated);
  return isAuthenticated ? children : <Navigate to="/login" replace />;
};

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/admin/users" element={<ProtectedRoute><UserManagementPage /></ProtectedRoute>} />
      <Route path="/admin/employees" element={<ProtectedRoute><EmployeeManagementPage /></ProtectedRoute>} />
      <Route path="/profile" element={<ProtectedRoute><MyProfilePage /></ProtectedRoute>} />
      <Route path="/team" element={<ProtectedRoute><MyTeamPage /></ProtectedRoute>} />
      <Route path="/admin/leave-types" element={<ProtectedRoute><LeaveTypeManagementPage /></ProtectedRoute>} />
      <Route path="/leave-balance" element={<ProtectedRoute><MyLeaveBalancePage /></ProtectedRoute>} />
      <Route path="/admin/leave-balances" element={<ProtectedRoute><LeaveBalanceManagementPage /></ProtectedRoute>} />
      <Route path="/leave-applications" element={<ProtectedRoute><MyLeaveApplicationsPage /></ProtectedRoute>} />
      <Route path="/admin/leave-applications" element={<ProtectedRoute><LeaveApplicationManagementPage /></ProtectedRoute>} />
      <Route path="/admin/holidays" element={<ProtectedRoute><PublicHolidayManagementPage /></ProtectedRoute>} />
      <Route path="/admin/audit-trail" element={<ProtectedRoute><AuditTrailPage /></ProtectedRoute>} />
            <Route path="/comp-off" element={<ProtectedRoute><MyCompOffPage /></ProtectedRoute>} />
      <Route path="/admin/comp-off" element={<ProtectedRoute><CompOffManagementPage /></ProtectedRoute>} />
      <Route path="/" element={<Navigate to="/login" replace />} />
      <Route path="/admin/approvals" element={<ApprovalQueuePage />} />
  <Route path="/dashboard" element={<EmployeeDashboardPage />} />
</Routes>
  );
}
