import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import FunctionsMenu from '../FunctionsMenu';

describe('FunctionsMenu', () => {
  const restartMock = vi.fn();
  const restoreMock = vi.fn();
  const saveMock = vi.fn();
  const gameMethods = [restartMock, restoreMock, saveMock];
  
  beforeEach(() => {
    vi.clearAllMocks();
  });
  
  it('renders the menu button', () => {
    render(<FunctionsMenu gameMethods={gameMethods} forceClose={false} />);
    
    expect(screen.getByText('Game Menu')).toBeInTheDocument();
  });
  
  it('opens the menu when button is clicked', () => {
    render(<FunctionsMenu gameMethods={gameMethods} forceClose={false} />);
    
    // Menu should be closed initially
    expect(screen.queryByText('Restart Your Game')).not.toBeInTheDocument();
    
    // Click the menu button
    fireEvent.click(screen.getByText('Game Menu'));
    
    // Menu should be open now
    expect(screen.getByText('Restart Your Game')).toBeInTheDocument();
    expect(screen.getByText('Restore a Previous Saved Game')).toBeInTheDocument();
    expect(screen.getByText('Save your Game')).toBeInTheDocument();
  });
  
  it('calls restart function when "Restart Your Game" is clicked', () => {
    render(<FunctionsMenu gameMethods={gameMethods} forceClose={false} />);
    
    // Open the menu
    fireEvent.click(screen.getByText('Game Menu'));
    
    // Click the restart option
    fireEvent.click(screen.getByText('Restart Your Game'));
    
    // Check if restart function was called
    expect(restartMock).toHaveBeenCalledTimes(1);
    expect(restoreMock).not.toHaveBeenCalled();
    expect(saveMock).not.toHaveBeenCalled();
  });
  
  it('calls restore function when "Restore a Previous Saved Game" is clicked', () => {
    render(<FunctionsMenu gameMethods={gameMethods} forceClose={false} />);
    
    // Open the menu
    fireEvent.click(screen.getByText('Game Menu'));
    
    // Click the restore option
    fireEvent.click(screen.getByText('Restore a Previous Saved Game'));
    
    // Check if restore function was called
    expect(restoreMock).toHaveBeenCalledTimes(1);
    expect(restartMock).not.toHaveBeenCalled();
    expect(saveMock).not.toHaveBeenCalled();
  });
  
  it('calls save function when "Save your Game" is clicked', () => {
    render(<FunctionsMenu gameMethods={gameMethods} forceClose={false} />);
    
    // Open the menu
    fireEvent.click(screen.getByText('Game Menu'));
    
    // Click the save option
    fireEvent.click(screen.getByText('Save your Game'));
    
    // Check if save function was called
    expect(saveMock).toHaveBeenCalledTimes(1);
    expect(restartMock).not.toHaveBeenCalled();
    expect(restoreMock).not.toHaveBeenCalled();
  });
});