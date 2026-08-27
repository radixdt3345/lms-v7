import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { Provider } from "react-redux";
import { configureStore } from "@reduxjs/toolkit";
import { MemoryRouter } from "react-router-dom";
import BackgroundJobsPage from "../pages/BackgroundJobs/BackgroundJobsPage";
import * as jobApi from "../api/jobApi";
import { authReducer } from "../store/authSlice";
import { notificationsReducer } from "../store/notificationsSlice";

vi.mock("../api/jobApi", () => ({
  triggerExpireCompOff: vi.fn(),
  triggerResetLeaveBalances: vi.fn(),
  triggerSendReminders: vi.fn(),
  fetchJobLogs: vi.fn(),
}));

const makeStore = () => configureStore({
  reducer: { auth: authReducer, notifications: notificationsReducer },
  preloadedState: { auth: { user: { id: "1", role: "HR_ADMIN" }, isAuthenticated: true, token: "t" } },
});

const Wrapper = ({ children }: { children: React.ReactNode }) => (
  <Provider store={makeStore()}><MemoryRouter>{children}</MemoryRouter></Provider>
);

const mockLog = { id: "1", jobName: "ExpireCompOffCredits", status: "Success", details: "2 expired.", startedAt: new Date().toISOString(), completedAt: new Date().toISOString() };

beforeEach(() => {
  vi.clearAllMocks();
  vi.mocked(jobApi.fetchJobLogs).mockResolvedValue([mockLog]);
  vi.mocked(jobApi.triggerExpireCompOff).mockResolvedValue("2 comp-off credits expired.");
  vi.mocked(jobApi.triggerResetLeaveBalances).mockResolvedValue("5 balances created.");
  vi.mocked(jobApi.triggerSendReminders).mockResolvedValue("3 reminders sent.");
});

describe("BackgroundJobsPage", () => {
  // UT-BJ-01: renders page title
  it("UT-BJ-01: renders page title", () => {
    render(<BackgroundJobsPage />, { wrapper: Wrapper });
    expect(screen.getByTestId("page-title")).toHaveTextContent("Background Jobs");
  });

  // UT-BJ-02: renders all three job trigger buttons
  it("UT-BJ-02: renders all trigger buttons", () => {
    render(<BackgroundJobsPage />, { wrapper: Wrapper });
    expect(screen.getByTestId("trigger-expire-compoff")).toBeInTheDocument();
    expect(screen.getByTestId("trigger-reset-balances")).toBeInTheDocument();
    expect(screen.getByTestId("trigger-send-reminders")).toBeInTheDocument();
  });

  // UT-BJ-03: loads and displays job logs
  it("UT-BJ-03: displays job logs table", async () => {
    render(<BackgroundJobsPage />, { wrapper: Wrapper });
    await waitFor(() => expect(screen.getByTestId("job-logs-table")).toBeInTheDocument());
    expect(screen.getByText("ExpireCompOffCredits")).toBeInTheDocument();
  });

  // UT-BJ-04: triggers expire comp-off job
  it("UT-BJ-04: triggers expire comp-off", async () => {
    render(<BackgroundJobsPage />, { wrapper: Wrapper });
    fireEvent.click(screen.getByTestId("trigger-expire-compoff"));
    await waitFor(() => expect(jobApi.triggerExpireCompOff).toHaveBeenCalled());
    await waitFor(() => expect(screen.getByTestId("success-message")).toBeInTheDocument());
  });

  // UT-BJ-05: shows error on job failure
  it("UT-BJ-05: shows error on failure", async () => {
    vi.mocked(jobApi.triggerExpireCompOff).mockRejectedValue(new Error("fail"));
    render(<BackgroundJobsPage />, { wrapper: Wrapper });
    fireEvent.click(screen.getByTestId("trigger-expire-compoff"));
    await waitFor(() => expect(screen.getByTestId("error-message")).toBeInTheDocument());
  });

  // UT-BJ-06: triggers send reminders job
  it("UT-BJ-06: triggers send reminders", async () => {
    render(<BackgroundJobsPage />, { wrapper: Wrapper });
    fireEvent.click(screen.getByTestId("trigger-send-reminders"));
    await waitFor(() => expect(jobApi.triggerSendReminders).toHaveBeenCalled());
  });
});
