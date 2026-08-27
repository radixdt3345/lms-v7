import { createAsyncThunk, createSlice } from "@reduxjs/toolkit";
import { fetchPendingApprovals, PendingApprovalDto } from "../api/approvalApi";

interface ApprovalState {
  pending: PendingApprovalDto[];
  loading: boolean;
  error: string | null;
}
const initialState: ApprovalState = { pending: [], loading: false, error: null };

export const fetchPendingApprovalsThunk = createAsyncThunk("approvals/fetchPending", async () => {
  const res = await fetchPendingApprovals();
  return res.data;
});

const approvalSlice = createSlice({
  name: "approvals",
  initialState,
  reducers: {},
  extraReducers: (builder) => {
    builder
      .addCase(fetchPendingApprovalsThunk.pending, (s) => { s.loading = true; s.error = null; })
      .addCase(fetchPendingApprovalsThunk.fulfilled, (s, a) => { s.loading = false; s.pending = a.payload ?? []; })
      .addCase(fetchPendingApprovalsThunk.rejected, (s, a) => { s.loading = false; s.error = a.error.message ?? "Error"; });
  },
});
export const approvalReducer = approvalSlice.reducer;
