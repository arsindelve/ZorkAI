import React from 'react';
import { render, screen, fireEvent, act } from '@testing-library/react';
import CommandsButton from '../components/CommandsButton';

// Mock the Mixpanel module
jest.mock('../Mixpanel.ts', () => ({
  Mixpanel: {
    track: jest.fn()
  }
}));

describe('CommandsButton Component', () => {
  const mockOnCommandClick = jest.fn();

  beforeEach(() => {
    // Clear mock function calls before each test
    mockOnCommandClick.mockClear();
    // Setup fake timers for all tests
    jest.useFakeTimers();
  });

  afterEach(() => {
    // Cleanup timers after each test
    jest.useRealTimers();
  });

  test('renders the commands button', () => {
    render(<CommandsButton onCommandClick={mockOnCommandClick} />);

    // Initially the button might not be visible due to loading state
    // Use act to wait for the useEffect to complete
    act(() => {
      // Advance timers if needed
      jest.advanceTimersByTime(0);
    });

    // Now the button should be visible
    const buttonElement = screen.getByText('Commands');
    expect(buttonElement).toBeInTheDocument();
  });

  test('opens menu when clicked', () => {
    render(<CommandsButton onCommandClick={mockOnCommandClick} />);

    // Wait for loading state
    act(() => {
      jest.advanceTimersByTime(0);
    });

    // Click the button to open the menu
    const buttonElement = screen.getByText('Commands');
    fireEvent.click(buttonElement);

    // Check if menu items are displayed (Sentence Case)
    expect(screen.getByText('Verbose')).toBeInTheDocument();
    expect(screen.getByText('Diagnose')).toBeInTheDocument();
    expect(screen.getByText('Inventory')).toBeInTheDocument();
    // Check a few more to ensure the menu is populated correctly
    expect(screen.getByText('Look')).toBeInTheDocument();
  });

  test('calls onCommandClick when a command is selected', () => {
    render(<CommandsButton onCommandClick={mockOnCommandClick} />);

    // Wait for loading state
    act(() => {
      jest.advanceTimersByTime(0);
    });

    // Click the button to open the menu
    const buttonElement = screen.getByText('Commands');
    fireEvent.click(buttonElement);

    // Click a command in the menu (displayed in Sentence Case)
    const inventoryCommand = screen.getByText('Inventory');
    fireEvent.click(inventoryCommand);

    // Check if onCommandClick was called with the original lowercase command
    expect(mockOnCommandClick).toHaveBeenCalledWith('inventory');
  });

  test('closes menu after selecting a command', () => {
    render(<CommandsButton onCommandClick={mockOnCommandClick} />);

    // Wait for loading state
    act(() => {
      jest.advanceTimersByTime(0);
    });

    // Click the button to open the menu
    const buttonElement = screen.getByText('Commands');
    fireEvent.click(buttonElement);

    // Click a command in the menu (displayed in Sentence Case)
    const lookCommand = screen.getByText('Look');
    fireEvent.click(lookCommand);

    // Menu should be closed, so the command should no longer be visible
    expect(screen.queryByText('Look')).not.toBeVisible();
  });

  test('button is enabled after loading', () => {
    render(<CommandsButton onCommandClick={mockOnCommandClick} />);

    // Run useEffect to set isLoaded to true
    act(() => {
      jest.runAllTimers();
    });

    // Get the button element
    const buttonElement = screen.getByText('Commands');

    // After useEffect runs, button should be enabled
    expect(buttonElement).not.toBeDisabled();

    // Check that the button has the expected styling for loaded state
    const buttonStyles = window.getComputedStyle(buttonElement);
    expect(buttonElement).toBeVisible();
  });
});
