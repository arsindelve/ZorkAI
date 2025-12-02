import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import RestartConfirmDialog from '../modal/RestartConfirmDialog';

describe('RestartConfirmDialog Component', () => {
  const mockSetOpen = jest.fn();
  const mockOnConfirm = jest.fn();
  const mockOnCancel = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
  });

  test('renders nothing when open is false', () => {
    render(
      <RestartConfirmDialog 
        open={false} 
        setOpen={mockSetOpen} 
        onConfirm={mockOnConfirm} 
        onCancel={mockOnCancel} 
      />
    );

    // Dialog should not be in the document
    expect(screen.queryByTestId('restart-confirm-dialog')).not.toBeInTheDocument();
  });

  test('renders dialog when open is true', () => {
    render(
      <RestartConfirmDialog 
        open={true} 
        setOpen={mockSetOpen} 
        onConfirm={mockOnConfirm} 
        onCancel={mockOnCancel} 
      />
    );

    // Dialog should be in the document
    expect(screen.getByTestId('restart-confirm-dialog')).toBeInTheDocument();
    expect(screen.getByText('Restart Your Game? Are you sure?')).toBeInTheDocument();
    expect(screen.getByText('Your game will be reset to the beginning. Are you sure you want to restart?')).toBeInTheDocument();
  });

  test('calls setOpen(false) and onCancel when Cancel button is clicked', () => {
    render(
      <RestartConfirmDialog 
        open={true} 
        setOpen={mockSetOpen} 
        onConfirm={mockOnConfirm} 
        onCancel={mockOnCancel} 
      />
    );

    // Click the Cancel button
    fireEvent.click(screen.getByTestId('restart-confirm-cancel'));

    // Check that setOpen and onCancel were called
    expect(mockSetOpen).toHaveBeenCalledWith(false);
    expect(mockOnCancel).toHaveBeenCalledTimes(1);
    expect(mockOnConfirm).not.toHaveBeenCalled();
  });

  test('calls setOpen(false) and onConfirm when Restart button is clicked', () => {
    render(
      <RestartConfirmDialog 
        open={true} 
        setOpen={mockSetOpen} 
        onConfirm={mockOnConfirm} 
        onCancel={mockOnCancel} 
      />
    );

    // Click the Restart button
    fireEvent.click(screen.getByTestId('restart-confirm-yes'));

    // Check that setOpen and onConfirm were called
    expect(mockSetOpen).toHaveBeenCalledWith(false);
    expect(mockOnConfirm).toHaveBeenCalledTimes(1);
    expect(mockOnCancel).not.toHaveBeenCalled();
  });

  test('does not call onCancel when it is not provided', () => {
    render(
      <RestartConfirmDialog 
        open={true} 
        setOpen={mockSetOpen} 
        onConfirm={mockOnConfirm} 
      />
    );

    // Click the Cancel button
    fireEvent.click(screen.getByTestId('restart-confirm-cancel'));

    // Check that setOpen was called but onCancel was not (since it wasn't provided)
    expect(mockSetOpen).toHaveBeenCalledWith(false);
    expect(mockOnCancel).not.toHaveBeenCalled();
  });

  test('calls setOpen(false) when clicking outside the dialog', () => {
    render(
      <RestartConfirmDialog 
        open={true} 
        setOpen={mockSetOpen} 
        onConfirm={mockOnConfirm} 
        onCancel={mockOnCancel} 
      />
    );

    // Find the dialog element
    const dialog = screen.getByTestId('restart-confirm-dialog');
    expect(dialog).toBeInTheDocument();

    // Simulate clicking outside the dialog by triggering the onClose handler directly
    // Get the backdrop element and click it
    const backdrop = document.querySelector('.MuiBackdrop-root');
    if (backdrop) {
      fireEvent.click(backdrop);
    }

    // Check that setOpen was called
    expect(mockSetOpen).toHaveBeenCalledWith(false);
  });
});
