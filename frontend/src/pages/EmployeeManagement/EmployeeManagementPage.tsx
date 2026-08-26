import React, { useEffect, useState } from 'react';
import {
  Alert,
  Box,
  Button,
  Chip,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControl,
  InputLabel,
  MenuItem,
  Paper,
  Select,
  TextField,
  Typography,
} from '@mui/material';
import { DataGrid, type GridColDef, type GridRenderCellParams } from '@mui/x-data-grid';
import {
  listEmployees,
  createEmployee,
  updateEmployee,
  deactivateEmployee,
  listDepartments,
  type EmployeeDto,
  type DepartmentDto,
} from '../../api/employeeApi';

/**
 * SCR-015 — Employee List
 * SCR-016 — Employee Create / Edit
 *
 * data-testid map:
 *   table-employees        — employee data grid container
 *   btn-create-employee    — open create dialog
 *   input-employee-search  — search filter field
 *   select-department      — department dropdown in form
 *   select-manager         — reporting manager dropdown in form
 *   btn-save-employee      — submit create/edit form
 *   btn-deactivate-employee — deactivate a row
 */

interface FormState {
  name: string;
  email: string;
  phone: string;
  jobTitle: string;
  dateOfJoining: string;
  departmentId: string;
  reportingManagerId: string;
}

const emptyForm = (): FormState => ({
  name: '',
  email: '',
  phone: '',
  jobTitle: '',
  dateOfJoining: '',
  departmentId: '',
  reportingManagerId: '',
});

export default function EmployeeManagementPage() {
  const [rows, setRows] = useState<EmployeeDto[]>([]);
  const [departments, setDepartments] = useState<DepartmentDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [search, setSearch] = useState('');
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [form, setForm] = useState<FormState>(emptyForm());
  const [saving, setSaving] = useState(false);
  const [deactivating, setDeactivating] = useState<string | null>(null);

  const fetchAll = async () => {
    setLoading(true);
    setError(null);
    try {
      const [employees, depts] = await Promise.all([
        listEmployees(),
        listDepartments().catch(() => [] as DepartmentDto[]),
      ]);
      setRows(employees);
      setDepartments(depts);
    } catch {
      setError('Failed to load employees.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void fetchAll();
  }, []);

  const filteredRows = rows.filter(
    (r) =>
      r.name.toLowerCase().includes(search.toLowerCase()) ||
      r.email.toLowerCase().includes(search.toLowerCase())
  );

  const openCreate = () => {
    setEditingId(null);
    setForm(emptyForm());
    setDialogOpen(true);
  };

  const openEdit = (employee: EmployeeDto) => {
    setEditingId(employee.id);
    setForm({
      name: employee.name,
      email: employee.email,
      phone: employee.phone ?? '',
      jobTitle: employee.jobTitle ?? '',
      dateOfJoining: employee.dateOfJoining ?? '',
      departmentId: employee.departmentId ?? '',
      reportingManagerId: employee.reportingManagerId ?? '',
    });
    setDialogOpen(true);
  };

  const handleSave = async () => {
    setSaving(true);
    try {
      if (editingId) {
        await updateEmployee(editingId, {
          name: form.name,
          phone: form.phone || null,
          jobTitle: form.jobTitle || null,
          dateOfJoining: form.dateOfJoining || null,
          departmentId: form.departmentId || null,
          clearDepartment: !form.departmentId,
          reportingManagerId: form.reportingManagerId || null,
          clearReportingManager: !form.reportingManagerId,
        });
      } else {
        await createEmployee({
          name: form.name,
          email: form.email,
          phone: form.phone || null,
          jobTitle: form.jobTitle || null,
          dateOfJoining: form.dateOfJoining || null,
          departmentId: form.departmentId || null,
          reportingManagerId: form.reportingManagerId || null,
        });
      }
      setDialogOpen(false);
      await fetchAll();
    } catch {
      setError(editingId ? 'Failed to update employee.' : 'Failed to create employee.');
    } finally {
      setSaving(false);
    }
  };

  const handleDeactivate = async (id: string) => {
    setDeactivating(id);
    try {
      await deactivateEmployee(id);
      await fetchAll();
    } catch {
      setError('Failed to deactivate employee.');
    } finally {
      setDeactivating(null);
    }
  };

  const managerOptions = rows.filter(
    (r) => (r.role === 'MANAGER' || r.role === 'HR_ADMIN') && r.status === 'Active'
  );

  const columns: GridColDef[] = [
    { field: 'name', headerName: 'Name', flex: 1, minWidth: 140 },
    { field: 'email', headerName: 'Email', flex: 1.5, minWidth: 200 },
    { field: 'role', headerName: 'Role', width: 130 },
    {
      field: 'status',
      headerName: 'Status',
      width: 120,
      renderCell: (params: GridRenderCellParams<EmployeeDto>) => (
        <Chip
          label={params.row.status}
          color={params.row.status === 'Active' ? 'success' : 'default'}
          size="small"
        />
      ),
    },
    { field: 'departmentName', headerName: 'Department', flex: 1, minWidth: 140 },
    { field: 'reportingManagerName', headerName: 'Manager', flex: 1, minWidth: 140 },
    {
      field: 'actions',
      headerName: 'Actions',
      width: 200,
      sortable: false,
      filterable: false,
      renderCell: (params: GridRenderCellParams<EmployeeDto>) => (
        <Box sx={{ display: 'flex', gap: 1 }}>
          <Button
            size="small"
            variant="outlined"
            onClick={() => openEdit(params.row)}
          >
            Edit
          </Button>
          <Button
            size="small"
            variant="contained"
            color="error"
            data-testid="btn-deactivate-employee"
            disabled={
              params.row.status === 'Inactive' || deactivating === params.row.id
            }
            onClick={() => void handleDeactivate(params.row.id)}
            aria-label={`Deactivate ${params.row.name}`}
          >
            {deactivating === params.row.id ? (
              <CircularProgress size={16} color="inherit" />
            ) : (
              'Deactivate'
            )}
          </Button>
        </Box>
      ),
    },
  ];

  return (
    <Box sx={{ p: 3 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
        <Typography variant="h5" fontWeight={700}>
          Employee Management
        </Typography>
        <Button
          variant="contained"
          color="primary"
          data-testid="btn-create-employee"
          onClick={openCreate}
        >
          Add Employee
        </Button>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      <TextField
        label="Search employees"
        value={search}
        onChange={(e) => setSearch(e.target.value)}
        size="small"
        sx={{ mb: 2, width: 320 }}
        inputProps={{ 'data-testid': 'input-employee-search' }}
      />

      <Paper elevation={1}>
        <Box data-testid="table-employees" sx={{ height: 520, width: '100%' }}>
          <DataGrid
            rows={filteredRows}
            columns={columns}
            loading={loading}
            getRowId={(r) => r.id}
            pageSizeOptions={[10, 25, 50]}
            initialState={{ pagination: { paginationModel: { pageSize: 10 } } }}
            disableRowSelectionOnClick
          />
        </Box>
      </Paper>

      {/* Create / Edit Dialog */}
      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{editingId ? 'Edit Employee' : 'Add Employee'}</DialogTitle>
        <DialogContent sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 2 }}>
          <TextField
            label="Name"
            value={form.name}
            onChange={(e) => setForm((f) => ({ ...f, name: e.target.value }))}
            fullWidth
            required
          />
          {!editingId && (
            <TextField
              label="Email"
              type="email"
              value={form.email}
              onChange={(e) => setForm((f) => ({ ...f, email: e.target.value }))}
              fullWidth
              required
            />
          )}
          <TextField
            label="Phone"
            value={form.phone}
            onChange={(e) => setForm((f) => ({ ...f, phone: e.target.value }))}
            fullWidth
          />
          <TextField
            label="Job Title"
            value={form.jobTitle}
            onChange={(e) => setForm((f) => ({ ...f, jobTitle: e.target.value }))}
            fullWidth
          />
          <TextField
            label="Date of Joining"
            type="date"
            value={form.dateOfJoining}
            onChange={(e) => setForm((f) => ({ ...f, dateOfJoining: e.target.value }))}
            fullWidth
            InputLabelProps={{ shrink: true }}
          />

          {/* Department dropdown — FR-12: dropdown-only, no free text */}
          <FormControl fullWidth>
            <InputLabel id="dept-label">Department</InputLabel>
            <Select
              labelId="dept-label"
              label="Department"
              value={form.departmentId}
              onChange={(e) =>
                setForm((f) => ({ ...f, departmentId: e.target.value }))
              }
              inputProps={{ 'data-testid': 'select-department' }}
            >
              <MenuItem value="">
                <em>None</em>
              </MenuItem>
              {departments.map((d) => (
                <MenuItem key={d.id} value={d.id}>
                  {d.name}
                </MenuItem>
              ))}
            </Select>
          </FormControl>

          {/* Reporting Manager dropdown — FR-12: dropdown-only */}
          <FormControl fullWidth>
            <InputLabel id="mgr-label">Reporting Manager</InputLabel>
            <Select
              labelId="mgr-label"
              label="Reporting Manager"
              value={form.reportingManagerId}
              onChange={(e) =>
                setForm((f) => ({ ...f, reportingManagerId: e.target.value }))
              }
              inputProps={{ 'data-testid': 'select-manager' }}
            >
              <MenuItem value="">
                <em>None (HR Admin handles approvals)</em>
              </MenuItem>
              {managerOptions.map((m) => (
                <MenuItem key={m.id} value={m.id}>
                  {m.name} ({m.role})
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDialogOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            data-testid="btn-save-employee"
            onClick={() => void handleSave()}
            disabled={saving || !form.name || (!editingId && !form.email)}
          >
            {saving ? <CircularProgress size={20} /> : 'Save'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
