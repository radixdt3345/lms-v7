import { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { Box, Typography, Button, Chip, Alert } from '@mui/material';
import { DataGrid, GridColDef } from '@mui/x-data-grid';
import { fetchAllCompOffRequests } from '../../store/compOffSlice';
import { compOffApi } from '../../api/compOffApi';
import type { AppDispatch, RootState } from '../../store';

export default function CompOffManagementPage() {
  const dispatch = useDispatch<AppDispatch>();
  const { requests, loading, error } = useSelector((s: RootState) => s.compOff);
  useEffect(() => { dispatch(fetchAllCompOffRequests()); }, [dispatch]);
  const handleApprove = async (id: string) => { await compOffApi.approve(id); dispatch(fetchAllCompOffRequests()); };
  const handleReject = async (id: string) => { const r = window.prompt('Reason:') || 'No reason'; await compOffApi.reject(id, r); dispatch(fetchAllCompOffRequests()); };
  const columns: GridColDef[] = [
    { field: 'employeeName', headerName: 'Employee', flex: 1 },
    { field: 'workedDate', headerName: 'Worked Date', width: 130 },
    { field: 'creditDays', headerName: 'Days', width: 70 },
    { field: 'status', headerName: 'Status', width: 110, renderCell: (p) => <Chip size="small" label={p.value} color={p.value === 'Approved' ? 'success' : p.value === 'Rejected' ? 'error' : 'warning'} /> },
    { field: 'actions', headerName: 'Actions', width: 180, renderCell: (p) => p.row.status === 'Pending' ? <Box><Button size="small" color="success" data-testid={`approve-btn-${p.row.id}`} onClick={() => handleApprove(p.row.id)}>Approve</Button><Button size="small" color="error" data-testid={`reject-btn-${p.row.id}`} onClick={() => handleReject(p.row.id)}>Reject</Button></Box> : null },
  ];
  return (
    <Box p={3}>
      <Typography variant="h5" gutterBottom data-testid="page-title">Comp-Off Management</Typography>
      {error && <Alert severity="error" data-testid="error-message">{error}</Alert>}
      <Box height={500} data-testid="comp-off-management-grid"><DataGrid rows={requests} columns={columns} loading={loading} getRowId={(r) => r.id} /></Box>
    </Box>
  );
}
