import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import { configureStore } from '@reduxjs/toolkit';
import { Provider } from 'react-redux';
import { compOffReducer } from '../../store/compOffSlice';
import MyCompOffPage from '../../pages/CompOff/MyCompOffPage';
import { compOffApi } from '../../api/compOffApi';
vi.mock('../../api/compOffApi', () => ({ compOffApi: { getMyRequests: vi.fn(), submit: vi.fn(), approve: vi.fn(), reject: vi.fn(), getMyCredits: vi.fn(), getAllRequests: vi.fn() } }));
function makeStore(p?: object) { return configureStore({ reducer: { compOff: compOffReducer }, preloadedState: p }); }
describe('MyCompOffPage', () => {
  beforeEach(() => vi.clearAllMocks());
  it('UT-CO-01: renders page title', () => { vi.mocked(compOffApi.getMyRequests).mockResolvedValue({ data: { data: [] } } as any); render(<Provider store={makeStore()}><MyCompOffPage /></Provider>); expect(screen.getByTestId('page-title')).toBeInTheDocument(); });
  it('UT-CO-02: shows request button', () => { vi.mocked(compOffApi.getMyRequests).mockResolvedValue({ data: { data: [] } } as any); render(<Provider store={makeStore()}><MyCompOffPage /></Provider>); expect(screen.getByTestId('request-comp-off-btn')).toBeInTheDocument(); });
  it('UT-CO-03: renders grid', () => { vi.mocked(compOffApi.getMyRequests).mockResolvedValue({ data: { data: [] } } as any); render(<Provider store={makeStore()}><MyCompOffPage /></Provider>); expect(screen.getByTestId('comp-off-grid')).toBeInTheDocument(); });
  it('UT-CO-04: shows error when store has error', () => { vi.mocked(compOffApi.getMyRequests).mockRejectedValue(new Error()); render(<Provider store={makeStore({ compOff: { requests: [], loading: false, error: 'Failed' } })}><MyCompOffPage /></Provider>); expect(screen.getByTestId('error-message')).toBeInTheDocument(); });
  it('UT-CO-05: loading from store', () => { vi.mocked(compOffApi.getMyRequests).mockResolvedValue({ data: { data: [] } } as any); render(<Provider store={makeStore({ compOff: { requests: [], loading: true, error: null } })}><MyCompOffPage /></Provider>); expect(screen.getByTestId('comp-off-grid')).toBeInTheDocument(); });
  it('UT-CO-06: empty state renders grid', () => { vi.mocked(compOffApi.getMyRequests).mockResolvedValue({ data: { data: [] } } as any); render(<Provider store={makeStore({ compOff: { requests: [], loading: false, error: null } })}><MyCompOffPage /></Provider>); expect(screen.getByTestId('comp-off-grid')).toBeInTheDocument(); });
});
