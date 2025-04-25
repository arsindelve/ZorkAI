import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import ConfirmDialog from '../ConfirmationDialog';

describe('ConfirmDialog', () => {
  it('renders the dialog with title when open', () => {
    const setOpen = vi.fn();
    const onConfirm = vi.fn();
    const title = 'Test Dialog Title';
    
    render(
      <ConfirmDialog 
        title={title} 
        open={true} 
        setOpen={setOpen} 
        onConfirm={onConfirm}
      >
        <p>Test dialog content</p>
      </ConfirmDialog>
    );
    
    expect(screen.getByText(title)).toBeInTheDocument();
    expect(screen.getByText('Test dialog content')).toBeInTheDocument();
    expect(screen.getByText('Yes')).toBeInTheDocument();
    expect(screen.getByText('No')).toBeInTheDocument();
  });
  
  it('does not render when open is false', () => {
    const setOpen = vi.fn();
    const onConfirm = vi.fn();
    const title = 'Test Dialog Title';
    
    render(
      <ConfirmDialog 
        title={title} 
        open={false} 
        setOpen={setOpen} 
        onConfirm={onConfirm}
      >
        <p>Test dialog content</p>
      </ConfirmDialog>
    );
    
    expect(screen.queryByText(title)).not.toBeInTheDocument();
    expect(screen.queryByText('Test dialog content')).not.toBeInTheDocument();
  });
  
  it('calls setOpen(false) when No button is clicked', () => {
    const setOpen = vi.fn();
    const onConfirm = vi.fn();
    
    render(
      <ConfirmDialog 
        title="Test Dialog" 
        open={true} 
        setOpen={setOpen} 
        onConfirm={onConfirm}
      />
    );
    
    fireEvent.click(screen.getByText('No'));
    expect(setOpen).toHaveBeenCalledWith(false);
    expect(onConfirm).not.toHaveBeenCalled();
  });
  
  it('calls onConfirm and setOpen(false) when Yes button is clicked', () => {
    const setOpen = vi.fn();
    const onConfirm = vi.fn();
    
    render(
      <ConfirmDialog 
        title="Test Dialog" 
        open={true} 
        setOpen={setOpen} 
        onConfirm={onConfirm}
      />
    );
    
    fireEvent.click(screen.getByText('Yes'));
    expect(setOpen).toHaveBeenCalledWith(false);
    expect(onConfirm).toHaveBeenCalled();
  });
});