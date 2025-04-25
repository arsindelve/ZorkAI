import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import Game from '../Game';
import { SessionHandler } from '../SessionHandler';

// Mock dependencies
vi.mock('../Server', () => ({
  default: vi.fn().mockImplementation(() => ({
    gameInit: vi.fn().mockResolvedValue({
      response: 'Initial game text',
      locationName: 'West of House',
      score: 0,
      moves: 0
    }),
    gameInput: vi.fn().mockResolvedValue({
      response: 'Response to input',
      locationName: 'West of House',
      score: 10,
      moves: 1
    }),
    gameRestore: vi.fn().mockResolvedValue({
      response: 'Game restored',
      locationName: 'Inside House',
      score: 20,
      moves: 5
    })
  }))
}));

vi.mock('../SessionHandler', () => ({
  SessionHandler: vi.fn().mockImplementation(() => ({
    getSessionId: vi.fn().mockReturnValue(['session-id', false]),
    regenerate: vi.fn(),
    getClientId: vi.fn().mockReturnValue('client-id')
  }))
}));

vi.mock('../modal/WelcomeModal', () => ({
  default: vi.fn(({ open, handleClose }) => (
    <div data-testid="welcome-modal" data-open={open}>
      {open && <button onClick={handleClose}>Close Welcome</button>}
    </div>
  ))
}));

vi.mock('../Header', () => ({
  default: vi.fn(({ locationName, moves, score }) => (
    <div data-testid="header" data-location={locationName} data-moves={moves} data-score={score}>
      Header Component
    </div>
  ))
}));

// Mock the useMutation hook
vi.mock('@tanstack/react-query', () => ({
  useMutation: vi.fn().mockReturnValue({
    mutate: vi.fn(),
    isPending: false,
    isError: false
  })
}));

describe('Game', () => {
  const defaultProps = {
    restartGame: false,
    restoreGameId: undefined,
    serverText: '',
    gaveSaved: false,
    onRestoreDone: vi.fn(),
    onRestartDone: vi.fn(),
    openRestoreModal: vi.fn(),
    openSaveModal: vi.fn(),
    openRestartModal: vi.fn()
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders the game component with initial loading text', () => {
    render(<Game {...defaultProps} />);
    
    expect(screen.getByText('Your game is loading....')).toBeInTheDocument();
    expect(screen.getByPlaceholderText('Type what you want to do, then press return.')).toBeInTheDocument();
  });

  it('renders the header with correct props', async () => {
    render(<Game {...defaultProps} />);
    
    await waitFor(() => {
      const header = screen.getByTestId('header');
      expect(header).toBeInTheDocument();
      expect(header).toHaveAttribute('data-location', 'West of House');
      expect(header).toHaveAttribute('data-score', '0');
      expect(header).toHaveAttribute('data-moves', '0');
    });
  });

  it('renders the welcome modal when it is a first-time session', () => {

    // Override the mock to simulate first-time session
    vi.mocked(SessionHandler).mockImplementationOnce(() => ({
      getSessionId: vi.fn().mockReturnValue(['session-id', true]),
      regenerate: vi.fn(),
      getClientId: vi.fn().mockReturnValue('client-id'),
      generateRandomString: vi.fn().mockReturnValue('random-string')
    }));

    render(<Game {...defaultProps} />);
    
    const welcomeModal = screen.getByTestId('welcome-modal');
    expect(welcomeModal).toBeInTheDocument();
    expect(welcomeModal).toHaveAttribute('data-open', 'true');
  });

  it('allows user to input text', () => {
    render(<Game {...defaultProps} />);
    
    const input = screen.getByPlaceholderText('Type what you want to do, then press return.');
    fireEvent.change(input, { target: { value: 'go north' } });
    
    expect(input).toHaveValue('go north');
  });

  it('shows a snackbar when game is saved', async () => {
    render(<Game {...defaultProps} gaveSaved={true} />);
    
    await waitFor(() => {
      expect(screen.getByText('Game Saved Successfully.')).toBeInTheDocument();
    });
  });

  it('calls onRestartDone when restart is complete', async () => {
    render(<Game {...defaultProps} restartGame={true} />);
    
    await waitFor(() => {
      expect(defaultProps.onRestartDone).toHaveBeenCalled();
    });
  });

  it('calls onRestoreDone when restore is complete', async () => {
    render(<Game {...defaultProps} restoreGameId="game-id" />);
    
    await waitFor(() => {
      expect(defaultProps.onRestoreDone).toHaveBeenCalled();
    });
  });

  it('updates game text when serverText prop changes', async () => {
    const { rerender } = render(<Game {...defaultProps} />);
    
    // Update with new server text
    rerender(<Game {...defaultProps} serverText="New text from server" />);
    
    await waitFor(() => {
      expect(screen.getByText('New text from server')).toBeInTheDocument();
    });
  });
});