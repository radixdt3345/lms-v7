# F06 — Leave Application Integration Smoke Test

## Verified Flows
1. POST /api/v1/leave-applications — employee submits, returns LeaveApplicationDto with status=Pending
2. GET /api/v1/leave-applications/me — employee sees own applications
3. PUT /api/v1/leave-applications/{id}/approve — HR admin approves, status→Approved
4. PUT /api/v1/leave-applications/{id}/reject — HR admin rejects with reason
5. DELETE /api/v1/leave-applications/{id}/cancel — employee cancels own pending application
6. GET /api/v1/leave-applications?status=Pending — HR admin views all pending

All responses wrapped in `{ "data": ... }`.
## Status: PASS
