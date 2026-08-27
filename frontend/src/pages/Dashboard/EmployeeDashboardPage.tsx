import React, { useEffect } from "react";
import { Box, Typography, Grid, Card, CardContent, Alert, CircularProgress, Chip } from "@mui/material";
import { useAppDispatch, useAppSelector } from "../../store/hooks";
import { fetchEmployeeDashboardThunk } from "../../store/dashboardSlice";

const StatCard: React.FC<{ label: string; value: number | string; color?: string; testId: string }> = ({ label, value, color, testId }) => (
  <Card data-testid={testId}>
    <CardContent>
      <Typography variant="h4" color={color ?? "primary"}>{value}</Typography>
      <Typography variant="body2" color="text.secondary">{label}</Typography>
    </CardContent>
  </Card>
);

const EmployeeDashboardPage: React.FC = () => {
  const dispatch = useAppDispatch();
  const { employee, loading, error } = useAppSelector((s) => s.dashboard);
  useEffect(() => { dispatch(fetchEmployeeDashboardThunk()); }, [dispatch]);

  return (
    <Box p={3}>
      <Typography variant="h5" gutterBottom data-testid="page-title">My Dashboard</Typography>
      {loading && <CircularProgress data-testid="loading-spinner" />}
      {error && <Alert severity="error" data-testid="error-message">{error}</Alert>}
      {employee && (
        <Grid container spacing={2}>
          <Grid item xs={6} md={3}><StatCard label="Pending Leaves" value={employee.pendingLeaves} testId="stat-pending-leaves" /></Grid>
          <Grid item xs={6} md={3}><StatCard label="Approved Leaves" value={employee.approvedLeaves} color="success.main" testId="stat-approved-leaves" /></Grid>
          <Grid item xs={6} md={3}><StatCard label="Pending Comp-Off" value={employee.pendingCompOff} testId="stat-pending-compoff" /></Grid>
          <Grid item xs={6} md={3}><StatCard label="Leave Balance" value={employee.totalLeaveBalance} color="info.main" testId="stat-leave-balance" /></Grid>
          <Grid item xs={12}>
            <Typography variant="h6" mt={2}>Leave Balances</Typography>
            <Box display="flex" gap={1} flexWrap="wrap" data-testid="leave-balances-list">
              {employee.leaveBalances.map((b, i) => (
                <Chip key={i} label={} variant="outlined" />
              ))}
            </Box>
          </Grid>
        </Grid>
      )}
    </Box>
  );
};
export default EmployeeDashboardPage;
