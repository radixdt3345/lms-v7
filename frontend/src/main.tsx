import React from 'react';
import ReactDOM from 'react-dom/client';
import { Provider } from 'react-redux';
import { BrowserRouter } from 'react-router-dom';
import { CssBaseline, ThemeProvider, createTheme } from '@mui/material';
import App from './App';
import { store } from './store';

const theme = createTheme({
  palette: {
    primary: { main: '#1566F1' },
    secondary: { main: '#2A2E35' },
    success: { main: '#2D9F6E' },
    warning: { main: '#E98C00' },
    error: { main: '#D92D20' },
    background: { default: '#F0F1F3', paper: '#FFFFFF' },
    text: { primary: '#1A2033' },
  },
  typography: {
    fontFamily: '"Inter", system-ui, -apple-system, sans-serif',
  },
});

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <Provider store={store}>
      <BrowserRouter>
        <ThemeProvider theme={theme}>
          <CssBaseline />
          <App />
        </ThemeProvider>
      </BrowserRouter>
    </Provider>
  </React.StrictMode>
);
