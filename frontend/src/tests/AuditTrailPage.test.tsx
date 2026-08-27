import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { Provider } from 'react-redux';
import { configureStore } from '@reduxjs/toolkit';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import AuditTrailPage from '../pages/AuditTrail/AuditTrailPage';
import authenticationReducer from '../store/authenticationSlice';
import { auditLogReducer } from '../store/auditLogSlice';
import * as auditLogApi from '../api/auditLogApi';

vi.mock('../api/auditLogApi', () => ({
  searchAuditLogs: vi.fn(),
}));

vi.mock('@mui/x-data-grid', () => ({
  DataGrid: ({
    rows,
    loading,
    'data-testid': testId,
  }: {
    rows: Array<Record<string, unknown>>;
    loading: boolean;
    'data-testid'?: string;
  }) => {
    if (loading) return <div role="progressbar" />;
    return (
      <div data-testid={testId ?? 'data-grid'}>
        {rows.map((row) => (
          <div key={String(row['id'])} data-testid={`row-${String(row['id'])}`}>
            {String(row['actionType'])} — {String(row['recordType'])}
          </div>
        ))}
      </div>
    );
  },
}));

const emptyResult = { items: [], totalCount: 0, page: 1, pageSize: 50 };

const sampleResult = {
  items: [
    {
      id: 'aaa-111',
      actorUserId: 'uid-1',
      actorName: 'admin@example.com',
      actionType: 'CREATE',
      recordType: 'Employee',
      recordId: 'emp-1',
      oldValue: null,
      newValue: '{"name":"Alice"}',
      ipAddress: null,
      timestamp: '2026-08-27T10:00:00Z',
    },
  ],
  totalCount: 1,
  page: 1,
  pageSize: 50,
};

function makeStore() {
  return configureStore({
    reducer: {
      authentication: authenticationReducer,
      auditLog: auditLogReducer,
    },
  });
}

function renderPage() {
  const store = makeStore();
  return render(
    <Provider store={store}>
      <AuditTrailPage />
    </Provider>
  );
}

describe('AuditTrailPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    (auditLogApi.searchAuditLogs as ReturnType<typeof vi.fn>).mockResolvedValue(emptyResult);
  });

  it('renders the audit trail table', async () => {
    renderPage();
    await waitFor(() =>
      expect(screen.getByTestId('audit-trail-table')).toBeInTheDocument()
    );
  });

  it('renders filter inputs with correct test IDs', async () => {
    renderPage();
    await waitFor(() => screen.getByTestId('audit-trail-table'));
    expect(screen.getByTestId('audit-filter-user')).toBeInTheDocument();
    expect(screen.getByTestId('audit-filter-from-date')).toBeInTheDocument();
    expect(screen.getByTestId('audit-filter-to-date')).toBeInTheDocument();
    expect(screen.getByTestId('audit-search-btn')).toBeInTheDocument();
  });

  it('calls searchAuditLogs on mount', async () => {
    renderPage();
    await waitFor(() =>
      expect(auditLogApi.searchAuditLogs).toHaveBeenCalledWith(
        expect.objectContaining({ page: 1, pageSize: 50 })
      )
    );
  });

  it('calls searchAuditLogs with userId filter when search is clicked', async () => {
    renderPage();
    await waitFor(() => screen.getByTestId('audit-filter-user'));

    fireEvent.change(screen.getByTestId('audit-filter-user'), {
      target: { value: 'some-user-id' },
    });
    fireEvent.click(screen.getByTestId('audit-search-btn'));

    await waitFor(() =>
      expect(auditLogApi.searchAuditLogs).toHaveBeenLastCalledWith(
        expect.objectContaining({ userId: 'some-user-id' })
      )
    );
  });

  it('displays rows returned by the API', async () => {
    (auditLogApi.searchAuditLogs as ReturnType<typeof vi.fn>).mockResolvedValue(sampleResult);
    renderPage();
    await waitFor(() =>
      expect(screen.getByTestId('row-aaa-111')).toBeInTheDocument()
    );
  });

  it('shows error alert on API failure', async () => {
    (auditLogApi.searchAuditLogs as ReturnType<typeof vi.fn>).mockRejectedValue(
      new Error('Network error')
    );
    renderPage();
    await waitFor(() =>
      expect(screen.getByRole('alert')).toBeInTheDocument()
    );
  });
});
