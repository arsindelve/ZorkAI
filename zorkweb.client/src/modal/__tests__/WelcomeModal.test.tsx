import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import WelcomeDialog from '../WelcomeModal';

describe('WelcomeDialog', () => {
  it('renders the dialog when open is true', () => {
    render(<WelcomeDialog open={true} handleClose={() => {}} />);
    
    expect(screen.getByText('Welcome to Zork AI')).toBeInTheDocument();
    expect(screen.getByText(/This is a re-imagining of the original 1980's classic text adventure game Zork I./)).toBeInTheDocument();
    expect(screen.getByText('Close')).toBeInTheDocument();
  });
  
  it('does not render the dialog when open is false', () => {
    render(<WelcomeDialog open={false} handleClose={() => {}} />);
    
    expect(screen.queryByText('Welcome to Zork AI')).not.toBeInTheDocument();
    expect(screen.queryByText(/This is a re-imagining of the original 1980's classic text adventure game Zork I./)).not.toBeInTheDocument();
    expect(screen.queryByText('Close')).not.toBeInTheDocument();
  });
  
  it('calls handleClose when Close button is clicked', () => {
    const handleClose = vi.fn();
    render(<WelcomeDialog open={true} handleClose={handleClose} />);
    
    fireEvent.click(screen.getByText('Close'));
    
    expect(handleClose).toHaveBeenCalledTimes(1);
  });
  
  it('contains game instructions', () => {
    render(<WelcomeDialog open={true} handleClose={() => {}} />);
    
    expect(screen.getByText(/To get started, type your input in the grey box below, and press 'return'./)).toBeInTheDocument();
    expect(screen.getByText(/Need inspiration\? try "open the mailbox", "go south", or "jump up and down"./)).toBeInTheDocument();
  });
});