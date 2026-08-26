import { useState, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import {
  Box,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Alert,
  Typography,
} from '@mui/material';
import { DataGrid, type GridColDef } from '@mui/x-data-grid';
import type { AppDispatch, RootState } from '../../store';
import {
  fetchDepartments,
  createDepartmentAsync,
  updateDepartmentAsync,
  deactivateDepartmentAsync,
  clearDepartmentError,
} from '../../store/departmentSlice';
import type { DepartmentDto } from '../../api/departmentApi';

export default function DepartmentManagementPage() {
  const dispatch = useDispatch<AppDispatch>();
  const { departments, loading, error, duplicateError } = useSelector(
    (s: RootState) => s.department
  );

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editTarget, setEditTarget] = useState<DepartmentDto | null>(null);
  const [name, setName] = useState('');
  const [code, setCode] = useState('');
  const [overlapLimit, setOverlapLimit] = useState(2);
  const [deactivateTarget, setDeactivateTarget] = useState<DepartmentDto | null>(null);

  useEffect(() => {
    dispatch(fetchDepartments());
  }, [dispatch]);

  const openCreate = () => {
    setEditTarget(null);
    setName('');
    setCode('');
    setOverlapLimit(2);
    dispatch(clearDepartmentError());
    setDialogOpen(true);
  };

  const openEdit = (dept: DepartmentDto) => {
    setEditTarget(dept);
    setName(dept.name);
    setCode(dept.code);
    setOverlapLimit(dept.overlapLimit);
    dispatch(clearDepartmentError());
    setDialogOpen(true);
  };

  const handleSave = async () => {
    if (editTarget) {
      const result = await dispatch(
        updateDepartmentAsync({ id: editTarget.id, data: { name, code, overlapLimit } })
      );
      if (updateDepartmentAsync.fulfilled.match(result)) setDialogOpen(false);
    } else {
      const result = await dispatch(createDepartmentAsync({ name, code, overlapLimit }));
      if (createDepartmentAsync.fulfilled.match(result)) setDialogOpen(false);
    }
  };

  const handleConfirmDeactivate = async () => {
    if (!deactivateTarget) return;
    const result = await dispatch(deactivateDepartmentAsync(deactivateTarget.id));
    if (deactivateDepartmentAsync.fulfilled.match(result)) {
      setDeactivateTarget(null);
    }
  };

  const columns: GridColDef[] = [
    { field: 'name', headerName: 'Name', flex: 1 },
    { field: 'code', headerName: 'Code', width: 120 },
    { field: 'overlapLimit', headerName: 'Overlap Limit', width: 140 },
    { field: 'status', headerName: 'Status', width: 120 },
    {
      field: 'actions',
      headerName: 'Actions',
      width: 220,
      sortable: false,
      renderCell: ({ row }: { row: DepartmentDto }) => (
        <Box sx={{ display: 'flex', gap: 1, alignItems: 'center', height: '100%' }}>
          <Button size="small" variant="outlined" onClick={() => openEdit(row)}>
            Edit
          </Button>
          {row.status === 'Active' && (
            <Button
              size="small"
              variant="outlined"
              color="warning"
              data-testid="btn-deactivate-department"
              onClick={() => setDeactivateTarget(row)}
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
      <Typography variant="h5" mb={2}>
        Department Management
      </Typography>

      <Button
        variant="contained"
        data-testid="btn-create-department"
        onClick={openCreate}
        sx={{ mb: 2 }}
      >
        Create Department
      </Button>

      {deactivateTarget && (
        <Alert
          severity="warning"
          data-testid="alert-dept-deactivation-warning"
          sx={{ mb: 2 }}
          action={
            <Box sx={{ display: 'flex', gap: 1 }}>
              <Button
                size="small"
                color="inherit"
                variant="outlined"
                onClick={handleConfirmDeactivate}
              >
                Confirm
              </Button>
              <Button size="small" color="inherit" onClick={() => setDeactivateTarget(null)}>
                Cancel
              </Button>
            </Box>
          }
        >
          Deactivating <strong>{deactivateTarget.name}</strong> will prevent new leave requests
          for employees in this department. Existing approved leave will not be affected.
        </Alert>
      )}

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      <Box data-testid="table-departments">
        <DataGrid
          rows={departments}
          columns={columns}
          loading={loading}
          autoHeight
          pageSizeOptions={[10, 25, 50]}
          initialState={{ pagination: { paginationModel: { pageSize: 10 } } }}
        />
      </Box>

      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{editTarget ? 'Edit Department' : 'Create Department'}</DialogTitle>
        <DialogContent
          sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: '16px !important' }}
        >
          {duplicateError && (
            <Alert severity="error">
              A department with this name or code already exists.
            </Alert>
          )}
          <TextField
            label="Department Name"
            value={name}
            onChange={(e) => setName(e.target.value)}
            inputProps={{ 'data-testid': 'input-dept-name' }}
            fullWidth
            autoFocus
          />
          <TextField
            label="Department Code"
            value={code}
            onChange={(e) => setCode(e.target.value.toUpperCase())}
            inputProps={{ 'data-testid': 'input-dept-code' }}
            fullWidth
          />
          <TextField
            label="Overlap Limit"
            type="number"
            value={overlapLimit}
            onChange={(e) => setOverlapLimit(Math.max(1, Number(e.target.value)))}
            inputProps={{ 'data-testid': 'input-overlap-limit', min: 1 }}
            fullWidth
            helperText="Maximum concurrent leave requests allowed in this department"
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDialogOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            data-testid="btn-save-department"
            onClick={handleSave}
            disabled={loading || !name.trim() || !code.trim()}
          >
            Save
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
