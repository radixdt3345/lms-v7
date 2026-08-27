import axios from "axios";
import { ApiResponse } from "../types";

export interface NotificationDto {
  id: string; title: string; message: string; type: string;
  isRead: boolean; relatedEntityId?: string; relatedEntityType?: string; createdAt: string;
}
const BASE = "/api/v1/notifications";
export const fetchNotifications = async (unreadOnly = false): Promise<ApiResponse<NotificationDto[]>> =>
  (await axios.get<ApiResponse<NotificationDto[]>>(`${BASE}?unreadOnly=${unreadOnly}`)).data;
export const fetchUnreadCount = async (): Promise<ApiResponse<number>> =>
  (await axios.get<ApiResponse<number>>(`${BASE}/unread-count`)).data;
export const markNotificationsRead = async (ids: string[]): Promise<ApiResponse<boolean>> =>
  (await axios.put<ApiResponse<boolean>>(`${BASE}/mark-read`, { notificationIds: ids })).data;
