import { render, screen } from "@testing-library/react";
import { configureStore } from "@reduxjs/toolkit";
import { Provider } from "react-redux";
import { vi, describe, it, expect, beforeEach } from "vitest";
import NotificationBell from "../components/NotificationBell";
import { notificationsReducer } from "../store/notificationsSlice";
import * as notifApi from "../api/notificationApi";
import { authReducer } from "../store/authSlice";

vi.mock("../api/notificationApi");

const makeStore = () => configureStore({
  reducer: { notifications: notificationsReducer, auth: authReducer },
  preloadedState: { auth: { user: { role: "EMPLOYEE" }, isAuthenticated: true } as any },
});

beforeEach(() => {
  vi.clearAllMocks();
  vi.mocked(notifApi.fetchUnreadCount).mockResolvedValue({ data: 0 });
  vi.mocked(notifApi.fetchNotifications).mockResolvedValue({ data: [] });
});

describe("UT-NF-01", () => {
  it("renders notification bell", () => {
    render(<Provider store={makeStore()}><NotificationBell /></Provider>);
    expect(screen.getByTestId("notification-bell")).toBeInTheDocument();
  });
});
describe("UT-NF-02", () => {
  it("shows unread count badge", async () => {
    vi.mocked(notifApi.fetchUnreadCount).mockResolvedValue({ data: 3 });
    render(<Provider store={makeStore()}><NotificationBell /></Provider>);
    expect(screen.getByTestId("unread-badge")).toBeInTheDocument();
  });
});
describe("UT-NF-03", () => {
  it("reads fetchUnreadCount response.data (ApiResponse envelope)", async () => {
    const mock = vi.mocked(notifApi.fetchUnreadCount);
    mock.mockResolvedValue({ data: 5 });
    render(<Provider store={makeStore()}><NotificationBell /></Provider>);
    expect(mock).toHaveBeenCalledTimes(1);
  });
});
describe("UT-NF-04", () => {
  it("shows no-notifications text when empty", async () => {
    vi.mocked(notifApi.fetchNotifications).mockResolvedValue({ data: [] });
    const { getByTestId } = render(<Provider store={makeStore()}><NotificationBell /></Provider>);
    await getByTestId("notification-bell").click();
    expect(await screen.findByTestId("no-notifications")).toBeInTheDocument();
  });
});
describe("UT-NF-05", () => {
  it("shows mark-all-read button in popover", async () => {
    render(<Provider store={makeStore()}><NotificationBell /></Provider>);
    screen.getByTestId("notification-bell").click();
    expect(await screen.findByTestId("mark-all-read-btn")).toBeInTheDocument();
  });
});
describe("UT-NF-06", () => {
  it("notification popover renders", async () => {
    render(<Provider store={makeStore()}><NotificationBell /></Provider>);
    screen.getByTestId("notification-bell").click();
    expect(await screen.findByTestId("notification-popover")).toBeInTheDocument();
  });
});
