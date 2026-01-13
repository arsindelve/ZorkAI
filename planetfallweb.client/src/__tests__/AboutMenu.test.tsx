import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import AboutMenu from '../menu/AboutMenu';
import { useGameContext } from '../GameContext';
import DialogType from '../model/DialogType';
import { Mixpanel } from '../Mixpanel';

// Mock the GameContext hook
jest.mock('../GameContext', () => ({
  useGameContext: jest.fn(),
}));

// Mock the Mixpanel
jest.mock('../Mixpanel', () => ({
  Mixpanel: {
    track: jest.fn(),
  },
}));

// Mock window.open
const mockOpen = jest.fn();
Object.defineProperty(window, 'open', {
  value: mockOpen,
  writable: true,
});

describe('AboutMenu Component', () => {
  const mockSetDialogToOpen = jest.fn();
  const defaultProps = {
    latestVersion: '1.0.0'
  };

  beforeEach(() => {
    // Setup mock for useGameContext
    (useGameContext as jest.Mock).mockReturnValue({
      setDialogToOpen: mockSetDialogToOpen,
    });

    // Clear all mocks
    jest.clearAllMocks();
  });

  test('renders the About button', () => {
    render(<AboutMenu {...defaultProps} />);

    const aboutButton = screen.getByTestId('about-button');
    expect(aboutButton).toBeInTheDocument();
    expect(aboutButton).toHaveTextContent('About');
  });

  test('opens the menu when button is clicked', () => {
    render(<AboutMenu {...defaultProps} />);

    // Menu should be closed initially
    expect(screen.queryByText('What is Planetfall.AI?')).not.toBeInTheDocument();

    // Click the button to open the menu
    fireEvent.click(screen.getByTestId('about-button'));

    // Menu should be open now
    expect(screen.getByText('What is Planetfall.AI?')).toBeInTheDocument();
    expect(screen.getByText('See the source code')).toBeInTheDocument();
  });

  test('opens Welcome dialog when "What is Planetfall.AI?" is clicked', () => {
    render(<AboutMenu {...defaultProps} />);

    // Open the menu
    fireEvent.click(screen.getByTestId('about-button'));

    // Click the "What is Planetfall.AI?" menu item
    fireEvent.click(screen.getByText('What is Planetfall.AI?'));

    // Check if setDialogToOpen was called with Welcome
    expect(mockSetDialogToOpen).toHaveBeenCalledWith(DialogType.Welcome);
  });

  test('opens ReleaseNotes dialog when version is clicked', () => {
    render(<AboutMenu {...defaultProps} />);

    // Open the menu
    fireEvent.click(screen.getByTestId('about-button'));

    // Find and click the version menu item (it shows "Version 1.0.0")
    const versionItem = screen.getByText('Version 1.0.0');
    fireEvent.click(versionItem);

    // Check if setDialogToOpen was called with ReleaseNotes
    expect(mockSetDialogToOpen).toHaveBeenCalledWith(DialogType.ReleaseNotes);
  });

  test('opens external link when "See the source code" is clicked', () => {
    render(<AboutMenu {...defaultProps} />);

    // Open the menu
    fireEvent.click(screen.getByTestId('about-button'));

    // Click the "See the source code" menu item
    fireEvent.click(screen.getByText('See the source code'));

    // Check if window.open was called with the correct URL
    expect(mockOpen).toHaveBeenCalledWith('https://github.com/arsindelve/ZorkAI', '_blank');

    // Check if Mixpanel.track was called
    expect(Mixpanel.track).toHaveBeenCalledWith('Click on Menu Item', {
      url: 'https://github.com/arsindelve/ZorkAI',
      name: 'Repo'
    });
  });

  test('opens external link when "Read the Original Infocom Manual" is clicked', () => {
    render(<AboutMenu {...defaultProps} />);

    // Open the menu
    fireEvent.click(screen.getByTestId('about-button'));

    // Click the menu item
    fireEvent.click(screen.getByText('Read the Original Infocom Manual'));

    // Check if window.open was called with the correct URL
    expect(mockOpen).toHaveBeenCalledWith('https://infodoc.plover.net/manuals/planetfa.pdf', '_blank');

    // Check if Mixpanel.track was called
    expect(Mixpanel.track).toHaveBeenCalledWith('Click on Menu Item', {
      url: 'https://infodoc.plover.net/manuals/planetfa.pdf',
      name: 'Planetfall Manual'
    });
  });
});
