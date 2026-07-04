import React from 'react';
import {render, screen, fireEvent, waitFor} from '@testing-library/react';
import {HintPanel} from '@zork-ai/shared-types';

describe('HintPanel Component', () => {
    const sessionId = 'test-session';
    const storageKey = `HintHistory-${sessionId}`;

    beforeEach(() => {
        localStorage.clear();
        // jsdom doesn't implement scrollTo on elements.
        Element.prototype.scrollTo = jest.fn();
    });

    const renderPanel = (ask: jest.Mock, open = true) =>
        render(<HintPanel open={open} onClose={jest.fn()} sessionId={sessionId} ask={ask}/>);

    test('renders nothing when closed', () => {
        renderPanel(jest.fn(), false);
        expect(screen.queryByTestId('hint-panel')).not.toBeInTheDocument();
    });

    test('shows the empty-state tagline and the no-turn badge when open', () => {
        renderPanel(jest.fn());
        expect(screen.getByText(/I won't judge/)).toBeInTheDocument();
        expect(screen.getByText('Costs no turn')).toBeInTheDocument();
    });

    test('sending a question calls ask and renders the exchange', async () => {
        const ask = jest.fn().mockResolvedValue('Try waiting. The ship has plans.');
        renderPanel(ask);

        fireEvent.change(screen.getByTestId('hint-input'), {target: {value: 'what do I do?'}});
        fireEvent.click(screen.getByTestId('hint-send'));

        await waitFor(() => expect(screen.getByTestId('hint-answer')).toBeInTheDocument());
        expect(ask).toHaveBeenCalledWith('what do I do?', []);
        expect(screen.getByTestId('hint-question')).toHaveTextContent('what do I do?');
        expect(screen.getByTestId('hint-answer')).toHaveTextContent('Try waiting. The ship has plans.');

        // The exchange is persisted per session — this is the client-owned stateless history.
        const stored = JSON.parse(localStorage.getItem(storageKey)!);
        expect(stored).toEqual([{question: 'what do I do?', revealed: 'Try waiting. The ship has plans.'}]);
    });

    test('a follow-up passes the prior history to ask', async () => {
        localStorage.setItem(storageKey, JSON.stringify([{question: 'q1', revealed: 'a1'}]));
        const ask = jest.fn().mockResolvedValue('a2');
        renderPanel(ask);

        fireEvent.change(screen.getByTestId('hint-input'), {target: {value: 'more'}});
        fireEvent.keyDown(screen.getByTestId('hint-input'), {key: 'Enter'});

        await waitFor(() => expect(ask).toHaveBeenCalled());
        expect(ask).toHaveBeenCalledWith('more', [{question: 'q1', revealed: 'a1'}]);
    });

    test('a failed ask shows an error, restores the question, and does NOT poison the history', async () => {
        const ask = jest.fn().mockRejectedValue(new Error('boom'));
        renderPanel(ask);

        fireEvent.change(screen.getByTestId('hint-input'), {target: {value: 'help'}});
        fireEvent.click(screen.getByTestId('hint-send'));

        await waitFor(() => expect(screen.getByTestId('hint-error')).toBeInTheDocument());
        // Question restored so the player can just hit send again...
        expect(screen.getByTestId('hint-input')).toHaveValue('help');
        // ...and the failure never entered the persisted conversation.
        expect(JSON.parse(localStorage.getItem(storageKey) ?? '[]')).toEqual([]);
        expect(screen.queryByTestId('hint-answer')).not.toBeInTheDocument();
    });

    test('loads the existing conversation for the session', () => {
        localStorage.setItem(storageKey, JSON.stringify([{question: 'old q', revealed: 'old a'}]));
        renderPanel(jest.fn());
        expect(screen.getByTestId('hint-question')).toHaveTextContent('old q');
        expect(screen.getByTestId('hint-answer')).toHaveTextContent('old a');
    });

    test('quick-ask chips send immediately', async () => {
        const ask = jest.fn().mockResolvedValue('an answer');
        renderPanel(ask);

        fireEvent.click(screen.getAllByTestId('hint-chip')[0]);

        await waitFor(() => expect(ask).toHaveBeenCalled());
        expect(ask.mock.calls[0][0].length).toBeGreaterThan(0);
    });
});
