import React, { useEffect, useState } from 'react';
import {
  Alert,
  Box,
  Paper,
  Typography,
} from '@mui/material';
import { DataGrid, type GridColDef } from '@mui/x-data-grid';
import { getTeam, type EmployeeDto } from '../../api/employeeApi';

/**
 * My Team — direct reports visible to Managers (FR-18).
 * Uses the same table-employees testid for E2E consistency.
 */
export default function MyTeamPage() {
  const [rows, setRows] = useState<EmployeeDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const load = async () => {
      setLoading(true);
      setError(null);
      try {
        const data = await getTeam();
        setRows(data);
      } catch {
        setError('Failed to load team members.');
      } finally {
        setLoading(false);
      }
    };
    void load();
  }, []);

  const columns: GridColDef[] = [
    { field: 'name', headerName: 'Name', flex: 1, minWidth: 140 },
    { field: 'email', headerName: 'Email', flex: 1.5, minWidth: 200 },
    { field: 'role', headerName: 'Role', width: 130 },
    { field: 'jobTitle', headerName: 'Job Title', flex: 1, minWidth: 140 },
    { field: 'departmentName', headerName: 'Department', flex: 1, minWidth: 140 },
  ];

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h5" fontWeight={700} mb={2}>
        My Team
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      <Paper elevation={1}>
        <Box data-testid="table-employees" sx={{ height: 500, width: '100%' }}>
          <DataGrid
            rows={rows}
            columns={columns}
            loading={loading}
            getRowId={(r) => r.id}
            pageSizeOptions={[10, 25, 50]}
            initialState={{ pagination: { paginationModel: { pageSize: 10 } } }}
            disableRowSelectionOnClick
          />
        </Box>
      </Paper>
    </Box>
  );
}
