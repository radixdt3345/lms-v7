import React, { useEffect, useState } from 'react';
import {
  Alert,
  Box,
  Button,
  CircularProgress,
  Paper,
  Typography,
} from '@mui/material';
import { DataGrid, type GridColDef, type GridRenderCellParams } from '@mui/x-data-grid';
import {
  getLockedAccounts,
  unlockAccount,
  type LockedUserDto,
} from '../../api/authenticationApi';

/**
 * SCR-021 - User Management / Locked Accounts
 * HR Admin / Super Admin can view and unlock locked user accounts.
 * data-testid map: table-locked-accounts, btn-unlock-account
 */
export default function UserManagementPage() {
  const [rows, setRows] = useState<LockedUserDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [unlocking, setUnlocking] = useState<string | null>(null);

  const fetchLocked = async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await getLockedAccounts();
      setRows(data);
    } catch {
      setError('Failed to load locked accounts.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void fetchLocked();
  }, []);

  const handleUnlock = async (userId: string) => {
    setUnlocking(userId);
    try {
      await unlockAccount(userId);
      await fetchLocked();
    } catch {
      setError('Failed to unlock account.');
    } finally {
      setUnlocking(null);
    }
  };

  const columns: GridColDef[] = [
    { field: 'name', headerName: 'Name', flex: 1, minWidth: 140 },
    { field: 'email', headerName: 'Email', flex: 1.5, minWidth: 200 },
    { field: 'role', headerName: 'Role', width: 140 },
    { field: 'failedAttempts', headerName: 'Failed Attempts', width: 150 },
    {
      field: 'lockedAt',
      headerName: 'Locked At',
      flex: 1,
      minWidth: 180,
      valueFormatter: (value: string) => new Date(value).toLocaleString(),
    },
    {
      field: 'actions',
      headerName: 'Actions',
      width: 130,
      sortable: false,
      filterable: false,
      renderCell: (params: GridRenderCellParams<LockedUserDto>) => (
        <Button
          size="small"
          variant="contained"
          color="success"
          data-testid="btn-unlock-account"
          disabled={unlocking === params.row.id}
          onClick={() => void handleUnlock(params.row.id)}
          aria-label={`Unlock ${params.row.name}`}
        >
          {unlocking === params.row.id ? (
            <CircularProgress size={16} color="inherit" />
          ) : (
            'Unlock'
          )}
        </Button>
      ),
    },
  ];

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h5" fontWeight={700} mb={2}>
        User Management - Locked Accounts
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      <Paper elevation={1}>
        <Box data-testid="table-locked-accounts" sx={{ height: 500, width: '100%' }}>
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
