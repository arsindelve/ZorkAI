import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import SaveModal from '../SaveModal';
import { ISavedGame } from '../../model/SavedGame';
import { SaveGameRequest } from '../../model/SaveGameRequest';

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

describe('SaveModal', () => {
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
    render(<SaveModal open={true} handleClose={mockHandleClose} games={[]} />);
    
    expect(screen.getByText('Save Your Game')).toBeInTheDocument();
  });
  
  it('renders the input field for new game name', () => {
    render(<SaveModal open={true} handleClose={mockHandleClose} games={[]} />);
    
    expect(screen.getByText('Name your saved game:')).toBeInTheDocument();
    expect(screen.getByRole('textbox')).toBeInTheDocument();
    expect(screen.getByText('Save')).toBeInTheDocument();
  });
  
  it('updates the input value when typing', () => {
    render(<SaveModal open={true} handleClose={mockHandleClose} games={[]} />);
    
    const input = screen.getByRole('textbox');
    fireEvent.change(input, { target: { value: 'My New Save' } });
    
    expect(input).toHaveValue('My New Save');
  });
  
  it('calls handleClose with new save game request when Save button is clicked', () => {
    render(<SaveModal open={true} handleClose={mockHandleClose} games={[]} />);
    
    const input = screen.getByRole('textbox');
    fireEvent.change(input, { target: { value: 'My New Save' } });
    
    fireEvent.click(screen.getByText('Save'));
    
    expect(mockHandleClose).toHaveBeenCalledWith(expect.objectContaining({
      name: 'My New Save',
      id: undefined,
      sessionId: undefined,
      clientId: undefined
    }));
  });
  
  it('calls handleClose with new save game request when Enter key is pressed', () => {
    render(<SaveModal open={true} handleClose={mockHandleClose} games={[]} />);
    
    const input = screen.getByRole('textbox');
    fireEvent.change(input, { target: { value: 'My New Save' } });
    
    fireEvent.keyDown(input, { key: 'Enter' });
    
    expect(mockHandleClose).toHaveBeenCalledWith(expect.objectContaining({
      name: 'My New Save'
    }));
  });
  
  it('renders a list of existing saved games when games are provided', () => {
    render(<SaveModal open={true} handleClose={mockHandleClose} games={mockSavedGames} />);
    
    expect(screen.getByText('Overwrite a previously saved game:')).toBeInTheDocument();
    expect(screen.getByText('Saved Game 1')).toBeInTheDocument();
    expect(screen.getByText('Saved Game 2')).toBeInTheDocument();
    expect(screen.getAllByText('January 1st, 12:00 pm')).toHaveLength(2); // Due to our mock
    expect(screen.getAllByText('Overwrite')).toHaveLength(2);
  });
  
  it('calls handleClose with existing game id when Overwrite button is clicked', () => {
    render(<SaveModal open={true} handleClose={mockHandleClose} games={mockSavedGames} />);
    
    // Click the first Overwrite button
    fireEvent.click(screen.getAllByText('Overwrite')[0]);
    
    expect(mockHandleClose).toHaveBeenCalledWith(expect.objectContaining({
      name: 'Saved Game 1',
      id: 'game-1'
    }));
  });
  
  it('calls handleClose with undefined when Cancel button is clicked', () => {
    render(<SaveModal open={true} handleClose={mockHandleClose} games={mockSavedGames} />);
    
    fireEvent.click(screen.getByText('Cancel'));
    
    expect(mockHandleClose).toHaveBeenCalledWith(undefined);
  });
  
  it('does not render when open is false', () => {
    render(<SaveModal open={false} handleClose={mockHandleClose} games={mockSavedGames} />);
    
    expect(screen.queryByText('Save Your Game')).not.toBeInTheDocument();
    expect(screen.queryByText('Name your saved game:')).not.toBeInTheDocument();
  });
});