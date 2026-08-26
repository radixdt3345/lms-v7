import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { Provider } from 'react-redux';
import { configureStore } from '@reduxjs/toolkit';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import DepartmentManagementPage from '../pages/DepartmentManagement/DepartmentManagementPage';
import authenticationReducer from '../store/authenticationSlice';
import { departmentReducer } from '../store/departmentSlice';
import * as departmentApi from '../api/departmentApi';

// 1. Mock ALL API modules used by the component
vi.mock('../api/departmentApi', () => ({
  getDepartments: vi.fn(),
  getDepartmentById: vi.fn(),
  createDepartment: vi.fn(),
  updateDepartment: vi.fn(),
  deactivateDepartment: vi.fn(),
}));

// 3. Mock @mui/x-data-grid DataGrid
vi.mock('@mui/x-data-grid', () => ({
  DataGrid: ({
    rows,
    loading,
    columns,
  }: {
    rows: Array<Record<string, unknown>>;
    loading: boolean;
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    columns: Array<{ field: string; renderCell?: (params: any) => React.ReactNode }>;
  }) => {
    if (loading) return <div role="progressbar" />;
    if (!rows.length) return <div data-testid="empty-grid">No rows</div>;
    return (
      <div data-testid="grid">
        {rows.map((row) => {
          const actionsCol = columns.find((c) => c.field === 'actions');
          return (
            <div key={String(row['id'])} data-testid={`grid-row-${String(row['id'])}`}>
              {actionsCol?.renderCell?.({ row, value: undefined })}
            </div>
          );
        })}
      </div>
    );
  },
}));

// ─── Fixtures ────────────────────────────────────────────────────────────────

const defaultDepts: departmentApi.DepartmentDto[] = [
  {
    id: '1',
    name: 'Engineering',
    code: 'ENG',
    overlapLimit: 2,
    status: 'Active',
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-01T00:00:00Z',
  },
  {
    id: '2',
    name: 'Human Resources',
    code: 'HR',
    overlapLimit: 1,
    status: 'Active',
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-01T00:00:00Z',
  },
];

// ─── Store factory ────────────────────────────────────────────────────────────

interface DeptStateOverride {
  departments?: departmentApi.DepartmentDto[];
  loading?: boolean;
  error?: string | null;
  duplicateError?: boolean;
}

function makeStore(deptOverride: DeptStateOverride = {}) {
  return configureStore({
    reducer: {
      authentication: authenticationReducer,
      department: departmentReducer,
    },
    preloadedState: {
      authentication: {
        accessToken: 'mock-token',
        isAuthenticated: true,
        role: 'HR_ADMIN',
        name: 'Admin User',
        email: 'admin@test.com',
        isLoading: false,
        error: null,
      },
      department: {
        departments: deptOverride.departments ?? defaultDepts,
        loading: deptOverride.loading ?? false,
        error: deptOverride.error ?? null,
        duplicateError: deptOverride.duplicateError ?? false,
      },
    },
  });
}

function renderPage(store = makeStore()) {
  return render(
    <Provider store={store}>
      <DepartmentManagementPage />
    </Provider>
  );
}

// ─── Tests ────────────────────────────────────────────────────────────────────

describe('DepartmentManagementPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(departmentApi.getDepartments).mockResolvedValue(defaultDepts);
    vi.mocked(departmentApi.createDepartment).mockResolvedValue(defaultDepts[0]);
    vi.mocked(departmentApi.updateDepartment).mockResolvedValue(defaultDepts[0]);
    vi.mocked(departmentApi.deactivateDepartment).mockResolvedValue(undefined);
  });

  // UT-FE-031: Render — table and create button present
  it('UT-FE-031: renders department table and create button', () => {
    renderPage();
    expect(screen.getByTestId('table-departments')).toBeInTheDocument();
    expect(screen.getByTestId('btn-create-department')).toBeInTheDocument();
  });

  // UT-FE-032: Loading — shows progressbar
  it('UT-FE-032: shows progressbar when loading is true', () => {
    vi.mocked(departmentApi.getDepartments).mockReturnValue(new Promise(() => {}));
    const store = makeStore({ departments: [], loading: true });
    renderPage(store);
    expect(screen.getByRole('progressbar')).toBeInTheDocument();
  });

  // UT-FE-033: Empty — no departments
  it('UT-FE-033: shows empty grid when no departments exist', () => {
    vi.mocked(departmentApi.getDepartments).mockResolvedValue([]);
    const store = makeStore({ departments: [] });
    renderPage(store);
    expect(screen.getByTestId('empty-grid')).toBeInTheDocument();
  });

  // UT-FE-034: Error — renders error message without crash
  it('UT-FE-034: renders error message when fetch fails', async () => {
    vi.mocked(departmentApi.getDepartments).mockRejectedValue(new Error('Network error'));
    const store = makeStore({ departments: [], error: 'Failed to load departments' });
    renderPage(store);
    expect(screen.getByTestId('btn-create-department')).toBeInTheDocument();
    expect(screen.getByText(/Failed to load departments/i)).toBeInTheDocument();
  });

  // UT-FE-035: Happy path — create dialog opens with all fields
  it('UT-FE-035: opens create department dialog with all required fields', async () => {
    renderPage();
    fireEvent.click(screen.getByTestId('btn-create-department'));
    await waitFor(() => {
      expect(screen.getByTestId('input-dept-name')).toBeInTheDocument();
      expect(screen.getByTestId('input-dept-code')).toBeInTheDocument();
      expect(screen.getByTestId('input-overlap-limit')).toBeInTheDocument();
      expect(screen.getByTestId('btn-save-department')).toBeInTheDocument();
    });
  });

  // UT-FE-036: Deactivation warning — appears on deactivate click
  it('UT-FE-036: shows deactivation warning alert when deactivate button clicked', async () => {
    renderPage();
    const deactivateBtns = screen.getAllByTestId('btn-deactivate-department');
    fireEvent.click(deactivateBtns[0]);
    await waitFor(() => {
      expect(screen.getByTestId('alert-dept-deactivation-warning')).toBeInTheDocument();
    });
  });

  // UT-FE-037: Failure path — duplicate error stored in slice on 409
  it('UT-FE-037: sets duplicateError on 409 from createDepartment', async () => {
    vi.mocked(departmentApi.createDepartment).mockRejectedValue({
      response: { status: 409 },
    });
    const store = makeStore();
    renderPage(store);

    fireEvent.click(screen.getByTestId('btn-create-department'));
    await waitFor(() => {
      expect(screen.getByTestId('input-dept-name')).toBeInTheDocument();
    });

    fireEvent.change(screen.getByTestId('input-dept-name'), {
      target: { value: 'Engineering' },
    });
    fireEvent.change(screen.getByTestId('input-dept-code'), {
      target: { value: 'ENG' },
    });
    fireEvent.click(screen.getByTestId('btn-save-department'));

    await waitFor(() => {
      expect(store.getState().department.duplicateError).toBe(true);
    });
  });

  // UT-FE-038: Deactivation confirm — warning disappears and API called
  it('UT-FE-038: deactivation warning disappears after confirming and API is called', async () => {
    renderPage();
    const deactivateBtns = screen.getAllByTestId('btn-deactivate-department');
    fireEvent.click(deactivateBtns[0]);

    await waitFor(() => {
      expect(screen.getByTestId('alert-dept-deactivation-warning')).toBeInTheDocument();
    });

    fireEvent.click(screen.getByText('Confirm'));

    await waitFor(() => {
      expect(screen.queryByTestId('alert-dept-deactivation-warning')).not.toBeInTheDocument();
    });

    expect(departmentApi.deactivateDepartment).toHaveBeenCalledWith('1');
  });
});
