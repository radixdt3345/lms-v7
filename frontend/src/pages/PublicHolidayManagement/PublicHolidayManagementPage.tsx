import React, { useEffect, useState } from 'react';
import { Box, Button, Typography, Paper, Table, TableHead, TableRow, TableCell, TableBody, IconButton, Dialog, DialogTitle, DialogContent, DialogActions, TextField, CircularProgress } from '@mui/material';
import DeleteIcon from '@mui/icons-material/Delete';
import EditIcon from '@mui/icons-material/Edit';
import { listHolidays, createHoliday, updateHoliday, deleteHoliday, PublicHolidayDto } from '../../api/publicHolidayApi';

const PublicHolidayManagementPage: React.FC = () => {
  const [holidays, setHolidays] = useState<PublicHolidayDto[]>([]);
  const [year, setYear] = useState(new Date().getFullYear());
  const [loading, setLoading] = useState(false);
  const [openDialog, setOpenDialog] = useState(false);
  const [editHoliday, setEditHoliday] = useState<PublicHolidayDto | null>(null);
  const [form, setForm] = useState({ date: '', name: '' });

  const fetchHolidays = async () => {
    setLoading(true);
    try {
      const data = await listHolidays(year);
      setHolidays(data);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchHolidays(); }, [year]);

  const handleSave = async () => {
    if (editHoliday) {
      await updateHoliday(editHoliday.id, form);
    } else {
      await createHoliday(form);
    }
    setOpenDialog(false);
    setEditHoliday(null);
    setForm({ date: '', name: '' });
    fetchHolidays();
  };

  const handleDelete = async (id: string) => {
    await deleteHoliday(id);
    fetchHolidays();
  };

  const handleEdit = (h: PublicHolidayDto) => {
    setEditHoliday(h);
    setForm({ date: h.date, name: h.name });
    setOpenDialog(true);
  };

  return (
    <Box p={3}>
      <Typography variant="h4" gutterBottom data-testid="page-title">Public Holiday Management</Typography>
      <Box display="flex" gap={2} mb={2}>
        <TextField
          label="Year"
          type="number"
          value={year}
          onChange={e => setYear(Number(e.target.value))}
          size="small"
          data-testid="year-filter"
        />
        <Button variant="contained" data-testid="add-holiday-btn" onClick={() => { setEditHoliday(null); setForm({ date: '', name: '' }); setOpenDialog(true); }}>
          Add Holiday
        </Button>
      </Box>
      {loading ? (
        <CircularProgress data-testid="loading-spinner" />
      ) : (
        <Paper>
          <Table data-testid="holidays-table">
            <TableHead>
              <TableRow>
                <TableCell>Date</TableCell>
                <TableCell>Name</TableCell>
                <TableCell>Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {holidays.length === 0 ? (
                <TableRow><TableCell colSpan={3} data-testid="no-holidays">No holidays for {year}</TableCell></TableRow>
              ) : holidays.map(h => (
                <TableRow key={h.id} data-testid={`holiday-row-${h.id}`}>
                  <TableCell>{h.date}</TableCell>
                  <TableCell>{h.name}</TableCell>
                  <TableCell>
                    <IconButton data-testid={`edit-holiday-${h.id}`} onClick={() => handleEdit(h)}><EditIcon /></IconButton>
                    <IconButton data-testid={`delete-holiday-${h.id}`} onClick={() => handleDelete(h.id)}><DeleteIcon /></IconButton>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </Paper>
      )}
      <Dialog open={openDialog} onClose={() => setOpenDialog(false)} data-testid="holiday-dialog">
        <DialogTitle>{editHoliday ? 'Edit Holiday' : 'Add Holiday'}</DialogTitle>
        <DialogContent>
          <TextField fullWidth label="Date" type="date" value={form.date} onChange={e => setForm(f => ({ ...f, date: e.target.value }))} sx={{ mt: 1 }} InputLabelProps={{ shrink: true }} data-testid="holiday-date-input" />
          <TextField fullWidth label="Name" value={form.name} onChange={e => setForm(f => ({ ...f, name: e.target.value }))} sx={{ mt: 2 }} data-testid="holiday-name-input" />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpenDialog(false)} data-testid="cancel-btn">Cancel</Button>
          <Button variant="contained" onClick={handleSave} data-testid="save-holiday-btn">Save</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default PublicHolidayManagementPage;
