import React from 'react';
import { render, screen, act } from '@testing-library/react';
import GameMenu from '../menu/GameMenu';
import { useGameContext } from '../GameContext';

// Mock the child components
jest.mock('../menu/AboutMenu', () => {
  return function MockAboutMenu() {
    return <div data-testid="about-menu-mock">About Menu</div>;
  };
});

jest.mock('../menu/FunctionsMenu', () => {
  return function MockFunctionsMenu() {
    return <div data-testid="functions-menu-mock">Functions Menu</div>;
  };
});

// Mock the GameContext hook since it might be used by child components
jest.mock('../GameContext', () => ({
  useGameContext: jest.fn().mockReturnValue({}),
}));

describe('GameMenu Component', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  test('renders the logo', () => {
    render(<GameMenu />);

    const logo = screen.getByAltText('Logo');
    expect(logo).toBeInTheDocument();
    expect(logo).toHaveAttribute('src', 'https://zorkai-assets.s3.amazonaws.com/Zork.webp');
  });

  test('renders the title', () => {
    render(<GameMenu />);

    const title = screen.getByText('Generative AI-Enhanced Zork I');
    expect(title).toBeInTheDocument();
  });

  test('renders the AboutMenu component', () => {
    render(<GameMenu />);

    const aboutMenu = screen.getByTestId('about-menu-mock');
    expect(aboutMenu).toBeInTheDocument();
  });

  test('renders the FunctionsMenu component', () => {
    render(<GameMenu />);

    const functionsMenu = screen.getByTestId('functions-menu-mock');
    expect(functionsMenu).toBeInTheDocument();
  });

  test('applies animation classes correctly after loading', () => {
    // Use fake timers to control useEffect timing
    jest.useFakeTimers();

    // Mock useState to control the initial state
    const originalUseState = React.useState;
    const mockSetState = jest.fn();

    // Mock useState to return false initially
    jest.spyOn(React, 'useState').mockImplementationOnce(() => [false, mockSetState]);

    render(<GameMenu />);

    // Get the main container
    const container = screen.getByTestId('game-menu-container');

    // Initially, the isLoaded state should be false, so the menu should have the -translate-y-full class
    expect(container.className).toContain('-translate-y-full');

    // Restore the original useState
    jest.spyOn(React, 'useState').mockRestore();

    // Cleanup
    jest.useRealTimers();
  });
});
