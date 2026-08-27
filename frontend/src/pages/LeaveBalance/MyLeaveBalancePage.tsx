import { useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { Box, Typography, Card, CardContent, Grid, CircularProgress, Alert, Select, MenuItem, FormControl, InputLabel } from '@mui/material';
import { fetchMyBalances } from '../../store/leaveBalanceSlice';
import type { AppDispatch, RootState } from '../../store';

export default function MyLeaveBalancePage() {
  const dispatch = useDispatch<AppDispatch>();
  const { balances, loading, error } = useSelector((s: RootState) => s.leaveBalance);
  const [year, setYear] = useState(new Date().getFullYear());

  useEffect(() => { dispatch(fetchMyBalances(year)); }, [dispatch, year]);

  return (
    <Box p={3}>
      <Typography variant="h5" gutterBottom data-testid="page-title">My Leave Balances</Typography>
      <FormControl size="small" sx={{ mb: 2, minWidth: 120 }}>
        <InputLabel>Year</InputLabel>
        <Select data-testid="year-select" value={year} label="Year" onChange={(e) => setYear(Number(e.target.value))}>
          {[2023, 2024, 2025, 2026].map(y => <MenuItem key={y} value={y}>{y}</MenuItem>)}
        </Select>
      </FormControl>
      {loading && <CircularProgress data-testid="loading-spinner" />}
      {error && <Alert severity="error" data-testid="error-message">{error}</Alert>}
      {!loading && !error && balances.length === 0 && (
        <Typography data-testid="empty-state">No leave balances found for {year}.</Typography>
      )}
      <Grid container spacing={2}>
        {balances.map((b) => (
          <Grid item xs={12} sm={6} md={4} key={b.id}>
            <Card data-testid={`balance-card-${b.leaveTypeId}`}>
              <CardContent>
                <Typography variant="h6">{b.leaveTypeName}</Typography>
                <Typography>Total: {b.totalDays} days</Typography>
                <Typography>Used: {b.usedDays} days</Typography>
                <Typography>Pending: {b.pendingDays} days</Typography>
                <Typography color="primary">Remaining: {b.remainingDays} days</Typography>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>
    </Box>
  );
}
