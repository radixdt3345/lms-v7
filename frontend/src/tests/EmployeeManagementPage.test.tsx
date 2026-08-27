import React from 'react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { Provider } from 'react-redux';
import { MemoryRouter } from 'react-router-dom';
import { configureStore } from '@reduxjs/toolkit';
import authenticationReducer from '../store/authenticationSlice';
import employeeReducer from '../store/employeeSlice';
import EmployeeManagementPage from '../pages/EmployeeManagement/EmployeeManagementPage';
import MyProfilePage from '../pages/EmployeeManagement/MyProfilePage';
import * as employeeApi from '../api/employeeApi';
import type { EmployeeDto, DepartmentDto } from '../api/employeeApi';

// ─── Mocks ───────────────────────────────────────────────────────────────────

vi.mock('../api/employeeApi', () => ({
  listEmployees: vi.fn(),
  getEmployee: vi.fn(),
  createEmployee: vi.fn(),
  updateEmployee: vi.fn(),
  deactivateEmployee: vi.fn(),
  getMe: vi.fn(),
  selfEdit: vi.fn(),
  getTeam: vi.fn(),
  anonymiseEmployee: vi.fn(),
  listDepartments: vi.fn(),
}));

// Mock MUI DataGrid with functional test double
vi.mock('@mui/x-data-grid', () => ({
  DataGrid: ({
    rows,
    loading,
    columns,
  }: {
    rows: EmployeeDto[];
    loading: boolean;
    columns: {
      field: string;
      renderCell?: (p: { row: EmployeeDto }) => React.ReactNode;
    }[];
  }) => {
    if (loading) return <div role="progressbar" data-testid="grid-loading" />;
    if (!rows.length) return <div data-testid="empty-grid">No employees</div>;
    return (
      <div data-testid="grid">
        {rows.map((row) => {
          const actionsCol = columns.find((c) => c.field === 'actions');
          const statusCol = columns.find((c) => c.field === 'status');
          return (
            <div key={row.id} data-testid={`grid-row-${row.id}`}>
              <span>{row.name}</span>
              <span>{row.email}</span>
              {statusCol?.renderCell?.({ row })}
              {actionsCol?.renderCell?.({ row })}
            </div>
          );
        })}
      </div>
    );
  },
}));

// ─── Test data ────────────────────────────────────────────────────────────────

const activeEmployee: EmployeeDto = {
  id: 'emp-001',
  name: 'Alice Smith',
  email: 'alice@example.com',
  phone: '+1-555-0100',
  role: 'EMPLOYEE',
  status: 'Active',
  jobTitle: 'Software Engineer',
  dateOfJoining: '2024-01-15',
  departmentId: 'dept-001',
  departmentName: 'Engineering',
  reportingManagerId: null,
  reportingManagerName: null,
  createdAt: new Date().toISOString(),
  updatedAt: new Date().toISOString(),
};

const department: DepartmentDto = {
  id: 'dept-001',
  name: 'Engineering',
};

// ─── Helpers ─────────────────────────────────────────────────────────────────

function makeStore() {
  return configureStore({
    reducer: {
      authentication: authenticationReducer,
      employee: employeeReducer,
    },
    preloadedState: {
      authentication: {
        accessToken: 'tok.mock.value',
        isAuthenticated: true,
        role: 'HR_ADMIN',
        name: 'HR Admin',
        email: 'admin@example.com',
        isLoading: false,
        error: null,
      },
      employee: { me: null },
    },
  });
}

function renderEmployeeManagementPage() {
  return render(
    <Provider store={makeStore()}>
      <MemoryRouter>
        <EmployeeManagementPage />
      </MemoryRouter>
    </Provider>
  );
}

function renderMyProfilePage() {
  return render(
    <Provider store={makeStore()}>
      <MemoryRouter>
        <MyProfilePage />
      </MemoryRouter>
    </Provider>
  );
}

// ─── Tests ───────────────────────────────────────────────────────────────────

describe('EmployeeManagementPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(employeeApi.listEmployees).mockResolvedValue([activeEmployee]);
    vi.mocked(employeeApi.listDepartments).mockResolvedValue([department]);
  });

  // UT-FE-013: table container renders immediately
  it('UT-FE-013: renders table-employees container on mount', () => {
    vi.mocked(employeeApi.listEmployees).mockReturnValue(new Promise(() => {}));
    vi.mocked(employeeApi.listDepartments).mockReturnValue(new Promise(() => {}));
    renderEmployeeManagementPage();
    expect(screen.getByTestId('table-employees')).toBeInTheDocument();
  });

  // UT-FE-014: btn-create-employee button is visible
  it('UT-FE-014: renders btn-create-employee button', async () => {
    renderEmployeeManagementPage();
    await waitFor(() =>
      expect(screen.getByTestId('btn-create-employee')).toBeInTheDocument()
    );
  });

  // UT-FE-015: populated table renders employee rows
  it('UT-FE-015: renders employee row after data loads', async () => {
    renderEmployeeManagementPage();
    await waitFor(() =>
      expect(screen.getByTestId(`grid-row-${activeEmployee.id}`)).toBeInTheDocument()
    );
    expect(screen.getByText('Alice Smith')).toBeInTheDocument();
    expect(screen.getByText('alice@example.com')).toBeInTheDocument();
  });

  // UT-FE-016: empty state when no employees
  it('UT-FE-016: shows empty-grid when no employees exist', async () => {
    vi.mocked(employeeApi.listEmployees).mockResolvedValue([]);
    renderEmployeeManagementPage();
    await waitFor(() =>
      expect(screen.getByTestId('empty-grid')).toBeInTheDocument()
    );
  });

  // UT-FE-017: deactivate button present per row
  it('UT-FE-017: renders btn-deactivate-employee for each active employee', async () => {
    renderEmployeeManagementPage();
    await waitFor(() =>
      expect(screen.getByTestId('btn-deactivate-employee')).toBeInTheDocument()
    );
  });

  // UT-FE-018: search filters the employee list
  it('UT-FE-018: search input filters employees by name', async () => {
    const secondEmployee: EmployeeDto = {
      ...activeEmployee,
      id: 'emp-002',
      name: 'Bob Jones',
      email: 'bob@example.com',
    };
    vi.mocked(employeeApi.listEmployees).mockResolvedValue([
      activeEmployee,
      secondEmployee,
    ]);
    renderEmployeeManagementPage();
    await waitFor(() =>
      expect(screen.getByTestId(`grid-row-${activeEmployee.id}`)).toBeInTheDocument()
    );
    const searchInput = screen.getByTestId('input-employee-search');
    fireEvent.change(searchInput, { target: { value: 'Bob' } });
    await waitFor(() =>
      expect(screen.queryByTestId(`grid-row-${activeEmployee.id}`)).not.toBeInTheDocument()
    );
    expect(screen.getByTestId(`grid-row-${secondEmployee.id}`)).toBeInTheDocument();
  });

  // UT-FE-019: create dialog opens with btn-save-employee
  it('UT-FE-019: opens create dialog on btn-create-employee click', async () => {
    renderEmployeeManagementPage();
    await waitFor(() =>
      expect(screen.getByTestId('btn-create-employee')).toBeInTheDocument()
    );
    fireEvent.click(screen.getByTestId('btn-create-employee'));
    await waitFor(() =>
      expect(screen.getByTestId('btn-save-employee')).toBeInTheDocument()
    );
    expect(screen.getByTestId('select-department')).toBeInTheDocument();
    expect(screen.getByTestId('select-manager')).toBeInTheDocument();
  });

  // UT-FE-020: fetch error shows error alert
  it('UT-FE-020: shows error when listEmployees rejects', async () => {
    vi.mocked(employeeApi.listEmployees).mockRejectedValue(new Error('network error'));
    renderEmployeeManagementPage();
    await waitFor(() =>
      expect(screen.getByText(/Failed to load employees/)).toBeInTheDocument()
    );
  });
});

describe('MyProfilePage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(employeeApi.getMe).mockResolvedValue(activeEmployee);
  });

  // UT-FE-021: profile page renders name and phone inputs
  it('UT-FE-021: renders input-self-name and input-self-phone after load', async () => {
    renderMyProfilePage();
    await waitFor(() =>
      expect(screen.getByTestId('input-self-name')).toBeInTheDocument()
    );
    expect(screen.getByTestId('input-self-phone')).toBeInTheDocument();
    expect(screen.getByTestId('btn-self-save')).toBeInTheDocument();
  });

  // UT-FE-022: name and phone pre-populated from API
  it('UT-FE-022: pre-populates name and phone from getMe response', async () => {
    renderMyProfilePage();
    await waitFor(() => {
      const nameInput = screen.getByTestId('input-self-name') as HTMLInputElement;
      expect(nameInput.value).toBe('Alice Smith');
    });
    const phoneInput = screen.getByTestId('input-self-phone') as HTMLInputElement;
    expect(phoneInput.value).toBe('+1-555-0100');
  });

  // UT-FE-023: save calls selfEdit with updated values
  it('UT-FE-023: clicking btn-self-save calls selfEdit with updated name', async () => {
    const updatedEmployee = { ...activeEmployee, name: 'Alice Updated' };
    vi.mocked(employeeApi.selfEdit).mockResolvedValue(updatedEmployee);
    renderMyProfilePage();
    await waitFor(() =>
      expect(screen.getByTestId('input-self-name')).toBeInTheDocument()
    );
    const nameInput = screen.getByTestId('input-self-name');
    fireEvent.change(nameInput, { target: { value: 'Alice Updated' } });
    fireEvent.click(screen.getByTestId('btn-self-save'));
    await waitFor(() =>
      expect(employeeApi.selfEdit).toHaveBeenCalledWith({
        name: 'Alice Updated',
        phone: '+1-555-0100',
      })
    );
  });

  // UT-FE-024: getMe failure shows error
  it('UT-FE-024: shows error when getMe rejects', async () => {
    vi.mocked(employeeApi.getMe).mockRejectedValue(new Error('network'));
    renderMyProfilePage();
    await waitFor(() =>
      expect(screen.getByText(/Failed to load your profile/)).toBeInTheDocument()
    );
  });
});
