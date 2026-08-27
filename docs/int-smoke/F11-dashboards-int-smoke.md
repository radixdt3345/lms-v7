# F11 Dashboard Integration Smoke Test
GET /api/v1/dashboard/employee → ApiResponse<EmployeeDashboardDto> ✓
GET /api/v1/dashboard/hr (HR Admin only) → ApiResponse<HrDashboardDto> ✓
UI reads response.data.data (axios + ApiResponse envelope) ✓
