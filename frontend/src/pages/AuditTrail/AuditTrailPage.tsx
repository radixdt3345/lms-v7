import React, { useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import {
  Box,
  Button,
  Paper,
  TextField,
  Typography,
  MenuItem,
  Select,
  FormControl,
  InputLabel,
  Stack,
  Alert,
} from '@mui/material';
import { DataGrid } from '@mui/x-data-grid';
import type { GridColDef } from '@mui/x-data-grid';
import type { AppDispatch, RootState } from '../../store';
import { fetchAuditLogs } from '../../store/auditLogSlice';
import type { AuditLogDto } from '../../api/auditLogApi';

const ACTION_TYPES = ['CREATE', 'UPDATE', 'DELETE', 'LOGIN', 'LOGOUT', 'PASSWORD_RESET'];
const RECORD_TYPES = ['User', 'Employee', 'Department', 'LeaveRequest', 'LeaveType'];

const columns: GridColDef<AuditLogDto>[] = [
  { field: 'timestamp', headerName: 'Timestamp', width: 180,
    valueFormatter: (v: string) => v ? new Date(v).toLocaleString() : '' },
  { field: 'actorName', headerName: 'Actor', width: 200 },
  { field: 'actionType', headerName: 'Action', width: 140 },
  { field: 'recordType', headerName: 'Record Type', width: 140 },
  { field: 'recordId', headerName: 'Record ID', width: 280 },
  { field: 'newValue', headerName: 'Changes', flex: 1 },
];

export default function AuditTrailPage() {
  const dispatch = useDispatch<AppDispatch>();
  const { result, loading, error } = useSelector((s: RootState) => s.auditLog);

  const [userId, setUserId] = useState('');
  const [actionType, setActionType] = useState('');
  const [recordType, setRecordType] = useState('');
  const [fromDate, setFromDate] = useState('');
  const [toDate, setToDate] = useState('');
  const [page, setPage] = useState(0);

  const PAGE_SIZE = 50;

  const doSearch = (newPage = 0) => {
    setPage(newPage);
    dispatch(fetchAuditLogs({
      userId: userId || undefined,
      actionType: actionType || undefined,
      recordType: recordType || undefined,
      fromDate: fromDate || undefined,
      toDate: toDate || undefined,
      page: newPage + 1,
      pageSize: PAGE_SIZE,
    }));
  };

  useEffect(() => { doSearch(0); }, []); // eslint-disable-line react-hooks/exhaustive-deps

  const rows = result?.items ?? [];
  const totalCount = result?.totalCount ?? 0;

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h5" gutterBottom>Audit Trail</Typography>

      <Paper sx={{ p: 2, mb: 2 }}>
        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} flexWrap="wrap">
          <TextField
            label="User ID"
            size="small"
            value={userId}
            onChange={(e) => setUserId(e.target.value)}
            inputProps={{ 'data-testid': 'audit-filter-user' }}
            sx={{ minWidth: 200 }}
          />

          <FormControl size="small" sx={{ minWidth: 160 }}>
            <InputLabel>Action Type</InputLabel>
            <Select
              label="Action Type"
              value={actionType}
              onChange={(e) => setActionType(e.target.value)}
              inputProps={{ 'data-testid': 'audit-filter-action' }}
            >
              <MenuItem value="">All</MenuItem>
              {ACTION_TYPES.map((a) => <MenuItem key={a} value={a}>{a}</MenuItem>)}
            </Select>
          </FormControl>

          <FormControl size="small" sx={{ minWidth: 160 }}>
            <InputLabel>Record Type</InputLabel>
            <Select
              label="Record Type"
              value={recordType}
              onChange={(e) => setRecordType(e.target.value)}
              inputProps={{ 'data-testid': 'audit-filter-record' }}
            >
              <MenuItem value="">All</MenuItem>
              {RECORD_TYPES.map((r) => <MenuItem key={r} value={r}>{r}</MenuItem>)}
            </Select>
          </FormControl>

          <TextField
            label="From Date"
            type="date"
            size="small"
            InputLabelProps={{ shrink: true }}
            value={fromDate}
            onChange={(e) => setFromDate(e.target.value)}
            inputProps={{ 'data-testid': 'audit-filter-from-date' }}
          />

          <TextField
            label="To Date"
            type="date"
            size="small"
            InputLabelProps={{ shrink: true }}
            value={toDate}
            onChange={(e) => setToDate(e.target.value)}
            inputProps={{ 'data-testid': 'audit-filter-to-date' }}
          />

          <Button
            variant="contained"
            onClick={() => doSearch(0)}
            data-testid="audit-search-btn"
          >
            Search
          </Button>
        </Stack>
      </Paper>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      <Paper sx={{ height: 600 }}>
        <DataGrid
          data-testid="audit-trail-table"
          rows={rows}
          columns={columns}
          loading={loading}
          rowCount={totalCount}
          paginationMode="server"
          paginationModel={{ page, pageSize: PAGE_SIZE }}
          onPaginationModelChange={(m) => doSearch(m.page)}
          pageSizeOptions={[PAGE_SIZE]}
          disableRowSelectionOnClick
          getRowId={(row) => row.id}
          sx={{ border: 0 }}
        />
      </Paper>
    </Box>
  );
}
