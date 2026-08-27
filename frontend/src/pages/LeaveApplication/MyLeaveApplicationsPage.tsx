import { useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { Box, Typography, Button, Chip, CircularProgress, Alert, Dialog, DialogTitle, DialogContent, DialogActions, TextField, MenuItem, Select, FormControl, InputLabel } from '@mui/material';
import { DataGrid, GridColDef } from '@mui/x-data-grid';
import { fetchMyApplications } from '../../store/leaveApplicationSlice';
import { leaveApplicationApi, SubmitLeaveApplicationRequest } from '../../api/leaveApplicationApi';
import type { AppDispatch, RootState } from '../../store';

const statusColor = (s: string) => s === 'Approved' ? 'success' : s === 'Rejected' ? 'error' : s === 'Cancelled' ? 'default' : 'warning';

export default function MyLeaveApplicationsPage() {
  const dispatch = useDispatch<AppDispatch>();
  const { applications, loading, error } = useSelector((s: RootState) => s.leaveApplication);
  const [open, setOpen] = useState(false);
  const [form, setForm] = useState<SubmitLeaveApplicationRequest>({ leaveTypeId: '', startDate: '', endDate: '', reason: '' });

  useEffect(() => { dispatch(fetchMyApplications()); }, [dispatch]);

  const handleSubmit = async () => {
    await leaveApplicationApi.submit(form);
    setOpen(false);
    dispatch(fetchMyApplications());
  };

  const columns: GridColDef[] = [
    { field: 'leaveTypeName', headerName: 'Leave Type', flex: 1 },
    { field: 'startDate', headerName: 'Start', width: 110 },
    { field: 'endDate', headerName: 'End', width: 110 },
    { field: 'totalDays', headerName: 'Days', width: 70 },
    { field: 'status', headerName: 'Status', width: 110, renderCell: (p) => <Chip size="small" label={p.value} color={statusColor(p.value)} /> },
    { field: 'reason', headerName: 'Reason', flex: 1 },
  ];

  return (
    <Box p={3}>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
        <Typography variant="h5" data-testid="page-title">My Leave Applications</Typography>
        <Button variant="contained" data-testid="apply-leave-btn" onClick={() => setOpen(true)}>Apply Leave</Button>
      </Box>
      {error && <Alert severity="error" data-testid="error-message">{error}</Alert>}
      <Box height={500} data-testid="applications-grid">
        <DataGrid rows={applications} columns={columns} loading={loading} getRowId={(r) => r.id} />
      </Box>
      <Dialog open={open} onClose={() => setOpen(false)} data-testid="apply-leave-dialog">
        <DialogTitle>Apply for Leave</DialogTitle>
        <DialogContent sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 2 }}>
          <TextField data-testid="start-date-input" label="Start Date" type="date" value={form.startDate} onChange={(e) => setForm(f => ({ ...f, startDate: e.target.value }))} InputLabelProps={{ shrink: true }} />
          <TextField data-testid="end-date-input" label="End Date" type="date" value={form.endDate} onChange={(e) => setForm(f => ({ ...f, endDate: e.target.value }))} InputLabelProps={{ shrink: true }} />
          <TextField data-testid="reason-input" label="Reason" multiline rows={3} value={form.reason} onChange={(e) => setForm(f => ({ ...f, reason: e.target.value }))} />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpen(false)} data-testid="cancel-dialog-btn">Cancel</Button>
          <Button variant="contained" onClick={handleSubmit} data-testid="submit-application-btn">Submit</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
