# F-04 Leave Type & Policy Management — Integration Smoke Test

> **Type:** Code-inspection smoke test
> **Feature:** F-04 Leave Type & Policy Management
> **Date:** 2026-08-27

This document verifies the DB → API → UI wiring for F-04 by inspecting
the committed source files rather than running the application.

---

## 1. DB Layer (Issue #33)

| Check | File | Verdict |
|---|---|---|
| `LeaveType` entity exists with all required fields | `backend/src/LMS.Infrastructure/Data/Entities/LeaveType.cs` | PASS |
| Migration `20260827100000_CreateLeaveTypesTable` creates `leave_types` table with correct columns | `backend/src/LMS.Infrastructure/Migrations/20260827100000_CreateLeaveTypesTable.cs` | PASS |
| Unique index on `code` column | Migration file, `ix_leave_types_code` | PASS |
| Unique index on `name` column | Migration file, `ix_leave_types_name` | PASS |
| 5 default leave types seeded (CL, SL, EL, CO, UL) | Migration `InsertData` / SQL seed block | PASS |
| `DbSet<LeaveType> LeaveTypes` added to `LmsDbContext` | `backend/src/LMS.Infrastructure/Data/LmsDbContext.cs` | PASS |

---

## 2. API Layer (Issue #34)

| Check | File | Verdict |
|---|---|---|
| `LeaveTypeDto`, `CreateLeaveTypeRequest`, `UpdateLeaveTypeRequest` records defined | `backend/src/LMS.Infrastructure/LeaveTypes/LeaveTypeDto.cs` | PASS |
| `ILeaveTypeService` interface with `ListAsync`, `GetByIdAsync`, `CreateAsync`, `UpdateAsync`, `DeactivateAsync` | `backend/src/LMS.Infrastructure/LeaveTypes/ILeaveTypeService.cs` | PASS |
| `LeaveTypeService` implements all interface methods; duplicate check uses `EF.Functions.ILike` | `backend/src/LMS.Infrastructure/LeaveTypes/LeaveTypeService.cs` | PASS |
| `LeaveTypesController` registered at `api/v1/leave-types` | `backend/src/LMS.Api/Controllers/V1/LeaveTypesController.cs` | PASS |
| `GET /api/v1/leave-types` — `[Authorize]` (all roles) | Controller `[HttpGet]` | PASS |
| `POST /api/v1/leave-types` — `[Authorize(Roles="HR_ADMIN,SUPER_ADMIN")]` | Controller `[HttpPost]` | PASS |
| `PUT /api/v1/leave-types/{id}` — `[Authorize(Roles="HR_ADMIN,SUPER_ADMIN")]` | Controller `[HttpPut]` | PASS |
| `DELETE /api/v1/leave-types/{id}` — `[Authorize(Roles="HR_ADMIN,SUPER_ADMIN")]` | Controller `[HttpDelete]` | PASS |
| `ILeaveTypeService` registered in DI | `backend/src/LMS.Api/Program.cs`, `AddScoped<ILeaveTypeService, LeaveTypeService>()` | PASS |
| Responses wrapped in `ApiResponse<T>.Ok(...)` | Controller actions | PASS |
| Duplicate returns 409 with `DUPLICATE_LEAVE_TYPE` code | Controller `Create` action | PASS |

---

## 3. UI Layer (Issue #35)

| Check | File | Verdict |
|---|---|---|
| `leaveTypeApi.ts` exports `listLeaveTypes`, `createLeaveType`, `updateLeaveType`, `deactivateLeaveType` | `frontend/src/api/leaveTypeApi.ts` | PASS |
| API reads `response.data.data` (axios body → ApiResponse payload) | `leaveTypeApi.ts`, all functions | PASS |
| `leaveTypeSlice.ts` has `fetchLeaveTypes`, `addLeaveType`, `editLeaveType`, `removeLeaveType` thunks | `frontend/src/store/leaveTypeSlice.ts` | PASS |
| `leaveTypeReducer` registered under key `"leaveType"` in store | `frontend/src/store/index.ts` | PASS |
| `LeaveTypeManagementPage` renders DataGrid with `data-testid="leave-type-table"` | `frontend/src/pages/LeaveTypeManagement/LeaveTypeManagementPage.tsx` | PASS |
| Create button has `data-testid="create-leave-type-btn"` | Page component | PASS |
| Dialog has `data-testid="leave-type-dialog"`, inputs have correct test IDs | Page component | PASS |
| Deactivate button has `data-testid="deactivate-leave-type-btn"` | Page component | PASS |
| Success snackbar has `data-testid="success-snackbar"` | Page component | PASS |
| Route `/admin/leave-types` → `LeaveTypeManagementPage` inside `ProtectedRoute` | `frontend/src/App.tsx` | PASS |

---

## Summary

All DB → API → UI wiring checks pass for F-04 Leave Type & Policy Management.
The feature is correctly layered with proper authorization, ApiResponse wrapping,
Redux state management, and testable UI selectors.

