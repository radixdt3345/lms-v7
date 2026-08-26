import React from 'react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { Provider } from 'react-redux';
import { MemoryRouter } from 'react-router-dom';
import { configureStore } from '@reduxjs/toolkit';
import authenticationReducer from '../store/authenticationSlice';
import LoginPage from '../pages/AuthenticationAndIdentity/LoginPage';
import * as authApi from '../api/authenticationApi';

// Mocks

vi.mock('../api/authenticationApi', () => ({
  getSsoLoginUrl: vi.fn(() => 'https://login.microsoftonline.com/test/oauth2/authorize'),
  loginWithCredentials: vi.fn(),
  logout: vi.fn(),
  getLockedAccounts: vi.fn(),
  unlockAccount: vi.fn(),
}));

const mockNavigate = vi.fn();
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return { ...actual, useNavigate: () => mockNavigate };
});

// Helpers

interface AuthOverride {
  isLoading?: boolean;
  error?: string | null;
  isAuthenticated?: boolean;
}

function makeStore(override: AuthOverride = {}) {
  return configureStore({
    reducer: { authentication: authenticationReducer },
    preloadedState: {
      authentication: {
        accessToken: null,
        isAuthenticated: false,
        role: null,
        name: null,
        email: null,
        isLoading: false,
        error: null,
        ...override,
      },
    },
  });
}

function renderLogin(override: AuthOverride = {}) {
  return render(
    <Provider store={makeStore(override)}>
      <MemoryRouter>
        <LoginPage />
      </MemoryRouter>
    </Provider>
  );
}

// Tests

describe('LoginPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockNavigate.mockReset();
  });

  // UT-FE-001: Idle render - all interactive elements present
  it('UT-FE-001: renders SSO button, email input, password input, and submit button', () => {
    renderLogin();
    expect(screen.getByTestId('btn-sso-login')).toBeInTheDocument();
    expect(screen.getByTestId('input-email')).toBeInTheDocument();
    expect(screen.getByTestId('input-password')).toBeInTheDocument();
    expect(screen.getByTestId('btn-local-login')).toBeInTheDocument();
  });

  // UT-FE-002: SSO button navigates to Azure AD URL
  it('UT-FE-002: clicking SSO button sets window.location.href to Azure AD URL', () => {
    const savedLocation = window.location;
    Object.defineProperty(window, 'location', {
      writable: true,
      value: { href: '' },
    });
    renderLogin();
    fireEvent.click(screen.getByTestId('btn-sso-login'));
    expect(window.location.href).toContain('login.microsoftonline.com');
    Object.defineProperty(window, 'location', { writable: true, value: savedLocation });
  });

  // UT-FE-003: Loading state disables submit button
  it('UT-FE-003: submit button is disabled when isLoading is true', () => {
    renderLogin({ isLoading: true });
    expect(screen.getByTestId('btn-local-login')).toBeDisabled();
  });

  // UT-FE-004: Error banner shown on login failure
  it('UT-FE-004: shows alert-login-error banner when login fails with non-locked error', async () => {
    vi.mocked(authApi.loginWithCredentials).mockRejectedValueOnce({
      response: { data: { detail: 'Invalid credentials' } },
    });
    renderLogin();
    fireEvent.change(screen.getByTestId('input-email'), {
      target: { value: 'bad@example.com' },
    });
    fireEvent.change(screen.getByTestId('input-password'), {
      target: { value: 'wrongpass' },
    });
    fireEvent.click(screen.getByTestId('btn-local-login'));
    await waitFor(() =>
      expect(screen.getByTestId('alert-login-error')).toBeInTheDocument()
    );
    expect(screen.queryByTestId('notice-account-locked')).not.toBeInTheDocument();
  });

  // UT-FE-005: Account locked notice shown when account is locked
  it('UT-FE-005: shows notice-account-locked when error message contains "locked"', async () => {
    vi.mocked(authApi.loginWithCredentials).mockRejectedValueOnce({
      response: { data: { detail: 'Account locked' } },
    });
    renderLogin();
    fireEvent.change(screen.getByTestId('input-email'), {
      target: { value: 'locked@example.com' },
    });
    fireEvent.change(screen.getByTestId('input-password'), {
      target: { value: 'Password1!' },
    });
    fireEvent.click(screen.getByTestId('btn-local-login'));
    await waitFor(() =>
      expect(screen.getByTestId('notice-account-locked')).toBeInTheDocument()
    );
    expect(screen.queryByTestId('alert-login-error')).not.toBeInTheDocument();
  });

  // UT-FE-006: No error banners on initial idle render
  it('UT-FE-006: no error or locked banners on initial render', () => {
    renderLogin();
    expect(screen.queryByTestId('alert-login-error')).not.toBeInTheDocument();
    expect(screen.queryByTestId('notice-account-locked')).not.toBeInTheDocument();
  });
});
