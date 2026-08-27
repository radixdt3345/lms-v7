import { createAsyncThunk, createSlice } from "@reduxjs/toolkit";
import { fetchUnreadCount, fetchNotifications, markNotificationsRead, NotificationDto } from "../api/notificationApi";

interface NotificationState { items: NotificationDto[]; unreadCount: number; loading: boolean; error: string | null; }
const initialState: NotificationState = { items: [], unreadCount: 0, loading: false, error: null };

export const fetchUnreadCountThunk = createAsyncThunk("notifications/fetchUnreadCount", async () =>
  (await fetchUnreadCount()).data ?? 0);
export const fetchNotificationsThunk = createAsyncThunk("notifications/fetch", async (unreadOnly: boolean = false) =>
  (await fetchNotifications(unreadOnly)).data ?? []);
export const markReadThunk = createAsyncThunk("notifications/markRead", async (ids: string[]) => {
  await markNotificationsRead(ids); return ids;
});

const slice = createSlice({ name: "notifications", initialState, reducers: {},
  extraReducers: (b) => {
    b.addCase(fetchUnreadCountThunk.fulfilled, (s, a) => { s.unreadCount = a.payload; });
    b.addCase(fetchNotificationsThunk.pending, (s) => { s.loading = true; });
    b.addCase(fetchNotificationsThunk.fulfilled, (s, a) => { s.loading = false; s.items = a.payload; });
    b.addCase(fetchNotificationsThunk.rejected, (s, a) => { s.loading = false; s.error = a.error.message ?? "Error"; });
    b.addCase(markReadThunk.fulfilled, (s, a) => {
      s.items = s.items.map(n => a.payload.includes(n.id) ? { ...n, isRead: true } : n);
      s.unreadCount = Math.max(0, s.unreadCount - a.payload.length);
    });
  }
});
export const notificationsReducer = slice.reducer;
