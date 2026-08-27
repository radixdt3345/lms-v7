import { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { Box, Typography, Button, Chip, Alert } from '@mui/material';
import { DataGrid, GridColDef } from '@mui/x-data-grid';
import { fetchAllApplications } from '../../store/leaveApplicationSlice';
import { leaveApplicationApi } from '../../api/leaveApplicationApi';
import type { AppDispatch, RootState } from '../../store';

const statusColor = (s: string) => s === 'Approved' ? 'success' : s === 'Rejected' ? 'error' : s === 'Cancelled' ? 'default' : 'warning';

export default function LeaveApplicationManagementPage() {
  const dispatch = useDispatch<AppDispatch>();
  const { applications, loading, error } = useSelector((s: RootState) => s.leaveApplication);

  useEffect(() => { dispatch(fetchAllApplications()); }, [dispatch]);

  const handleApprove = async (id: string) => { await leaveApplicationApi.approve(id); dispatch(fetchAllApplications()); };
  const handleReject = async (id: string) => {
    const reason = window.prompt('Rejection reason:') || 'No reason given';
    await leaveApplicationApi.reject(id, reason);
    dispatch(fetchAllApplications());
  };

  const columns: GridColDef[] = [
    { field: 'employeeName', headerName: 'Employee', flex: 1 },
    { field: 'leaveTypeName', headerName: 'Leave Type', flex: 1 },
    { field: 'startDate', headerName: 'Start', width: 110 },
    { field: 'endDate', headerName: 'End', width: 110 },
    { field: 'totalDays', headerName: 'Days', width: 70 },
    { field: 'status', headerName: 'Status', width: 110, renderCell: (p) => <Chip size="small" label={p.value} color={statusColor(p.value)} /> },
    { field: 'actions', headerName: 'Actions', width: 200, renderCell: (p) => p.row.status === 'Pending' ? (
      <Box>
        <Button size="small" color="success" data-testid={`approve-btn-${p.row.id}`} onClick={() => handleApprove(p.row.id)}>Approve</Button>
        <Button size="small" color="error" data-testid={`reject-btn-${p.row.id}`} onClick={() => handleReject(p.row.id)}>Reject</Button>
      </Box>
    ) : null },
  ];

  return (
    <Box p={3}>
      <Typography variant="h5" gutterBottom data-testid="page-title">Leave Application Management</Typography>
      {error && <Alert severity="error" data-testid="error-message">{error}</Alert>}
      <Box height={600} data-testid="all-applications-grid">
        <DataGrid rows={applications} columns={columns} loading={loading} getRowId={(r) => r.id} />
      </Box>
    </Box>
  );
}
