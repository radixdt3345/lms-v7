# F09 Notifications Integration Smoke Test
## Feature: F-09 — Notifications & Email

### 1. Get notifications (authenticated user)
GET /api/v1/notifications → ApiResponse<List<NotificationDto>> ✓
GET /api/v1/notifications?unreadOnly=true → filtered list ✓

### 2. Get unread count
GET /api/v1/notifications/unread-count → ApiResponse<int> ✓

### 3. Mark notifications as read
PUT /api/v1/notifications/mark-read with { notificationIds: [...] } → ApiResponse<bool> ✓

### 4. UI integration
NotificationBell dispatches fetchUnreadCountThunk on mount.
Popover opens, fetchNotificationsThunk fires, list renders.
markReadThunk reduces unreadCount in store.

## Status: All flows verified end-to-end.
