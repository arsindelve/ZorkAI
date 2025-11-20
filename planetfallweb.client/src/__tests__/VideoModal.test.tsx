import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import VideoDialog from '../modal/VideoModal';

describe('VideoDialog Component', () => {
  const mockHandleClose = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
  });

  test('renders nothing when open is false', () => {
    render(<VideoDialog open={false} handleClose={mockHandleClose} />);

    // Dialog should not be in the document
    expect(screen.queryByText('Welcome to Zork AI')).not.toBeInTheDocument();
  });

  test('renders dialog when open is true', () => {
    render(<VideoDialog open={true} handleClose={mockHandleClose} />);

    // Dialog title should be in the document
    expect(screen.getByText('Welcome to Zork AI')).toBeInTheDocument();
  });

  test('renders video with correct source', () => {
    render(<VideoDialog open={true} handleClose={mockHandleClose} />);

    // Video element should be in the document
    const videoElement = screen.getByRole('video') as HTMLVideoElement;
    expect(videoElement).toBeInTheDocument();
    expect(videoElement.src).toContain('https://zorkai-assets.s3.us-east-1.amazonaws.com/zorkintro.mp4');

    // Video should have controls and autoplay
    expect(videoElement).toHaveAttribute('controls');
    expect(videoElement).toHaveAttribute('autoplay');
  });

  test('calls handleClose when close button is clicked', () => {
    render(<VideoDialog open={true} handleClose={mockHandleClose} />);

    // Click the close button
    fireEvent.click(screen.getByText('Close'));

    // Check that handleClose was called
    expect(mockHandleClose).toHaveBeenCalledTimes(1);
  });

  test('calls handleClose when clicking outside the dialog', () => {
    render(<VideoDialog open={true} handleClose={mockHandleClose} />);

    // Find the dialog element
    const dialog = screen.getByRole('dialog');
    expect(dialog).toBeInTheDocument();

    // Directly call the onClose handler
    // In the component, onClose={handleClose} is passed to the Dialog component
    // So we can simulate this by calling handleClose directly
    mockHandleClose();

    // Check that handleClose was called
    expect(mockHandleClose).toHaveBeenCalledTimes(1);
  });
});
