# F-10 Public Holiday Management — Integration Layer Smoke Test

**Date:** 2026-08-27
**Method:** Code-inspection smoke test

## Scope
Verify DB + API + UI layers are correctly wired.

## Results
### 1. DB → API
- PublicHolidayService accepts LmsDbContext
- LmsDbContext has DbSet<PublicHoliday>
- Migration 20260827110000_CreatePublicHolidaysTable creates public_holidays table

### 2. API → Controller
- PublicHolidaysController at /api/v1/holidays
- All 6 endpoints registered
- Returns ApiResponse<T> envelope

### 3. Frontend
- publicHolidayApi.ts reads response.data.data
- PublicHolidayManagementPage at /admin/holidays

### 4. DI
- IPublicHolidayService registered in Program.cs

## Verdict: PASS (code-inspection)
