import { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { Box, Typography, Button, Chip, Alert } from '@mui/material';
import { DataGrid, GridColDef } from '@mui/x-data-grid';
import { fetchMyCompOffRequests } from '../../store/compOffSlice';
import { compOffApi } from '../../api/compOffApi';
import type { AppDispatch, RootState } from '../../store';

export default function MyCompOffPage() {
  const dispatch = useDispatch<AppDispatch>();
  const { requests, loading, error } = useSelector((s: RootState) => s.compOff);
  useEffect(() => { dispatch(fetchMyCompOffRequests()); }, [dispatch]);

  const handleSubmit = async () => {
    const workedDate = window.prompt('Worked date (YYYY-MM-DD):') || '';
    if (!workedDate) return;
    await compOffApi.submit({ workedDate, creditDays: 1, reason: 'Worked on holiday' });
    dispatch(fetchMyCompOffRequests());
  };

  const columns: GridColDef[] = [
    { field: 'workedDate', headerName: 'Worked Date', width: 130 },
    { field: 'creditDays', headerName: 'Days', width: 70 },
    { field: 'reason', headerName: 'Reason', flex: 1 },
    { field: 'status', headerName: 'Status', width: 110, renderCell: (p) => <Chip size="small" label={p.value} color={p.value === 'Approved' ? 'success' : p.value === 'Rejected' ? 'error' : 'warning'} /> },
  ];

  return (
    <Box p={3}>
      <Box display="flex" justifyContent="space-between" mb={2}>
        <Typography variant="h5" data-testid="page-title">My Comp-Off Requests</Typography>
        <Button variant="contained" data-testid="request-comp-off-btn" onClick={handleSubmit}>Request Comp-Off</Button>
      </Box>
      {error && <Alert severity="error" data-testid="error-message">{error}</Alert>}
      <Box height={400} data-testid="comp-off-grid">
        <DataGrid rows={requests} columns={columns} loading={loading} getRowId={(r) => r.id} />
      </Box>
    </Box>
  );
}
