import { render, screen } from '@testing-library/react';
import { Provider } from 'react-redux';
import { configureStore } from '@reduxjs/toolkit';
import PublicHolidayManagementPage from '../pages/PublicHolidayManagement/PublicHolidayManagementPage';
import * as api from '../api/publicHolidayApi';
import { vi } from 'vitest';

vi.mock('../api/publicHolidayApi');

const store = configureStore({ reducer: {} });
const wrapper = ({ children }: any) => <Provider store={store}>{children}</Provider>;

describe('PublicHolidayManagementPage', () => {
  beforeEach(() => {
    vi.mocked(api.listHolidays).mockResolvedValue([]);
  });

  it('renders page title', async () => {
    render(<PublicHolidayManagementPage />, { wrapper });
    expect(screen.getByTestId('page-title')).toBeInTheDocument();
  });

  it('shows add holiday button', async () => {
    render(<PublicHolidayManagementPage />, { wrapper });
    expect(screen.getByTestId('add-holiday-btn')).toBeInTheDocument();
  });
});
