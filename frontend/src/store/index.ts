import { configureStore } from '@reduxjs/toolkit';
import authenticationReducer from './authenticationSlice';
import employeeReducer from './employeeSlice';
import leaveTypeReducer from './leaveTypeSlice';
import { auditLogReducer } from './auditLogSlice';

export const store = configureStore({
  reducer: {
    authentication: authenticationReducer,
    employee: employeeReducer,
    leaveType: leaveTypeReducer,
    auditLog: auditLogReducer,
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
