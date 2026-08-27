# F12 Reports & Analytics — Integration Smoke Test

## Feature: Reports & Analytics

### Pre-conditions
- Authenticated as HR_ADMIN
- Leave applications and comp-off requests exist in DB

### IT-052: Leave report downloads as CSV
1. GET /api/v1/reports/leave
2. Expect 200, Content-Type: text/csv
3. Body contains CSV headers: EmployeeName,Email,LeaveType,StartDate,EndDate,TotalDays,Status,Reason

### IT-053: Comp-Off report downloads as CSV
1. GET /api/v1/reports/comp-off
2. Expect 200, Content-Type: text/csv
3. Body contains CSV headers: EmployeeName,Email,WorkedDate,CreditDays,Status,Reason

### IT-054: Leave Balance report downloads as CSV
1. GET /api/v1/reports/leave-balances
2. Expect 200, Content-Type: text/csv
3. Body contains CSV headers: EmployeeName,Email,LeaveType,Year,TotalDays,UsedDays,PendingDays,AvailableDays

### IT-055: Non-HR user cannot access reports
1. Authenticate as EMPLOYEE
2. GET /api/v1/reports/leave
3. Expect 403 Forbidden
