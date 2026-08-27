import { useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { Box, Typography, Button, CircularProgress, Alert, Select, MenuItem, FormControl, InputLabel } from '@mui/material';
import { DataGrid, GridColDef } from '@mui/x-data-grid';
import { fetchAllBalances } from '../../store/leaveBalanceSlice';
import { leaveBalanceApi } from '../../api/leaveBalanceApi';
import type { AppDispatch, RootState } from '../../store';

export default function LeaveBalanceManagementPage() {
  const dispatch = useDispatch<AppDispatch>();
  const { balances, loading, error } = useSelector((s: RootState) => s.leaveBalance);
  const [year, setYear] = useState(new Date().getFullYear());
  const [crediting, setCrediting] = useState(false);

  useEffect(() => { dispatch(fetchAllBalances(year)); }, [dispatch, year]);

  const handleCreditAnnual = async () => {
    setCrediting(true);
    await leaveBalanceApi.creditAnnual(year);
    dispatch(fetchAllBalances(year));
    setCrediting(false);
  };

  const columns: GridColDef[] = [
    { field: 'employeeName', headerName: 'Employee', flex: 1 },
    { field: 'leaveTypeName', headerName: 'Leave Type', flex: 1 },
    { field: 'year', headerName: 'Year', width: 80 },
    { field: 'totalDays', headerName: 'Total', width: 80 },
    { field: 'usedDays', headerName: 'Used', width: 80 },
    { field: 'pendingDays', headerName: 'Pending', width: 90 },
    { field: 'remainingDays', headerName: 'Remaining', width: 100 },
  ];

  return (
    <Box p={3}>
      <Typography variant="h5" gutterBottom data-testid="page-title">Leave Balance Management</Typography>
      <Box display="flex" gap={2} mb={2} alignItems="center">
        <FormControl size="small" sx={{ minWidth: 120 }}>
          <InputLabel>Year</InputLabel>
          <Select data-testid="year-select" value={year} label="Year" onChange={(e) => setYear(Number(e.target.value))}>
            {[2023, 2024, 2025, 2026].map(y => <MenuItem key={y} value={y}>{y}</MenuItem>)}
          </Select>
        </FormControl>
        <Button data-testid="credit-annual-btn" variant="contained" onClick={handleCreditAnnual} disabled={crediting}>
          {crediting ? 'Crediting...' : 'Credit Annual Balances'}
        </Button>
      </Box>
      {error && <Alert severity="error" data-testid="error-message">{error}</Alert>}
      <Box height={500} data-testid="balances-grid">
        <DataGrid rows={balances} columns={columns} loading={loading} getRowId={(r) => r.id} />
      </Box>
    </Box>
  );
}
