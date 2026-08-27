import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { Provider } from "react-redux";
import { configureStore } from "@reduxjs/toolkit";
import { MemoryRouter } from "react-router-dom";
import ReportsPage from "../pages/Reports/ReportsPage";
import * as reportApi from "../api/reportApi";
import { authReducer } from "../store/authSlice";
import { notificationsReducer } from "../store/notificationsSlice";

vi.mock("../api/reportApi", () => ({
  downloadLeaveReport: vi.fn(),
  downloadCompOffReport: vi.fn(),
  downloadLeaveBalanceReport: vi.fn(),
}));

const makeStore = () => configureStore({
  reducer: { auth: authReducer, notifications: notificationsReducer },
  preloadedState: { auth: { user: { id: "1", role: "HR_ADMIN" }, isAuthenticated: true, token: "t" } },
});

const Wrapper = ({ children }: { children: React.ReactNode }) => (
  <Provider store={makeStore()}><MemoryRouter>{children}</MemoryRouter></Provider>
);

const blob = new Blob(["csv"], { type: "text/csv" });

beforeEach(() => {
  vi.clearAllMocks();
  vi.mocked(reportApi.downloadLeaveReport).mockResolvedValue(blob);
  vi.mocked(reportApi.downloadCompOffReport).mockResolvedValue(blob);
  vi.mocked(reportApi.downloadLeaveBalanceReport).mockResolvedValue(blob);
  // Mock URL.createObjectURL
  global.URL.createObjectURL = vi.fn(() => "blob:mock");
  global.URL.revokeObjectURL = vi.fn();
});

// UT-RP-01: renders page title
describe("ReportsPage", () => {
  it("UT-RP-01: renders page title", () => {
    render(<ReportsPage />, { wrapper: Wrapper });
    expect(screen.getByTestId("page-title")).toHaveTextContent("Reports");
  });

  // UT-RP-02: renders all three download buttons
  it("UT-RP-02: renders all three download buttons", () => {
    render(<ReportsPage />, { wrapper: Wrapper });
    expect(screen.getByTestId("download-leave-report")).toBeInTheDocument();
    expect(screen.getByTestId("download-compoff-report")).toBeInTheDocument();
    expect(screen.getByTestId("download-balance-report")).toBeInTheDocument();
  });

  // UT-RP-03: downloads leave report on click
  it("UT-RP-03: downloads leave report on click", async () => {
    render(<ReportsPage />, { wrapper: Wrapper });
    fireEvent.click(screen.getByTestId("download-leave-report"));
    await waitFor(() => expect(reportApi.downloadLeaveReport).toHaveBeenCalled());
  });

  // UT-RP-04: downloads comp-off report on click
  it("UT-RP-04: downloads comp-off report on click", async () => {
    render(<ReportsPage />, { wrapper: Wrapper });
    fireEvent.click(screen.getByTestId("download-compoff-report"));
    await waitFor(() => expect(reportApi.downloadCompOffReport).toHaveBeenCalled());
  });

  // UT-RP-05: shows error message on failure
  it("UT-RP-05: shows error message on failure", async () => {
    vi.mocked(reportApi.downloadLeaveReport).mockRejectedValue(new Error("fail"));
    render(<ReportsPage />, { wrapper: Wrapper });
    fireEvent.click(screen.getByTestId("download-leave-report"));
    await waitFor(() => expect(screen.getByTestId("error-message")).toBeInTheDocument());
  });

  // UT-RP-06: downloads leave balance report on click
  it("UT-RP-06: downloads leave balance report on click", async () => {
    render(<ReportsPage />, { wrapper: Wrapper });
    fireEvent.click(screen.getByTestId("download-balance-report"));
    await waitFor(() => expect(reportApi.downloadLeaveBalanceReport).toHaveBeenCalled());
  });
});
