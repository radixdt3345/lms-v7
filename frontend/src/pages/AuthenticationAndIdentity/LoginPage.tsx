import React, { useState } from 'react';
import {
  Alert,
  Box,
  Button,
  CircularProgress,
  Divider,
  Paper,
  TextField,
  Typography,
} from '@mui/material';
import { useDispatch, useSelector } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import type { AppDispatch, RootState } from '../../store';
import { loginAsync } from '../../store/authenticationSlice';
import { getSsoLoginUrl } from '../../api/authenticationApi';

/**
 * SCR-001 - Login Page
 * Allows users to authenticate via Azure AD SSO or local email/password.
 * data-testid map: btn-sso-login, input-email, input-password,
 *                  btn-local-login, alert-login-error, notice-account-locked
 */
export default function LoginPage() {
  const dispatch = useDispatch<AppDispatch>();
  const navigate = useNavigate();
  const { isLoading, error } = useSelector((s: RootState) => s.authentication);

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [accountLocked, setAccountLocked] = useState(false);

  const handleSsoLogin = () => {
    window.location.href = getSsoLoginUrl();
  };

  const handleLocalLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setAccountLocked(false);
    const result = await dispatch(loginAsync({ email, password }));
    if (loginAsync.fulfilled.match(result)) {
      navigate('/dashboard');
    } else {
      const msg = ((result.payload as string) ?? '').toLowerCase();
      if (msg.includes('locked')) {
        setAccountLocked(true);
      }
    }
  };

  return (
    <Box
      sx={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        bgcolor: 'background.default',
      }}
    >
      <Paper elevation={3} sx={{ p: 4, width: '100%', maxWidth: 420 }}>
        <Typography variant="h5" fontWeight={700} mb={3} textAlign="center">
          Leave Management System
        </Typography>

        {/* Azure AD SSO */}
        <Button
          fullWidth
          variant="contained"
          size="large"
          data-testid="btn-sso-login"
          onClick={handleSsoLogin}
          sx={{ mb: 2 }}
        >
          Sign in with Microsoft
        </Button>

        <Divider sx={{ my: 2 }}>or sign in with email</Divider>

        {/* Account locked notice */}
        {accountLocked && (
          <Alert severity="error" data-testid="notice-account-locked" sx={{ mb: 2 }}>
            Your account has been locked after 3 failed attempts. Contact an HR Admin to
            unlock it.
          </Alert>
        )}

        {/* General error banner */}
        {error && !accountLocked && (
          <Alert severity="error" data-testid="alert-login-error" sx={{ mb: 2 }}>
            {error}
          </Alert>
        )}

        {/* Local login form */}
        <Box component="form" onSubmit={(e) => void handleLocalLogin(e)} noValidate>
          <TextField
            fullWidth
            label="Email"
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            inputProps={{ 'data-testid': 'input-email' }}
            margin="normal"
            required
            autoComplete="email"
          />
          <TextField
            fullWidth
            label="Password"
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            inputProps={{ 'data-testid': 'input-password' }}
            margin="normal"
            required
            autoComplete="current-password"
          />
          <Button
            type="submit"
            fullWidth
            variant="outlined"
            size="large"
            data-testid="btn-local-login"
            disabled={isLoading}
            sx={{ mt: 2 }}
            startIcon={isLoading ? <CircularProgress size={18} /> : null}
          >
            {isLoading ? 'Signing in...' : 'Sign in with Email'}
          </Button>
        </Box>
      </Paper>
    </Box>
  );
}
