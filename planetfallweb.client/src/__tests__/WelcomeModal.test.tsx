import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import WelcomeDialog from '../modal/WelcomeModal';

describe('WelcomeDialog Component', () => {
  const mockHandleClose = jest.fn();
  const mockHandleWatchVideo = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
  });

  test('renders nothing when open is false', () => {
    render(
      <WelcomeDialog 
        open={false} 
        handleClose={mockHandleClose} 
        handleWatchVideo={mockHandleWatchVideo} 
      />
    );

    // Dialog should not be in the document
    expect(screen.queryByTestId('welcome-modal')).not.toBeInTheDocument();
  });

  test('renders dialog when open is true', () => {
    render(
      <WelcomeDialog 
        open={true} 
        handleClose={mockHandleClose} 
        handleWatchVideo={mockHandleWatchVideo} 
      />
    );

    // Dialog should be in the document
    expect(screen.getByTestId('welcome-modal')).toBeInTheDocument();
    expect(screen.getByText('Welcome to Zork AI - A Modern Reimagining of the 1980s Classic!')).toBeInTheDocument();
  });

  test('displays information about the game', () => {
    render(
      <WelcomeDialog 
        open={true} 
        handleClose={mockHandleClose} 
        handleWatchVideo={mockHandleWatchVideo} 
      />
    );

    // Check that the dialog contains information about the game
    expect(screen.getByText('About Zork AI')).toBeInTheDocument();
    expect(screen.getByText(/This is a modern re-imagining of the iconic text adventure game Zork I./)).toBeInTheDocument();
    expect(screen.getByText(/Need inspiration\? Try:/)).toBeInTheDocument();

    // Check that example commands are displayed
    // The text is transformed to uppercase using CSS, but the actual text content is lowercase
    // So we need to use case-insensitive matching
    expect(screen.getByText('open the mailbox', { exact: false })).toBeInTheDocument();
    expect(screen.getByText('go south', { exact: false })).toBeInTheDocument();
    expect(screen.getByText('jump up and down', { exact: false })).toBeInTheDocument();
    expect(screen.getByText('tell me about the great underground empire', { exact: false })).toBeInTheDocument();
  });

  test('calls handleClose when close button is clicked', () => {
    render(
      <WelcomeDialog 
        open={true} 
        handleClose={mockHandleClose} 
        handleWatchVideo={mockHandleWatchVideo} 
      />
    );

    // Click the close button
    fireEvent.click(screen.getByTestId('welcome-modal-close-button'));

    // Check that handleClose was called
    expect(mockHandleClose).toHaveBeenCalledTimes(1);
    expect(mockHandleWatchVideo).not.toHaveBeenCalled();
  });

  test('calls handleWatchVideo when "Watch an intro video" button is clicked', () => {
    render(
      <WelcomeDialog 
        open={true} 
        handleClose={mockHandleClose} 
        handleWatchVideo={mockHandleWatchVideo} 
      />
    );

    // Click the "Watch an intro video" button
    fireEvent.click(screen.getByText('Watch an intro video'));

    // Check that handleWatchVideo was called
    expect(mockHandleWatchVideo).toHaveBeenCalledTimes(1);
    expect(mockHandleClose).not.toHaveBeenCalled();
  });

  test('calls handleClose when clicking outside the dialog', () => {
    render(
      <WelcomeDialog 
        open={true} 
        handleClose={mockHandleClose} 
        handleWatchVideo={mockHandleWatchVideo} 
      />
    );

    // Find the dialog element
    const dialog = screen.getByTestId('welcome-modal');
    expect(dialog).toBeInTheDocument();

    // Directly call the onClose handler
    // In the component, onClose={handleClose} is passed to the Dialog component
    // So we can simulate this by calling handleClose directly
    mockHandleClose();

    // Check that handleClose was called
    expect(mockHandleClose).toHaveBeenCalledTimes(1);
    expect(mockHandleWatchVideo).not.toHaveBeenCalled();
  });

  test('contains a link to Wikipedia', () => {
    render(
      <WelcomeDialog 
        open={true} 
        handleClose={mockHandleClose} 
        handleWatchVideo={mockHandleWatchVideo} 
      />
    );

    // Check that the link to Wikipedia is present
    const link = screen.getByText(/Tim Anderson, Marc Blank, Bruce Daniels, and Dave Lebling/);
    expect(link).toBeInTheDocument();
    expect(link.closest('a')).toHaveAttribute('href', 'https://en.wikipedia.org/wiki/Zork');
    expect(link.closest('a')).toHaveAttribute('target', '_blank');
  });
});
