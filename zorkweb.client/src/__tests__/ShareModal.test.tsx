import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import ShareModal from '../modal/ShareModal';

// Mock SessionHandler
jest.mock('../SessionHandler', () => ({
    SessionHandler: jest.fn().mockImplementation(() => ({
        getSessionId: () => ['ABC123DEF456GHI', false]
    }))
}));

// Mock clipboard API
Object.assign(navigator, {
    clipboard: {
        writeText: jest.fn().mockImplementation(() => Promise.resolve()),
    },
});

describe('ShareModal', () => {
    const mockSetOpen = jest.fn();

    beforeEach(() => {
        jest.clearAllMocks();
    });

    test('renders share modal with session ID', () => {
        render(
            <ShareModal 
                open={true}
                setOpen={mockSetOpen}
            />
        );

        expect(screen.getByTestId('share-game-modal')).toBeInTheDocument();
        expect(screen.getByText('Share Your Game')).toBeInTheDocument();
        expect(screen.getByTestId('session-id-display')).toHaveTextContent('ABC123DEF456GHI');
        expect(screen.getByText('Share your Session ID with others so they can load copies of your saved games.')).toBeInTheDocument();
    });

    test('copies session ID to clipboard when copy button is clicked', async () => {
        render(
            <ShareModal 
                open={true}
                setOpen={mockSetOpen}
            />
        );

        const copyButton = screen.getByTestId('copy-session-id-button');
        fireEvent.click(copyButton);

        await waitFor(() => {
            expect(navigator.clipboard.writeText).toHaveBeenCalledWith('ABC123DEF456GHI');
        });

        expect(screen.getByText('Session ID copied to clipboard!')).toBeInTheDocument();
    });

    test('closes modal when close button is clicked', () => {
        render(
            <ShareModal 
                open={true}
                setOpen={mockSetOpen}
            />
        );

        const closeButton = screen.getByText('Close');
        fireEvent.click(closeButton);

        expect(mockSetOpen).toHaveBeenCalledWith(false);
    });

    test('does not render modal when open is false', () => {
        render(
            <ShareModal 
                open={false}
                setOpen={mockSetOpen}
            />
        );

        expect(screen.queryByTestId('share-game-modal')).not.toBeInTheDocument();
    });

    test('handles clipboard copy failure gracefully', async () => {
        const consoleErrorSpy = jest.spyOn(console, 'error').mockImplementation(() => {});
        (navigator.clipboard.writeText as jest.Mock).mockRejectedValueOnce(new Error('Clipboard error'));

        render(
            <ShareModal 
                open={true}
                setOpen={mockSetOpen}
            />
        );

        const copyButton = screen.getByTestId('copy-session-id-button');
        fireEvent.click(copyButton);

        await waitFor(() => {
            expect(consoleErrorSpy).toHaveBeenCalledWith('Failed to copy session ID:', expect.any(Error));
        });

        consoleErrorSpy.mockRestore();
    });
});