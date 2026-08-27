import React, { useEffect, useState } from "react";
import { Box, Typography, Button, Grid, Card, CardContent, CardActions, CircularProgress, Alert, Chip, Table, TableBody, TableCell, TableHead, TableRow, Paper } from "@mui/material";
import PlayArrowIcon from "@mui/icons-material/PlayArrow";
import { triggerExpireCompOff, triggerResetLeaveBalances, triggerSendReminders, fetchJobLogs, JobLog } from "../../api/jobApi";

const BackgroundJobsPage: React.FC = () => {
  const [running, setRunning] = useState<string | null>(null);
  const [result, setResult] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [logs, setLogs] = useState<JobLog[]>([]);
  const [logsLoading, setLogsLoading] = useState(true);

  const loadLogs = async () => {
    try {
      const data = await fetchJobLogs();
      setLogs(data);
    } catch { /* ignore */ } finally {
      setLogsLoading(false);
    }
  };

  useEffect(() => { loadLogs(); }, []);

  const run = async (name: string, fn: () => Promise<string>) => {
    setRunning(name);
    setResult(null);
    setError(null);
    try {
      const msg = await fn();
      setResult(msg);
      loadLogs();
    } catch {
      setError("Job failed. Please try again.");
    } finally {
      setRunning(null);
    }
  };

  const jobs = [
    { key: "expire", label: "Expire Comp-Off Credits", testid: "trigger-expire-compoff", fn: triggerExpireCompOff },
    { key: "reset", label: "Reset Leave Balances", testid: "trigger-reset-balances", fn: () => triggerResetLeaveBalances() },
    { key: "remind", label: "Send Leave Reminders", testid: "trigger-send-reminders", fn: triggerSendReminders },
  ];

  return (
    <Box p={3}>
      <Typography variant="h5" data-testid="page-title" gutterBottom>Background Jobs</Typography>
      {result && <Alert severity="success" data-testid="success-message" sx={{ mb: 2 }}>{result}</Alert>}
      {error && <Alert severity="error" data-testid="error-message" sx={{ mb: 2 }}>{error}</Alert>}
      <Grid container spacing={2} mb={3}>
        {jobs.map((j) => (
          <Grid item xs={12} md={4} key={j.key}>
            <Card>
              <CardContent>
                <Typography variant="subtitle1">{j.label}</Typography>
              </CardContent>
              <CardActions>
                <Button
                  variant="contained"
                  size="small"
                  startIcon={running === j.key ? <CircularProgress size={14} /> : <PlayArrowIcon />}
                  disabled={running !== null}
                  data-testid={j.testid}
                  onClick={() => run(j.key, j.fn)}
                >
                  Run Now
                </Button>
              </CardActions>
            </Card>
          </Grid>
        ))}
      </Grid>
      <Typography variant="h6" gutterBottom>Recent Job Logs</Typography>
      {logsLoading ? (
        <CircularProgress data-testid="logs-loading" />
      ) : (
        <Paper data-testid="job-logs-table">
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Job</TableCell>
                <TableCell>Status</TableCell>
                <TableCell>Details</TableCell>
                <TableCell>Started</TableCell>
                <TableCell>Completed</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {logs.map((l) => (
                <TableRow key={l.id}>
                  <TableCell>{l.jobName}</TableCell>
                  <TableCell><Chip label={l.status} color={l.status === "Success" ? "success" : l.status === "Failed" ? "error" : "default"} size="small" /></TableCell>
                  <TableCell>{l.details}</TableCell>
                  <TableCell>{new Date(l.startedAt).toLocaleString()}</TableCell>
                  <TableCell>{l.completedAt ? new Date(l.completedAt).toLocaleString() : "-"}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </Paper>
      )}
    </Box>
  );
};

export default BackgroundJobsPage;
