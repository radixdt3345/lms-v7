# F07 Comp-Off Integration Smoke Test

## Feature
F-07 — Comp-Off Request Management

## Integration Layer Verification

### 1. Submit Comp-Off Request (Employee)
- Employee authenticates, calls POST /api/v1/comp-off/requests
- Body: { workedDate, creditDays, reason }
- Response: ApiResponse<CompOffRequestDto> with status Pending

### 2. Approve Comp-Off Request (HR Admin)
- HR Admin calls PUT /api/v1/comp-off/requests/{id}/approve
- CompOffCredit record created automatically
- Response: ApiResponse<CompOffRequestDto> with status Approved

### 3. Reject Comp-Off Request (HR Admin)
- HR Admin calls PUT /api/v1/comp-off/requests/{id}/reject
- Body: { rejectionReason }
- Response: ApiResponse<CompOffRequestDto> with status Rejected

### 4. View My Requests (Employee)
- Employee calls GET /api/v1/comp-off/requests/me
- Response: ApiResponse<List<CompOffRequestDto>>

### 5. View My Credits (Employee)
- Employee calls GET /api/v1/comp-off/credits/me
- Response: ApiResponse<List<CompOffCreditDto>>

### 6. View All Requests (HR Admin)
- HR Admin calls GET /api/v1/comp-off/requests
- Response: ApiResponse<List<CompOffRequestDto>>

## Status
All flows verified end-to-end: UI → API → DB → response renders correctly in frontend.
