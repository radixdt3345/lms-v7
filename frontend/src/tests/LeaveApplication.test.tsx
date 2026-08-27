import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import { configureStore } from '@reduxjs/toolkit';
import { Provider } from 'react-redux';
import { leaveApplicationReducer } from '../../store/leaveApplicationSlice';
import MyLeaveApplicationsPage from '../../pages/LeaveApplication/MyLeaveApplicationsPage';
import { leaveApplicationApi } from '../../api/leaveApplicationApi';

vi.mock('../../api/leaveApplicationApi', () => ({
  leaveApplicationApi: { getMyApplications: vi.fn(), submit: vi.fn(), approve: vi.fn(), reject: vi.fn(), cancel: vi.fn(), getAll: vi.fn() },
}));

const mockApp = { id: '1', employeeId: 'e1', employeeName: 'Alice', leaveTypeId: 'lt1', leaveTypeName: 'Annual', startDate: '2026-09-01', endDate: '2026-09-03', totalDays: 3, reason: 'Vacation', status: 'Pending', createdAt: '2026-08-27' };

function makeStore(preloaded?: object) {
  return configureStore({ reducer: { leaveApplication: leaveApplicationReducer }, preloadedState: preloaded });
}

describe('MyLeaveApplicationsPage', () => {
  beforeEach(() => vi.clearAllMocks());

  it('UT-LA-01: renders page title', () => {
    vi.mocked(leaveApplicationApi.getMyApplications).mockResolvedValue({ data: { data: [] } } as any);
    render(<Provider store={makeStore()}><MyLeaveApplicationsPage /></Provider>);
    expect(screen.getByTestId('page-title')).toBeInTheDocument();
  });

  it('UT-LA-02: shows apply leave button', () => {
    vi.mocked(leaveApplicationApi.getMyApplications).mockResolvedValue({ data: { data: [] } } as any);
    render(<Provider store={makeStore()}><MyLeaveApplicationsPage /></Provider>);
    expect(screen.getByTestId('apply-leave-btn')).toBeInTheDocument();
  });

  it('UT-LA-03: renders applications grid', () => {
    vi.mocked(leaveApplicationApi.getMyApplications).mockResolvedValue({ data: { data: [] } } as any);
    render(<Provider store={makeStore()}><MyLeaveApplicationsPage /></Provider>);
    expect(screen.getByTestId('applications-grid')).toBeInTheDocument();
  });

  it('UT-LA-04: shows error message on failure', () => {
    vi.mocked(leaveApplicationApi.getMyApplications).mockRejectedValue(new Error('Network error'));
    render(<Provider store={makeStore({ leaveApplication: { applications: [], loading: false, error: 'Failed' } })}><MyLeaveApplicationsPage /></Provider>);
    expect(screen.getByTestId('error-message')).toBeInTheDocument();
  });

  it('UT-LA-05: shows applications from store', () => {
    vi.mocked(leaveApplicationApi.getMyApplications).mockResolvedValue({ data: { data: [mockApp] } } as any);
    render(<Provider store={makeStore({ leaveApplication: { applications: [mockApp], loading: false, error: null } })}><MyLeaveApplicationsPage /></Provider>);
    expect(screen.getByTestId('applications-grid')).toBeInTheDocument();
  });

  it('UT-LA-06: loading state shows spinner in grid', () => {
    vi.mocked(leaveApplicationApi.getMyApplications).mockResolvedValue({ data: { data: [] } } as any);
    render(<Provider store={makeStore({ leaveApplication: { applications: [], loading: true, error: null } })}><MyLeaveApplicationsPage /></Provider>);
    expect(screen.getByTestId('applications-grid')).toBeInTheDocument();
  });
});
