import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import App from '../App';

// Mock child components
vi.mock('../Game', () => ({
  default: vi.fn((props) => (
    <div data-testid="game-component" data-props={JSON.stringify(props)}>
      Game Component
      <button onClick={() => props.onRestartDone()}>Trigger onRestartDone</button>
      <button onClick={() => props.onRestoreDone()}>Trigger onRestoreDone</button>
    </div>
  ))
}));

vi.mock('../menu/GameMenu', () => ({
  default: vi.fn(({ gameMethods, forceClose }) => (
    <div data-testid="game-menu" data-force-close={forceClose}>
      Game Menu
      <button onClick={() => gameMethods[0]()}>Restart</button>
      <button onClick={() => gameMethods[1]()}>Restore</button>
      <button onClick={() => gameMethods[2]()}>Save</button>
    </div>
  ))
}));

vi.mock('../modal/ConfirmationDialog', () => ({
  default: vi.fn(({ title, open, setOpen, onConfirm }) => (
    <div data-testid="confirm-dialog" data-open={open} data-title={title}>
      {open && (
        <>
          <button onClick={() => setOpen(false)}>Cancel</button>
          <button onClick={() => onConfirm()}>Confirm</button>
        </>
      )}
    </div>
  ))
}));

vi.mock('../modal/RestoreModal', () => ({
  default: vi.fn(({ games, open, handleClose }) => (
    <div data-testid="restore-modal" data-open={open} data-games={JSON.stringify(games)}>
      {open && (
        <>
          <button onClick={() => handleClose(undefined)}>Cancel</button>
          <button onClick={() => handleClose('game-id-1')}>Restore Game 1</button>
        </>
      )}
    </div>
  ))
}));

vi.mock('../modal/SaveModal', () => ({
  default: vi.fn(({ games, open, handleClose }) => (
    <div data-testid="save-modal" data-open={open} data-games={JSON.stringify(games)}>
      {open && (
        <>
          <button onClick={() => handleClose(undefined)}>Cancel</button>
          <button onClick={() => handleClose({ name: 'Test Save', description: 'Test Description' })}>Save Game</button>
        </>
      )}
    </div>
  ))
}));

vi.mock('../Server', () => ({
  default: vi.fn().mockImplementation(() => ({
    getSavedGames: vi.fn().mockResolvedValue([
      { id: 'game-id-1', name: 'Save 1', description: 'Description 1', date: '2023-01-01' }
    ]),
    saveGame: vi.fn().mockResolvedValue('Game saved successfully')
  }))
}));

vi.mock('../SessionHandler', () => ({
  SessionHandler: vi.fn().mockImplementation(() => ({
    getClientId: vi.fn().mockReturnValue('client-id'),
    getSessionId: vi.fn().mockReturnValue(['session-id', false])
  }))
}));

// Mock QueryClient
vi.mock('@tanstack/react-query', () => ({
  QueryClient: vi.fn().mockImplementation(() => ({})),
  QueryClientProvider: vi.fn(({ children }) => <div data-testid="query-provider">{children}</div>)
}));

describe('App', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders the App component with GameMenu and Game', () => {
    render(<App />);
    
    expect(screen.getByTestId('game-menu')).toBeInTheDocument();
    expect(screen.getByTestId('game-component')).toBeInTheDocument();
    expect(screen.getByTestId('query-provider')).toBeInTheDocument();
  });

  it('opens the confirmation dialog when restart is clicked', () => {
    render(<App />);
    
    const restartButton = screen.getByText('Restart');
    fireEvent.click(restartButton);
    
    const confirmDialog = screen.getByTestId('confirm-dialog');
    expect(confirmDialog).toHaveAttribute('data-open', 'true');
    expect(confirmDialog).toHaveAttribute('data-title', 'Restart Your Game? Are you sure? ');
  });

  it('sets isRestarting to true when restart is confirmed', () => {
    render(<App />);
    
    // Click restart button to open dialog
    fireEvent.click(screen.getByText('Restart'));
    
    // Confirm restart
    fireEvent.click(screen.getByText('Confirm'));
    
    // Check if Game component received the restartGame prop as true
    const gameComponent = screen.getByTestId('game-component');
    const props = JSON.parse(gameComponent.getAttribute('data-props') || '{}');
    expect(props.restartGame).toBe(true);
  });

  it('opens the restore modal when restore is clicked', async () => {
    render(<App />);
    
    const restoreButton = screen.getByText('Restore');
    fireEvent.click(restoreButton);
    
    await waitFor(() => {
      const restoreModal = screen.getByTestId('restore-modal');
      expect(restoreModal).toHaveAttribute('data-open', 'true');
      
      // Check if saved games were fetched
      const games = JSON.parse(restoreModal.getAttribute('data-games') || '[]');
      expect(games).toHaveLength(1);
      expect(games[0].id).toBe('game-id-1');
    });
  });

  it('sets restoreGameId when a game is selected for restore', async () => {
    render(<App />);
    
    // Open restore modal
    fireEvent.click(screen.getByText('Restore'));
    
    await waitFor(() => {
      // Select a game to restore
      fireEvent.click(screen.getByText('Restore Game 1'));
      
      // Check if Game component received the restoreGameId prop
      const gameComponent = screen.getByTestId('game-component');
      const props = JSON.parse(gameComponent.getAttribute('data-props') || '{}');
      expect(props.restoreGameId).toBe('game-id-1');
    });
  });

  it('opens the save modal when save is clicked', async () => {
    render(<App />);
    
    const saveButton = screen.getByText('Save');
    fireEvent.click(saveButton);
    
    await waitFor(() => {
      const saveModal = screen.getByTestId('save-modal');
      expect(saveModal).toHaveAttribute('data-open', 'true');
    });
  });

  it('resets confirmation dialog when onRestartDone is called', () => {
    render(<App />);
    
    // Open restart dialog
    fireEvent.click(screen.getByText('Restart'));
    
    // Confirm restart
    fireEvent.click(screen.getByText('Confirm'));
    
    // Trigger onRestartDone
    fireEvent.click(screen.getByText('Trigger onRestartDone'));
    
    // Check if dialog is closed
    const confirmDialog = screen.getByTestId('confirm-dialog');
    expect(confirmDialog).toHaveAttribute('data-open', 'false');
    
    // Check if GameMenu forceClose is true
    const gameMenu = screen.getByTestId('game-menu');
    expect(gameMenu).toHaveAttribute('data-force-close', 'true');
  });

  it('resets restoreGameId when onRestoreDone is called', async () => {
    render(<App />);
    
    // Open restore modal
    fireEvent.click(screen.getByText('Restore'));
    
    await waitFor(() => {
      // Select a game to restore
      fireEvent.click(screen.getByText('Restore Game 1'));
      
      // Trigger onRestoreDone
      fireEvent.click(screen.getByText('Trigger onRestoreDone'));
      
      // Check if restoreGameId is reset
      const gameComponent = screen.getByTestId('game-component');
      const props = JSON.parse(gameComponent.getAttribute('data-props') || '{}');
      expect(props.restoreGameId).toBeUndefined();
    });
  });
});