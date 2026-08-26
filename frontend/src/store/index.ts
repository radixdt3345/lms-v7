import { configureStore } from '@reduxjs/toolkit';
import authenticationReducer from './authenticationSlice';
import { departmentReducer } from './departmentSlice';

export const store = configureStore({
  reducer: {
    authentication: authenticationReducer,
    department: departmentReducer,
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
