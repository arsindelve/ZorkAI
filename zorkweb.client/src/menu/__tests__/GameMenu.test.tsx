import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import GameMenu from '../GameMenu';

// Mock the child components
vi.mock('../FunctionsMenu', () => ({
  default: vi.fn(({ gameMethods, forceClose }) => (
    <div data-testid="functions-menu" data-methods={gameMethods} data-force-close={forceClose}>
      FunctionsMenu
    </div>
  ))
}));

vi.mock('../AboutMenu', () => ({
  default: vi.fn(() => <div data-testid="about-menu">AboutMenu</div>)
}));

// Mock config
vi.mock('../../config.json', () => ({
  default: { version: '1.0.0-test' }
}));

describe('GameMenu', () => {
  const gameMethods = [vi.fn(), vi.fn(), vi.fn()];

  it('renders the FunctionsMenu with correct props', () => {
    render(<GameMenu gameMethods={gameMethods} forceClose={true} />);
    
    const functionsMenu = screen.getByTestId('functions-menu');
    expect(functionsMenu).toBeInTheDocument();
    expect(functionsMenu).toHaveAttribute('data-force-close', 'true');
  });
  
  it('renders the AboutMenu', () => {
    render(<GameMenu gameMethods={gameMethods} forceClose={false} />);
    
    expect(screen.getByTestId('about-menu')).toBeInTheDocument();
  });
  
  it('passes the game methods to FunctionsMenu', () => {
    render(<GameMenu gameMethods={gameMethods} forceClose={false} />);
    
    const functionsMenu = screen.getByTestId('functions-menu');
    expect(functionsMenu).toHaveAttribute('data-methods');
  });
});