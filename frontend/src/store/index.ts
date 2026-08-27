import { dashboardReducer } from "./dashboardSlice";
import { notificationsReducer } from "./notificationsSlice";
import { approvalReducer } from "./approvalSlice";
import { configureStore } from '@reduxjs/toolkit';
import authenticationReducer from './authenticationSlice';
import employeeReducer from './employeeSlice';
import leaveTypeReducer from './leaveTypeSlice';
import { auditLogReducer } from './auditLogSlice';
import { leaveBalanceReducer } from './leaveBalanceSlice';
import { compOffReducer } from "./compOffSlice";
import { approvalReducer } from './compOffSlice';
import { leaveApplicationReducer } from './leaveApplicationSlice';

export const store = configureStore({
  reducer: {
    authentication: authenticationReducer,
    employee: employeeReducer,
    leaveType: leaveTypeReducer,
    auditLog: auditLogReducer,
    leaveBalance: leaveBalanceReducer,
    leaveApplication: leaveApplicationReducer,
    compOff: compOffReducer,
  approvals: approvalReducer,
  notifications: notificationsReducer,
  dashboard: dashboardReducer,
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
