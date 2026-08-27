# F15 Background Jobs — Integration Smoke Test

## Feature: Background Jobs & Scheduled Tasks

### IT-056: Expire comp-off credits job runs successfully
1. POST /api/v1/jobs/expire-comp-off (HR_ADMIN token)
2. Expect 200, body: { data: string message }
3. CompOffCredits with ExpiryDate < today marked as Expired

### IT-057: Reset leave balances job creates balances for new year
1. POST /api/v1/jobs/reset-leave-balances?year=2027
2. Expect 200, body: { data: string message }
3. LeaveBalances created for all active employees and leave types for 2027

### IT-058: Send reminders job runs successfully
1. POST /api/v1/jobs/send-reminders
2. Expect 200, body: { data: string message with pending count }

### IT-059: Job logs are recorded and retrievable
1. Run any job
2. GET /api/v1/jobs/logs
3. Expect 200, body: { data: JobLog[] }
4. Most recent log entry matches the job just run

### IT-060: Non-admin cannot trigger jobs
1. Authenticate as EMPLOYEE
2. POST /api/v1/jobs/expire-comp-off
3. Expect 403 Forbidden
