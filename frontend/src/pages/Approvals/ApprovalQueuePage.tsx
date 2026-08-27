import React, { useEffect } from "react";
import { Box, Typography, Alert, Chip, CircularProgress } from "@mui/material";
import { DataGrid, GridColDef } from "@mui/x-data-grid";
import { useAppDispatch, useAppSelector } from "../../store/hooks";
import { fetchPendingApprovalsThunk } from "../../store/approvalSlice";

const ApprovalQueuePage: React.FC = () => {
  const dispatch = useAppDispatch();
  const { pending, loading, error } = useAppSelector((s) => s.approvals);

  useEffect(() => { dispatch(fetchPendingApprovalsThunk()); }, [dispatch]);

  const columns: GridColDef[] = [
    { field: "employeeName", headerName: "Employee", flex: 1 },
    { field: "entityType", headerName: "Type", width: 180,
      renderCell: (p) => <Chip label={p.value} size="small" color={p.value === "LeaveApplication" ? "primary" : "secondary"} /> },
    { field: "description", headerName: "Details", flex: 2 },
    { field: "submittedAt", headerName: "Submitted", width: 180,
      valueFormatter: (p) => new Date(p.value).toLocaleDateString() },
    { field: "status", headerName: "Status", width: 120,
      renderCell: (p) => <Chip label={p.value} size="small" color="warning" /> },
  ];

  return (
    <Box p={3}>
      <Typography variant="h5" gutterBottom data-testid="page-title">Approval Queue</Typography>
      {loading && <CircularProgress data-testid="loading-spinner" />}
      {error && <Alert severity="error" data-testid="error-message">{error}</Alert>}
      {!loading && !error && pending.length === 0 && (
        <Alert severity="info" data-testid="empty-state">No pending approvals.</Alert>
      )}
      {!loading && pending.length > 0 && (
        <DataGrid
          data-testid="approval-queue-grid"
          rows={pending}
          columns={columns}
          autoHeight
          getRowId={(r) => r.id}
          pageSizeOptions={[10, 25]}
        />
      )}
    </Box>
  );
};
export default ApprovalQueuePage;
