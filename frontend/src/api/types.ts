/**
 * Shared API response envelope - matches backend ApiResponse<T>.
 * Every backend endpoint returns { "data": T }.
 * NEVER access response.data directly - always response.data.data.
 */
export interface ApiResponse<T> {
  data: T;
}
