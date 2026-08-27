import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import { configureStore } from '@reduxjs/toolkit';
import { Provider } from 'react-redux';
import { leaveBalanceReducer } from '../../store/leaveBalanceSlice';
import MyLeaveBalancePage from '../../pages/LeaveBalance/MyLeaveBalancePage';
import { leaveBalanceApi } from '../../api/leaveBalanceApi';

vi.mock('../../api/leaveBalanceApi', () => ({
  leaveBalanceApi: {
    getMyBalances: vi.fn(),
    getAllBalances: vi.fn(),
    creditAnnual: vi.fn(),
    adjustBalance: vi.fn(),
  },
}));

const mockBalance = {
  id: '1', employeeId: 'e1', employeeName: 'Alice',
  leaveTypeId: 'lt1', leaveTypeName: 'Annual Leave',
  year: 2026, totalDays: 20, usedDays: 5, pendingDays: 1, remainingDays: 14,
};

function makeStore(preloaded = {}) {
  return configureStore({
    reducer: { leaveBalance: leaveBalanceReducer },
    preloadedState: preloaded,
  });
}

describe('MyLeaveBalancePage', () => {
  beforeEach(() => { vi.clearAllMocks(); });

  it('UT-LB-01: renders page title', () => {
    vi.mocked(leaveBalanceApi.getMyBalances).mockResolvedValue({ data: { data: [] } } as any);
    render(<Provider store={makeStore()}><MyLeaveBalancePage /></Provider>);
    expect(screen.getByTestId('page-title')).toBeInTheDocument();
  });

  it('UT-LB-02: shows loading spinner', () => {
    vi.mocked(leaveBalanceApi.getMyBalances).mockResolvedValue({ data: { data: [] } } as any);
    render(<Provider store={makeStore({ leaveBalance: { balances: [], loading: true, error: null } })}><MyLeaveBalancePage /></Provider>);
    expect(screen.getByTestId('loading-spinner')).toBeInTheDocument();
  });

  it('UT-LB-03: shows empty state', () => {
    vi.mocked(leaveBalanceApi.getMyBalances).mockResolvedValue({ data: { data: [] } } as any);
    render(<Provider store={makeStore({ leaveBalance: { balances: [], loading: false, error: null } })}><MyLeaveBalancePage /></Provider>);
    expect(screen.getByTestId('empty-state')).toBeInTheDocument();
  });

  it('UT-LB-04: shows error message', () => {
    vi.mocked(leaveBalanceApi.getMyBalances).mockRejectedValue(new Error('Network error'));
    render(<Provider store={makeStore({ leaveBalance: { balances: [], loading: false, error: 'Failed' } })}><MyLeaveBalancePage /></Provider>);
    expect(screen.getByTestId('error-message')).toBeInTheDocument();
  });

  it('UT-LB-05: renders balance cards', () => {
    vi.mocked(leaveBalanceApi.getMyBalances).mockResolvedValue({ data: { data: [mockBalance] } } as any);
    render(<Provider store={makeStore({ leaveBalance: { balances: [mockBalance], loading: false, error: null } })}><MyLeaveBalancePage /></Provider>);
    expect(screen.getByTestId(`balance-card-${mockBalance.leaveTypeId}`)).toBeInTheDocument();
  });

  it('UT-LB-06: year select is present', () => {
    vi.mocked(leaveBalanceApi.getMyBalances).mockResolvedValue({ data: { data: [] } } as any);
    render(<Provider store={makeStore()}><MyLeaveBalancePage /></Provider>);
    expect(screen.getByTestId('year-select')).toBeInTheDocument();
  });
});
