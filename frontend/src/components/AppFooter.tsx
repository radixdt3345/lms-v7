import { Box, Typography, Link } from '@mui/material';

export default function AppFooter() {
  return (
    <Box
      component="footer"
      data-testid="app-footer"
      sx={{ py: 2, px: 3, mt: 'auto', backgroundColor: 'background.paper', borderTop: 1, borderColor: 'divider' }}
    >
      <Typography variant="body2" color="text.secondary" align="center" data-testid="footer-version">
        LMS v1.0.0 &mdash; {new Date().getFullYear()} &mdash;{' '}
        <Link href="/api/v1/system/health" underline="hover" data-testid="footer-health-link">
          System Health
        </Link>
      </Typography>
    </Box>
  );
}
