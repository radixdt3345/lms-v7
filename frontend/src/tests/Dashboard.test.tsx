import { render, screen } from "@testing-library/react";
import { configureStore } from "@reduxjs/toolkit";
import { Provider } from "react-redux";
import { vi, describe, it, expect, beforeEach } from "vitest";
import EmployeeDashboardPage from "../pages/Dashboard/EmployeeDashboardPage";
import { dashboardReducer } from "../store/dashboardSlice";
import { notificationsReducer } from "../store/notificationsSlice";
import { authReducer } from "../store/authSlice";
import * as dashApi from "../api/dashboardApi";

vi.mock("../api/dashboardApi");
vi.mock("../api/notificationApi");

const makeDash = () => ({ pendingLeaves: 1, approvedLeaves: 2, rejectedLeaves: 0,
  pendingCompOff: 0, totalLeaveBalance: 10, unreadNotifications: 3, leaveBalances: [] });

const makeStore = () => configureStore({
  reducer: { dashboard: dashboardReducer, notifications: notificationsReducer, auth: authReducer },
  preloadedState: { auth: { user: { role: "EMPLOYEE" }, isAuthenticated: true } as any },
});

beforeEach(() => { vi.clearAllMocks(); vi.mocked(dashApi.fetchEmployeeDashboard).mockResolvedValue({ data: makeDash() }); });

describe("UT-DB-01", () => {
  it("renders page title", () => {
    render(<Provider store={makeStore()}><EmployeeDashboardPage /></Provider>);
    expect(screen.getByTestId("page-title")).toBeInTheDocument();
  });
});
describe("UT-DB-02", () => {
  it("shows loading spinner", () => {
    vi.mocked(dashApi.fetchEmployeeDashboard).mockReturnValue(new Promise(() => {}));
    render(<Provider store={makeStore()}><EmployeeDashboardPage /></Provider>);
    expect(screen.getByTestId("loading-spinner")).toBeInTheDocument();
  });
});
describe("UT-DB-03", () => {
  it("shows error on failure", async () => {
    vi.mocked(dashApi.fetchEmployeeDashboard).mockRejectedValue(new Error("fail"));
    render(<Provider store={makeStore()}><EmployeeDashboardPage /></Provider>);
    expect(await screen.findByTestId("error-message")).toBeInTheDocument();
  });
});
describe("UT-DB-04", () => {
  it("shows pending leaves stat", async () => {
    render(<Provider store={makeStore()}><EmployeeDashboardPage /></Provider>);
    expect(await screen.findByTestId("stat-pending-leaves")).toBeInTheDocument();
  });
});
describe("UT-DB-05", () => {
  it("reads response.data (ApiResponse envelope)", async () => {
    const mock = vi.mocked(dashApi.fetchEmployeeDashboard);
    mock.mockResolvedValue({ data: makeDash() });
    render(<Provider store={makeStore()}><EmployeeDashboardPage /></Provider>);
    await screen.findByTestId("stat-pending-leaves");
    expect(mock).toHaveBeenCalledTimes(1);
  });
});
describe("UT-DB-06", () => {
  it("shows leave balance stat card", async () => {
    render(<Provider store={makeStore()}><EmployeeDashboardPage /></Provider>);
    expect(await screen.findByTestId("stat-leave-balance")).toBeInTheDocument();
  });
});
