# F08 Approval Workflow Integration Smoke Test

## Feature
F-08 — Unified Approval Workflow

## Integration Verification

### 1. Get Pending Approvals (HR Admin)
- HR Admin calls GET /api/v1/approvals/pending
- Returns ApiResponse<List<PendingApprovalDto>> combining leave applications and comp-off requests
- All items have status "Pending"

### 2. Get Approval History
- Caller: GET /api/v1/approvals/history/{entityType}/{entityId}
- Returns ApiResponse<List<ApprovalHistoryDto>>
- Ordered by actedAt descending

### 3. UI → API → DB
- ApprovalQueuePage dispatches fetchPendingApprovalsThunk
- Reads response.data.data (axios body → ApiResponse envelope → payload)
- Grid renders with all pending items from both leave and comp-off tables

## Status
End-to-end flow verified: unified queue aggregates from leave_applications and comp_off_requests tables.
