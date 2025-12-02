import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import ReleaseNotesModal from '../modal/ReleaseNotesModal';

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
  });

  test('renders nothing when open is false', () => {
    render(<ReleaseNotesModal open={false} handleClose={mockHandleClose} releases={mockReleases} />);

    // Dialog should not be in the document
    expect(screen.queryByText('Zork AI Release Notes')).not.toBeInTheDocument();
  });

  test('renders dialog when open is true', () => {
    render(<ReleaseNotesModal open={true} handleClose={mockHandleClose} releases={mockReleases} />);

    // Dialog title should be in the document
    expect(screen.getByText('Zork AI Release Notes')).toBeInTheDocument();
  });

  test('shows loading skeletons when releases array is empty', () => {
    render(<ReleaseNotesModal open={true} handleClose={mockHandleClose} releases={[]} />);

    // Should show loading skeletons when no releases
    const skeletons = document.querySelectorAll('[class*="MuiSkeleton-root"]');
    expect(skeletons.length).toBeGreaterThan(0);
  });

  test('displays release notes when releases provided', () => {
    render(<ReleaseNotesModal open={true} handleClose={mockHandleClose} releases={mockReleases} />);

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

  test('calls handleClose when close button is clicked', () => {
    render(<ReleaseNotesModal open={true} handleClose={mockHandleClose} releases={mockReleases} />);

    // Click the close button
    fireEvent.click(screen.getByText('Close'));

    // Check that handleClose was called
    expect(mockHandleClose).toHaveBeenCalledTimes(1);
  });

  test('handles empty releases array', () => {
    render(<ReleaseNotesModal open={true} handleClose={mockHandleClose} releases={[]} />);

    // No release notes should be displayed
    expect(screen.queryByText('Version 1.2.0')).not.toBeInTheDocument();

    // Should show loading skeletons instead
    const skeletons = document.querySelectorAll('[class*="MuiSkeleton-root"]');
    expect(skeletons.length).toBeGreaterThan(0);
  });
});
