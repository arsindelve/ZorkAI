import React from 'react';
import { render, screen } from '@testing-library/react';
import GameMenu from '../menu/GameMenu';

// Mock the child components
jest.mock('../menu/AboutMenu', () => ({
  __esModule: true,
  default: function MockAboutMenu() {
    return <div data-testid="about-menu-mock">About Menu</div>;
  }
}));

jest.mock('../menu/FunctionsMenu', () => ({
  __esModule: true,
  default: function MockFunctionsMenu() {
    return <div data-testid="functions-menu-mock">Functions Menu</div>;
  }
}));

describe('GameMenu Component', () => {
  const defaultProps = {
    latestVersion: '1.0.0'
  };

  beforeEach(() => {
    jest.clearAllMocks();
  });

  test('renders the logo', () => {
    render(<GameMenu {...defaultProps} />);

    const logo = screen.getByAltText('Planetfall');
    expect(logo).toBeInTheDocument();
    expect(logo).toHaveAttribute('src', '/logo.png');
  });

  test('renders the AboutMenu component', () => {
    render(<GameMenu {...defaultProps} />);

    const aboutMenu = screen.getByTestId('about-menu-mock');
    expect(aboutMenu).toBeInTheDocument();
  });

  test('renders the FunctionsMenu component', () => {
    render(<GameMenu {...defaultProps} />);

    const functionsMenu = screen.getByTestId('functions-menu-mock');
    expect(functionsMenu).toBeInTheDocument();
  });

  test('applies animation classes correctly after loading', () => {
    render(<GameMenu {...defaultProps} />);

    // Get the main container
    const container = screen.getByTestId('game-menu-container');

    // After render and useEffect, the menu should have translate-y-0 class
    expect(container.className).toContain('translate-y-0');
  });
});
