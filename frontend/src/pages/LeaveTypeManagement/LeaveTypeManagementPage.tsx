import React, { useEffect, useState } from 'react';
import {
  Box, Button, Chip, Dialog, DialogActions, DialogContent, DialogTitle,
  FormControlLabel, Snackbar, Alert, Switch, TextField, Typography,
} from '@mui/material';
import { DataGrid, type GridColDef, type GridRenderCellParams } from '@mui/x-data-grid';
import { useDispatch, useSelector } from 'react-redux';
import type { AppDispatch, RootState } from '../../store';
import {
  fetchLeaveTypes, addLeaveType, editLeaveType, removeLeaveType,
} from '../../store/leaveTypeSlice';
import type { LeaveTypeDto } from '../../api/leaveTypeApi';

interface FormState {
  name: string;
  code: string;
  description: string;
  annualDays: number;
  requiresAttachment: boolean;
  requiresHrApproval: boolean;
}

const emptyForm = (): FormState => ({
  name: '', code: '', description: '', annualDays: 0,
  requiresAttachment: false, requiresHrApproval: false,
});

export default function LeaveTypeManagementPage() {
  const dispatch = useDispatch<AppDispatch>();
  const { items, loading, error } = useSelector((s: RootState) => s.leaveType);

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<LeaveTypeDto | null>(null);
  const [form, setForm] = useState<FormState>(emptyForm());
  const [snackbar, setSnackbar] = useState<{ open: boolean; message: string; severity: 'success' | 'error' }>({
    open: false, message: '', severity: 'success',
  });

  useEffect(() => { dispatch(fetchLeaveTypes()); }, [dispatch]);

  const openCreate = () => { setEditing(null); setForm(emptyForm()); setDialogOpen(true); };
  const openEdit = (row: LeaveTypeDto) => {
    setEditing(row);
    setForm({
      name: row.name, code: row.code, description: row.description ?? '',
      annualDays: row.annualDays, requiresAttachment: row.requiresAttachment,
      requiresHrApproval: row.requiresHrApproval,
    });
    setDialogOpen(true);
  };

  const handleSubmit = async () => {
    try {
      if (editing) {
        await dispatch(editLeaveType({ id: editing.id, req: form })).unwrap();
      } else {
        await dispatch(addLeaveType(form)).unwrap();
      }
      setSnackbar({ open: true, message: editing ? 'Leave type updated' : 'Leave type created', severity: 'success' });
      setDialogOpen(false);
    } catch {
      setSnackbar({ open: true, message: 'Operation failed. Name or code may already exist.', severity: 'error' });
    }
  };

  const handleDeactivate = async (id: string) => {
    try {
      await dispatch(removeLeaveType(id)).unwrap();
      setSnackbar({ open: true, message: 'Leave type deactivated', severity: 'success' });
    } catch {
      setSnackbar({ open: true, message: 'Failed to deactivate', severity: 'error' });
    }
  };

  const columns: GridColDef[] = [
    { field: 'name', headerName: 'Name', flex: 1.5 },
    { field: 'code', headerName: 'Code', width: 80 },
    { field: 'annualDays', headerName: 'Days/Year', width: 100, type: 'number' },
    { field: 'requiresAttachment', headerName: 'Attachment', width: 110, type: 'boolean' },
    { field: 'requiresHrApproval', headerName: 'HR Approval', width: 120, type: 'boolean' },
    {
      field: 'isActive', headerName: 'Status', width: 100,
      renderCell: (p: GridRenderCellParams) => (
        <Chip label={p.value ? 'Active' : 'Inactive'} color={p.value ? 'success' : 'default'} size="small" />
      ),
    },
    {
      field: 'actions', headerName: 'Actions', width: 180, sortable: false,
      renderCell: (p: GridRenderCellParams<LeaveTypeDto>) => (
        <Box sx={{ display: 'flex', gap: 1, alignItems: 'center', height: '100%' }}>
          <Button size="small" onClick={() => openEdit(p.row)}>Edit</Button>
          {p.row.isActive && (
            <Button
              size="small"
              color="error"
              data-testid="deactivate-leave-type-btn"
              onClick={() => handleDeactivate(p.row.id)}
            >
              Deactivate
            </Button>
          )}
        </Box>
      ),
    },
  ];

  return (
    <Box sx={{ p: 3 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
        <Typography variant="h5">Leave Type Management</Typography>
        <Button
          variant="contained"
          data-testid="create-leave-type-btn"
          onClick={openCreate}
        >
          Create Leave Type
        </Button>
      </Box>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      <Box data-testid="leave-type-table" sx={{ height: 500 }}>
        <DataGrid
          rows={items}
          columns={columns}
          loading={loading}
          getRowId={(r) => r.id}
          pageSizeOptions={[10, 25, 50]}
          initialState={{ pagination: { paginationModel: { pageSize: 10 } } }}
        />
      </Box>

      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} fullWidth maxWidth="sm" data-testid="leave-type-dialog">
        <DialogTitle>{editing ? 'Edit Leave Type' : 'Create Leave Type'}</DialogTitle>
        <DialogContent sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: '16px !important' }}>
          <TextField
            label="Name"
            value={form.name}
            onChange={(e) => setForm((f) => ({ ...f, name: e.target.value }))}
            inputProps={{ 'data-testid': 'leave-type-name-input' }}
            fullWidth
            required
          />
          <TextField
            label="Code"
            value={form.code}
            onChange={(e) => setForm((f) => ({ ...f, code: e.target.value }))}
            inputProps={{ 'data-testid': 'leave-type-code-input' }}
            fullWidth
            required
          />
          <TextField
            label="Description"
            value={form.description}
            onChange={(e) => setForm((f) => ({ ...f, description: e.target.value }))}
            fullWidth
            multiline
            rows={2}
          />
          <TextField
            label="Annual Days"
            type="number"
            value={form.annualDays}
            onChange={(e) => setForm((f) => ({ ...f, annualDays: Number(e.target.value) }))}
            inputProps={{ 'data-testid': 'leave-type-annual-days-input', min: 0 }}
            fullWidth
          />
          <FormControlLabel
            control={<Switch checked={form.requiresAttachment} onChange={(e) => setForm((f) => ({ ...f, requiresAttachment: e.target.checked }))} />}
            label="Requires Attachment"
          />
          <FormControlLabel
            control={<Switch checked={form.requiresHrApproval} onChange={(e) => setForm((f) => ({ ...f, requiresHrApproval: e.target.checked }))} />}
            label="Requires HR Approval"
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDialogOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            data-testid="leave-type-submit-btn"
            onClick={handleSubmit}
          >
            {editing ? 'Update' : 'Create'}
          </Button>
        </DialogActions>
      </Dialog>

      <Snackbar
        open={snackbar.open}
        autoHideDuration={4000}
        onClose={() => setSnackbar((s) => ({ ...s, open: false }))}
        data-testid="success-snackbar"
      >
        <Alert severity={snackbar.severity} onClose={() => setSnackbar((s) => ({ ...s, open: false }))}>
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Box>
  );
}
