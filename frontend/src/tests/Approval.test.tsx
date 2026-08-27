import { render, screen } from "@testing-library/react";
import { configureStore } from "@reduxjs/toolkit";
import { Provider } from "react-redux";
import { vi, describe, it, expect, beforeEach } from "vitest";
import ApprovalQueuePage from "../pages/Approvals/ApprovalQueuePage";
import { approvalReducer } from "../store/approvalSlice";
import * as approvalApi from "../api/approvalApi";
import { authReducer } from "../store/authSlice";
import { notificationsReducer } from "../store/notificationsSlice";

vi.mock("../api/approvalApi");

vi.mock("@mui/x-data-grid", () => ({
  DataGrid: ({ rows }: any) => (
    <div data-testid="approval-queue-grid">
      {rows.map((r: any) => <div key={r.id} data-testid={}>{r.employeeName}</div>)}
    </div>
  ),
}));

const makeStore = (approvals: any = []) =>
  configureStore({
    reducer: { approvals: approvalReducer, auth: authReducer, notifications: notificationsReducer },
    preloadedState: { auth: { user: { role: "HR_ADMIN" }, isAuthenticated: true } as any },
  });

// UT-AW-01: Page renders title
describe("UT-AW-01", () => {
  it("renders page title", async () => {
    vi.mocked(approvalApi.fetchPendingApprovals).mockResolvedValue({ data: [] });
    render(<Provider store={makeStore()}><ApprovalQueuePage /></Provider>);
    expect(screen.getByTestId("page-title")).toBeInTheDocument();
  });
});

// UT-AW-02: Shows loading spinner
describe("UT-AW-02", () => {
  it("shows loading spinner while fetching", () => {
    vi.mocked(approvalApi.fetchPendingApprovals).mockReturnValue(new Promise(() => {}));
    render(<Provider store={makeStore()}><ApprovalQueuePage /></Provider>);
    expect(screen.getByTestId("loading-spinner")).toBeInTheDocument();
  });
});

// UT-AW-03: Shows empty state
describe("UT-AW-03", () => {
  it("shows empty state when no pending approvals", async () => {
    vi.mocked(approvalApi.fetchPendingApprovals).mockResolvedValue({ data: [] });
    render(<Provider store={makeStore()}><ApprovalQueuePage /></Provider>);
    expect(await screen.findByTestId("empty-state")).toBeInTheDocument();
  });
});

// UT-AW-04: Shows error message
describe("UT-AW-04", () => {
  it("shows error message on API failure", async () => {
    vi.mocked(approvalApi.fetchPendingApprovals).mockRejectedValue(new Error("Network error"));
    render(<Provider store={makeStore()}><ApprovalQueuePage /></Provider>);
    expect(await screen.findByTestId("error-message")).toBeInTheDocument();
  });
});

// UT-AW-05: Renders grid with data
describe("UT-AW-05", () => {
  it("renders grid when approvals exist", async () => {
    vi.mocked(approvalApi.fetchPendingApprovals).mockResolvedValue({
      data: [{ id: "1", entityType: "LeaveApplication", employeeName: "Alice", description: "Annual leave", status: "Pending", submittedAt: "2026-08-01" }]
    });
    render(<Provider store={makeStore()}><ApprovalQueuePage /></Provider>);
    expect(await screen.findByTestId("approval-queue-grid")).toBeInTheDocument();
  });
});

// UT-AW-06: ApiResponse envelope shape
describe("UT-AW-06", () => {
  it("reads response.data (ApiResponse envelope)", async () => {
    const mock = vi.mocked(approvalApi.fetchPendingApprovals);
    mock.mockResolvedValue({ data: [] });
    render(<Provider store={makeStore()}><ApprovalQueuePage /></Provider>);
    await screen.findByTestId("empty-state");
    expect(mock).toHaveBeenCalledTimes(1);
  });
});
