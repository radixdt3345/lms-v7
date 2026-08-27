import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import AppFooter from '../../components/AppFooter';

describe('AppFooter', () => {
  it('UT-SYS-01: renders footer element', () => {
    render(<AppFooter />);
    expect(screen.getByTestId('app-footer')).toBeInTheDocument();
  });

  it('UT-SYS-02: displays version text', () => {
    render(<AppFooter />);
    expect(screen.getByTestId('footer-version')).toBeInTheDocument();
  });

  it('UT-SYS-03: shows health link', () => {
    render(<AppFooter />);
    expect(screen.getByTestId('footer-health-link')).toBeInTheDocument();
  });

  it('UT-SYS-04: health link points to correct URL', () => {
    render(<AppFooter />);
    expect(screen.getByTestId('footer-health-link')).toHaveAttribute('href', '/api/v1/system/health');
  });

  it('UT-SYS-05: renders current year', () => {
    render(<AppFooter />);
    expect(screen.getByTestId('footer-version').textContent).toContain(String(new Date().getFullYear()));
  });

  it('UT-SYS-06: footer is accessible landmark', () => {
    render(<AppFooter />);
    expect(screen.getByRole('contentinfo')).toBeInTheDocument();
  });
});
