import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import WelcomeDialog from '../modal/WelcomeModal';

describe('WelcomeDialog Component', () => {
  const mockHandleClose = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
  });

  test('renders nothing when open is false', () => {
    render(
      <WelcomeDialog
        open={false}
        handleClose={mockHandleClose}
      />
    );

    // Dialog should not be in the document
    expect(screen.queryByTestId('welcome-modal')).not.toBeInTheDocument();
  });

  test('renders dialog when open is true', () => {
    render(
      <WelcomeDialog
        open={true}
        handleClose={mockHandleClose}
      />
    );

    // Dialog should be in the document
    expect(screen.getByTestId('welcome-modal')).toBeInTheDocument();
    expect(screen.getByText('Welcome to Planetfall AI - A Modern Reimagining of the 1983 Classic!')).toBeInTheDocument();
  });

  test('displays information about the game', () => {
    render(
      <WelcomeDialog
        open={true}
        handleClose={mockHandleClose}
      />
    );

    // Check that the dialog contains information about the game
    expect(screen.getByText(/About Planetfall AI/)).toBeInTheDocument();
    expect(screen.getByText(/This is a modern re-imagining of the beloved 1983 science fiction text adventure game Planetfall./)).toBeInTheDocument();
    expect(screen.getByText(/Need inspiration\? Try:/)).toBeInTheDocument();

    // Check that example commands are displayed
    expect(screen.getByText('look', { exact: false })).toBeInTheDocument();
    expect(screen.getByText('take the kit', { exact: false })).toBeInTheDocument();
    expect(screen.getByText('go west', { exact: false })).toBeInTheDocument();
  });

  test('calls handleClose when close button is clicked', () => {
    render(
      <WelcomeDialog
        open={true}
        handleClose={mockHandleClose}
      />
    );

    // Click the close button
    fireEvent.click(screen.getByTestId('welcome-modal-close-button'));

    // Check that handleClose was called
    expect(mockHandleClose).toHaveBeenCalledTimes(1);
  });

  test('contains a link to Steve Meretzky Wikipedia', () => {
    render(
      <WelcomeDialog
        open={true}
        handleClose={mockHandleClose}
      />
    );

    // Check that the link to Wikipedia is present
    const link = screen.getByText(/Steve Meretzky/);
    expect(link).toBeInTheDocument();
    expect(link.closest('a')).toHaveAttribute('href', 'https://en.wikipedia.org/wiki/Steve_Meretzky');
    expect(link.closest('a')).toHaveAttribute('target', '_blank');
  });
});
