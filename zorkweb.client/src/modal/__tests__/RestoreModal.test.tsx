import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import RestoreModal from '../RestoreModal';
import { ISavedGame } from '../../model/SavedGame';

// Mock moment to have consistent date formatting in tests
vi.mock('moment', () => ({
  default: {
    utc: vi.fn().mockImplementation(() => ({
      local: vi.fn().mockReturnValue({
        format: vi.fn().mockReturnValue('January 1st, 12:00 pm')
      })
    }))
  }
}));

describe('RestoreModal', () => {
  const mockHandleClose = vi.fn();
  
  const mockSavedGames: ISavedGame[] = [
    {
      id: 'game-1',
      name: 'Saved Game 1',
      description: 'First saved game',
      date: '2023-01-01T12:00:00Z'
    },
    {
      id: 'game-2',
      name: 'Saved Game 2',
      description: 'Second saved game',
      date: '2023-01-02T12:00:00Z'
    }
  ];
  
  beforeEach(() => {
    mockHandleClose.mockClear();
  });
  
  it('renders the dialog title when open', () => {
    render(<RestoreModal open={true} handleClose={mockHandleClose} games={[]} />);
    
    expect(screen.getByText('Restore A Previously Saved Game:')).toBeInTheDocument();
  });
  
  it('shows a message when there are no saved games', () => {
    render(<RestoreModal open={true} handleClose={mockHandleClose} games={[]} />);
    
    expect(screen.getByText("You don't have any previously saved games.")).toBeInTheDocument();
  });
  
  it('renders a list of saved games when games are provided', () => {
    render(<RestoreModal open={true} handleClose={mockHandleClose} games={mockSavedGames} />);
    
    expect(screen.getByText('Saved Game 1')).toBeInTheDocument();
    expect(screen.getByText('Saved Game 2')).toBeInTheDocument();
    expect(screen.getAllByText('January 1st, 12:00 pm')).toHaveLength(2); // Due to our mock
    expect(screen.getAllByText('Restore')).toHaveLength(2);
  });
  
  it('calls handleClose with undefined when Cancel button is clicked', () => {
    render(<RestoreModal open={true} handleClose={mockHandleClose} games={mockSavedGames} />);
    
    fireEvent.click(screen.getByText('Cancel'));
    
    expect(mockHandleClose).toHaveBeenCalledWith(undefined);
  });
  
  it('calls handleClose with game id when Restore button is clicked', () => {
    render(<RestoreModal open={true} handleClose={mockHandleClose} games={mockSavedGames} />);
    
    // Click the first Restore button
    fireEvent.click(screen.getAllByText('Restore')[0]);
    
    expect(mockHandleClose).toHaveBeenCalledWith('game-1');
  });
  
  it('does not render when open is false', () => {
    render(<RestoreModal open={false} handleClose={mockHandleClose} games={mockSavedGames} />);
    
    expect(screen.queryByText('Restore A Previously Saved Game:')).not.toBeInTheDocument();
    expect(screen.queryByText('Saved Game 1')).not.toBeInTheDocument();
  });
});