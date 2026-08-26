import React from 'react';
import { Navigate, Route, Routes } from 'react-router-dom';
import { useSelector } from 'react-redux';
import LoginPage from './pages/AuthenticationAndIdentity/LoginPage';
import UserManagementPage from './pages/AuthenticationAndIdentity/UserManagementPage';
import type { RootState } from './store';

/**
 * ProtectedRoute — redirects unauthenticated users to /login.
 */
const ProtectedRoute = ({ children }: { children: React.ReactElement }) => {
  const isAuthenticated = useSelector((s: RootState) => s.authentication.isAuthenticated);
  return isAuthenticated ? children : <Navigate to="/login" replace />;
};

/**
 * RootRedirect — smart redirect from '/'.
 * Authenticated users go to their role-appropriate home screen.
 * Unauthenticated users go to /login.
 */
const RootRedirect = () => {
  const { isAuthenticated, role } = useSelector((s: RootState) => s.authentication);
  if (!isAuthenticated) return <Navigate to="/login" replace />;
  if (role === 'HR_ADMIN' || role === 'SUPER_ADMIN') {
    return <Navigate to="/admin/users" replace />;
  }
  // Other roles (EMPLOYEE, MANAGER) will land on /leave once F-02 is built;
  // for now redirect to /login so they get a clean state rather than a blank screen.
  return <Navigate to="/login" replace />;
};

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route
        path="/admin/users"
        element={
          <ProtectedRoute>
            <UserManagementPage />
          </ProtectedRoute>
        }
      />
      {/* Root: smart role-based redirect */}
      <Route path="/" element={<RootRedirect />} />
      {/* Catch-all: unknown routes redirect to root (which handles auth check) */}
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
