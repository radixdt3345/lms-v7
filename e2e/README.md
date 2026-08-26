# LMS V7 E2E Tests

Playwright tests for LMS V7. Trigger: `workflow_dispatch` only.

| ID | Test |
|---|---|
| E2E-001 | SSO login button initiates Azure AD redirect |
| E2E-002 | Valid credentials log in and redirect to dashboard |
| E2E-003 | Invalid credentials show error alert |
| E2E-004 | 3 failed logins lock account; 4th attempt shows locked error |
| E2E-005 | HR Admin sees locked-accounts table on dashboard |
| E2E-006 | HR Admin unlocks account and row is removed from table |
| E2E-007 | Logout redirects to /login and blocks protected route access |

## Run

```bash
BASE_URL=https://staging.example.com npx playwright test e2e/auth/authentication.spec.ts
```
