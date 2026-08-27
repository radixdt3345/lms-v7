import React, { useState } from "react";
import { Box, Typography, Button, Grid, Card, CardContent, CardActions, CircularProgress, Alert } from "@mui/material";
import DownloadIcon from "@mui/icons-material/Download";
import { downloadLeaveReport, downloadCompOffReport, downloadLeaveBalanceReport } from "../../api/reportApi";

const saveBlob = (blob: Blob, filename: string) => {
  const url = URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = filename;
  a.click();
  URL.revokeObjectURL(url);
};

const ReportsPage: React.FC = () => {
  const [loading, setLoading] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const year = new Date().getFullYear();

  const handle = async (type: string, fn: () => Promise<Blob>, filename: string) => {
    setLoading(type);
    setError(null);
    try {
      const blob = await fn();
      saveBlob(blob, filename);
    } catch {
      setError("Failed to download report. Please try again.");
    } finally {
      setLoading(null);
    }
  };

  const reports = [
    {
      key: "leave",
      title: "Leave Report",
      desc: "Download all leave applications for the current year as CSV.",
      testid: "download-leave-report",
      fn: () => downloadLeaveReport(year),
      filename: `leave-report-${year}.csv`,
    },
    {
      key: "compoff",
      title: "Comp-Off Report",
      desc: "Download all comp-off requests for the current year as CSV.",
      testid: "download-compoff-report",
      fn: () => downloadCompOffReport(year),
      filename: `compoff-report-${year}.csv`,
    },
    {
      key: "balance",
      title: "Leave Balance Report",
      desc: "Download leave balances for all employees as CSV.",
      testid: "download-balance-report",
      fn: () => downloadLeaveBalanceReport(year),
      filename: `leave-balance-report-${year}.csv`,
    },
  ];

  return (
    <Box p={3}>
      <Typography variant="h5" data-testid="page-title" gutterBottom>
        Reports &amp; Analytics
      </Typography>
      {error && <Alert severity="error" data-testid="error-message" sx={{ mb: 2 }}>{error}</Alert>}
      <Grid container spacing={3}>
        {reports.map((r) => (
          <Grid item xs={12} md={4} key={r.key}>
            <Card>
              <CardContent>
                <Typography variant="h6">{r.title}</Typography>
                <Typography variant="body2" color="text.secondary">{r.desc}</Typography>
              </CardContent>
              <CardActions>
                <Button
                  variant="contained"
                  startIcon={loading === r.key ? <CircularProgress size={16} /> : <DownloadIcon />}
                  disabled={loading !== null}
                  data-testid={r.testid}
                  onClick={() => handle(r.key, r.fn, r.filename)}
                >
                  Download CSV
                </Button>
              </CardActions>
            </Card>
          </Grid>
        ))}
      </Grid>
    </Box>
  );
};

export default ReportsPage;
