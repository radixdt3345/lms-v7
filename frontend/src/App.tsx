import { Navigate, Route, Routes } from 'react-router-dom';
import { useSelector } from 'react-redux';
import LoginPage from './pages/AuthenticationAndIdentity/LoginPage';
import UserManagementPage from './pages/AuthenticationAndIdentity/UserManagementPage';
import EmployeeManagementPage from './pages/EmployeeManagement/EmployeeManagementPage';
import MyProfilePage from './pages/EmployeeManagement/MyProfilePage';
import MyTeamPage from './pages/EmployeeManagement/MyTeamPage';
import AuditTrailPage from './pages/AuditTrail/AuditTrailPage';
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
      <Route
        path="/admin/users"
        element={
          <ProtectedRoute>
            <UserManagementPage />
          </ProtectedRoute>
        }
      />

      {/* F-02: Employee Management */}
      <Route
        path="/admin/employees"
        element={
          <ProtectedRoute>
            <EmployeeManagementPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/profile"
        element={
          <ProtectedRoute>
            <MyProfilePage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/team"
        element={
          <ProtectedRoute>
            <MyTeamPage />
          </ProtectedRoute>
        }
      />

      {/* F-13: Audit Trail — HR_ADMIN / SUPER_ADMIN only */}
      <Route
        path="/admin/audit-trail"
        element={
          <ProtectedRoute>
            <AuditTrailPage />
          </ProtectedRoute>
        }
      />

      <Route path="/" element={<Navigate to="/login" replace />} />
    </Routes>
  );
}
