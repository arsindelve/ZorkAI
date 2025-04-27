import React from 'react';
import { render, screen, waitFor, fireEvent } from '@testing-library/react';
import ReleaseNotesModal from '../modal/ReleaseNotesModal';
import { ReleaseNotesServer } from '../ReleaseNotesServer';

// Mock the ReleaseNotesServer
jest.mock('../ReleaseNotesServer', () => ({
  ReleaseNotesServer: jest.fn(),
}));

describe('ReleaseNotesModal Component', () => {
  const mockHandleClose = jest.fn();
  const mockReleases = [
    {
      date: '2023-01-15T00:00:00.000Z',
      name: 'Version 1.2.0',
      notes: '<ul><li>Added new feature A</li><li>Fixed bug B</li></ul>'
    },
    {
      date: '2022-12-01T00:00:00.000Z',
      name: 'Version 1.1.0',
      notes: '<ul><li>Initial release</li></ul>'
    }
  ];

  beforeEach(() => {
    jest.clearAllMocks();
    // Default mock implementation returns the mock releases after a short delay
    (ReleaseNotesServer as jest.Mock).mockImplementation(() => 
      new Promise(resolve => {
        setTimeout(() => resolve(mockReleases), 100);
      })
    );
  });

  test('renders nothing when open is false', () => {
    render(<ReleaseNotesModal open={false} handleClose={mockHandleClose} />);

    // Dialog should not be in the document
    expect(screen.queryByText('Zork AI Release Notes')).not.toBeInTheDocument();
  });

  test('renders dialog when open is true', () => {
    render(<ReleaseNotesModal open={true} handleClose={mockHandleClose} />);

    // Dialog title should be in the document
    expect(screen.getByText('Zork AI Release Notes')).toBeInTheDocument();
  });

  test('shows loading skeletons while fetching data', () => {
    render(<ReleaseNotesModal open={true} handleClose={mockHandleClose} />);

    // Should show loading skeletons initially
    const skeletons = document.querySelectorAll('[class*="MuiSkeleton-root"]');
    expect(skeletons.length).toBeGreaterThan(0);
  });

  test('displays release notes after fetching', async () => {
    render(<ReleaseNotesModal open={true} handleClose={mockHandleClose} />);

    // Wait for the release notes to be displayed
    await waitFor(() => {
      expect(screen.getByText('Version 1.2.0')).toBeInTheDocument();
    });

    // Check that both releases are displayed
    expect(screen.getByText('Version 1.2.0')).toBeInTheDocument();
    expect(screen.getByText('Version 1.1.0')).toBeInTheDocument();

    // Check that the dates are displayed
    // Instead of checking for specific date formats which depend on locale,
    // we'll check that the dates are displayed in some format
    const date1 = new Date('2023-01-15T00:00:00.000Z').toLocaleDateString();
    const date2 = new Date('2022-12-01T00:00:00.000Z').toLocaleDateString();
    expect(screen.getByText(date1)).toBeInTheDocument();
    expect(screen.getByText(date2)).toBeInTheDocument();

    // Check that the notes content is rendered
    const releaseNotes = document.querySelectorAll('ul');
    expect(releaseNotes.length).toBe(2);
  });

  test('calls handleClose when close button is clicked', async () => {
    render(<ReleaseNotesModal open={true} handleClose={mockHandleClose} />);

    // Wait for the release notes to be displayed
    await waitFor(() => {
      expect(screen.getByText('Version 1.2.0')).toBeInTheDocument();
    });

    // Click the close button
    fireEvent.click(screen.getByText('Close'));

    // Check that handleClose was called
    expect(mockHandleClose).toHaveBeenCalledTimes(1);
  });

  test('fetches data when opened', () => {
    render(<ReleaseNotesModal open={true} handleClose={mockHandleClose} />);

    // Check that ReleaseNotesServer was called
    expect(ReleaseNotesServer).toHaveBeenCalledTimes(1);
  });

  test('handles error when fetching data', async () => {
    // Mock ReleaseNotesServer to return an empty array (simulating an error)
    (ReleaseNotesServer as jest.Mock).mockResolvedValue([]);

    render(<ReleaseNotesModal open={true} handleClose={mockHandleClose} />);

    // Wait for the component to finish rendering
    await waitFor(() => {
      expect(ReleaseNotesServer).toHaveBeenCalledTimes(1);
    });

    // No release notes should be displayed
    expect(screen.queryByText('Version 1.2.0')).not.toBeInTheDocument();
  });
});
