import axios from "axios";
import { ApiResponse } from "../types";

export interface LeaveBalanceSummary { leaveTypeName: string; total: number; used: number; remaining: number; }
export interface EmployeeDashboardDto {
  pendingLeaves: number; approvedLeaves: number; rejectedLeaves: number;
  pendingCompOff: number; totalLeaveBalance: number; unreadNotifications: number;
  leaveBalances: LeaveBalanceSummary[];
}
export interface HrDashboardDto {
  totalEmployees: number; pendingLeaveApprovals: number; pendingCompOffApprovals: number;
  todayOnLeave: number; thisMonthApprovals: number;
}
export const fetchEmployeeDashboard = async (): Promise<ApiResponse<EmployeeDashboardDto>> =>
  (await axios.get<ApiResponse<EmployeeDashboardDto>>("/api/v1/dashboard/employee")).data;
export const fetchHrDashboard = async (): Promise<ApiResponse<HrDashboardDto>> =>
  (await axios.get<ApiResponse<HrDashboardDto>>("/api/v1/dashboard/hr")).data;
