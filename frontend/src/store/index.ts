import { configureStore } from '@reduxjs/toolkit';
import authenticationReducer from './authenticationSlice';
import employeeReducer from './employeeSlice';
import { auditLogReducer } from './auditLogSlice';

export const store = configureStore({
  reducer: {
    authentication: authenticationReducer,
    employee: employeeReducer,
    auditLog: auditLogReducer,
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
