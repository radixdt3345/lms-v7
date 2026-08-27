import React, { useEffect, useState } from 'react';
import {
  Alert,
  Box,
  Button,
  CircularProgress,
  Paper,
  TextField,
  Typography,
} from '@mui/material';
import { useDispatch } from 'react-redux';
import { getMe, selfEdit, type EmployeeDto } from '../../api/employeeApi';
import { setMe } from '../../store/employeeSlice';
import type { AppDispatch } from '../../store';

/**
 * SCR-024 — Profile & Settings (self-edit)
 *
 * Employees can edit their own name and phone only.
 * Email and role are shown as read-only (FR-17).
 *
 * data-testid map:
 *   input-self-name  — name text field
 *   input-self-phone — phone text field
 *   btn-self-save    — submit self-edit
 */
export default function MyProfilePage() {
  const dispatch = useDispatch<AppDispatch>();
  const [profile, setProfile] = useState<EmployeeDto | null>(null);
  const [name, setName] = useState('');
  const [phone, setPhone] = useState('');
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  useEffect(() => {
    const load = async () => {
      setLoading(true);
      setError(null);
      try {
        const data = await getMe();
        setProfile(data);
        setName(data.name);
        setPhone(data.phone ?? '');
        dispatch(setMe(data));
      } catch {
        setError('Failed to load your profile.');
      } finally {
        setLoading(false);
      }
    };
    void load();
  }, [dispatch]);

  const handleSave = async () => {
    setSaving(true);
    setError(null);
    setSuccess(false);
    try {
      const updated = await selfEdit({ name, phone: phone || null });
      setProfile(updated);
      dispatch(setMe(updated));
      setSuccess(true);
    } catch {
      setError('Failed to save profile changes.');
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return (
      <Box sx={{ p: 3, display: 'flex', justifyContent: 'center' }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box sx={{ p: 3, maxWidth: 600 }}>
      <Typography variant="h5" fontWeight={700} mb={3}>
        My Profile
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}
      {success && (
        <Alert severity="success" sx={{ mb: 2 }} onClose={() => setSuccess(false)}>
          Profile updated successfully.
        </Alert>
      )}

      <Paper elevation={1} sx={{ p: 3, display: 'flex', flexDirection: 'column', gap: 3 }}>
        {/* Read-only fields */}
        <TextField
          label="Email"
          value={profile?.email ?? ''}
          disabled
          fullWidth
          helperText="Email cannot be changed"
        />
        <TextField
          label="Role"
          value={profile?.role ?? ''}
          disabled
          fullWidth
          helperText="Role is assigned by HR Admin"
        />
        {profile?.departmentName && (
          <TextField
            label="Department"
            value={profile.departmentName}
            disabled
            fullWidth
          />
        )}

        {/* Editable fields */}
        <TextField
          label="Name"
          value={name}
          onChange={(e) => setName(e.target.value)}
          fullWidth
          required
          inputProps={{ 'data-testid': 'input-self-name' }}
        />
        <TextField
          label="Phone"
          value={phone}
          onChange={(e) => setPhone(e.target.value)}
          fullWidth
          inputProps={{ 'data-testid': 'input-self-phone' }}
        />

        <Box sx={{ display: 'flex', justifyContent: 'flex-end' }}>
          <Button
            variant="contained"
            data-testid="btn-self-save"
            onClick={() => void handleSave()}
            disabled={saving || !name.trim()}
          >
            {saving ? <CircularProgress size={20} /> : 'Save Changes'}
          </Button>
        </Box>
      </Paper>
    </Box>
  );
}
