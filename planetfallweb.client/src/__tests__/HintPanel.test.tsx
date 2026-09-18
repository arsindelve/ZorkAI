import React from 'react';
import {act, render, screen, fireEvent, waitFor} from '@testing-library/react';
import {HintPanel} from '@zork-ai/shared-types';

describe('HintPanel Component', () => {
    const sessionId = 'test-session';
    beforeEach(() => {
        localStorage.clear();
        // jsdom doesn't implement scrollTo on elements.
        Element.prototype.scrollTo = jest.fn();
    });

    const renderPanel = (ask: jest.Mock, open = true) =>
        render(<HintPanel open={open} onClose={jest.fn()} sessionId={sessionId} ask={ask} />);

    test('renders nothing when closed', () => {
        renderPanel(jest.fn(), false);
        expect(screen.queryByTestId('hint-panel')).not.toBeInTheDocument();
    });

    test('shows the empty-state tagline when open', () => {
        renderPanel(jest.fn());
        expect(screen.getByText("Stuck? Ask me anything. I won't judge. Much.")).toBeInTheDocument();
        expect(screen.queryByText(/Ask about the room/)).not.toBeInTheDocument();
        expect(screen.queryByText('Costs no turn')).not.toBeInTheDocument();
    });

    test('uses one consistent UI font throughout the hint panel', async () => {
        const ask = jest.fn().mockResolvedValue({text: 'Try waiting.'});
        renderPanel(ask);

        const uiFont = "Roboto, system-ui, -apple-system, 'Segoe UI', Helvetica, Arial, sans-serif";

        expect(screen.getByTestId('hint-panel')).toHaveStyle({
            fontFamily: uiFont,
        });
        expect(screen.getByTestId('hint-input')).toHaveStyle({fontFamily: uiFont});

        fireEvent.change(screen.getByTestId('hint-input'), {target: {value: 'what now?'}});
        fireEvent.click(screen.getByTestId('hint-send'));
        await waitFor(() => expect(screen.getByTestId('hint-answer')).toBeInTheDocument());

        expect(screen.getByTestId('hint-answer').style.fontFamily).toBe('');
    });

    test('renders narrator emphasis instead of exposing markdown markers', async () => {
        const ask = jest.fn().mockResolvedValue({text: 'Try **waiting** by the door.'});
        renderPanel(ask);

        fireEvent.change(screen.getByTestId('hint-input'), {target: {value: 'what now?'}});
        fireEvent.click(screen.getByTestId('hint-send'));

        await waitFor(() => expect(screen.getByText('waiting').tagName).toBe('STRONG'));
        expect(screen.getByTestId('hint-answer')).not.toHaveTextContent('**');
    });

    test('sending a question calls ask and renders the exchange', async () => {
        const ask = jest.fn().mockResolvedValue({text: 'Try waiting. The ship has plans.'});
        renderPanel(ask);

        fireEvent.change(screen.getByTestId('hint-input'), {target: {value: 'what do I do?'}});
        fireEvent.click(screen.getByTestId('hint-send'));

        await waitFor(() => expect(screen.getByTestId('hint-answer')).toBeInTheDocument());
        expect(ask).toHaveBeenCalledWith('what do I do?', []);
        expect(screen.getByTestId('hint-question')).toHaveTextContent('what do I do?');
        expect(screen.getByTestId('hint-answer')).toHaveTextContent(
            'Try waiting. The ship has plans.',
        );
    });

    test('a follow-up passes the prior history to ask', async () => {
        const ask = jest
            .fn()
            .mockResolvedValueOnce({text: 'a1'})
            .mockResolvedValueOnce({text: 'a2'});
        renderPanel(ask);

        fireEvent.change(screen.getByTestId('hint-input'), {target: {value: 'q1'}});
        fireEvent.click(screen.getByTestId('hint-send'));
        await waitFor(() => expect(screen.getByTestId('hint-answer')).toHaveTextContent('a1'));

        fireEvent.change(screen.getByTestId('hint-input'), {target: {value: 'more'}});
        fireEvent.keyDown(screen.getByTestId('hint-input'), {key: 'Enter'});

        await waitFor(() => expect(ask).toHaveBeenCalledTimes(2));
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
        // ...and the failure never entered the conversation.
        expect(screen.queryByTestId('hint-answer')).not.toBeInTheDocument();
    });

    test('closing and reopening always starts a new chat', async () => {
        const ask = jest.fn().mockResolvedValue({text: 'old answer'});
        const view = render(<HintPanel open onClose={jest.fn()} sessionId={sessionId} ask={ask} />);

        fireEvent.change(screen.getByTestId('hint-input'), {target: {value: 'old question'}});
        fireEvent.click(screen.getByTestId('hint-send'));
        await screen.findByText('old answer');

        view.rerender(
            <HintPanel open={false} onClose={jest.fn()} sessionId={sessionId} ask={ask} />,
        );
        view.rerender(<HintPanel open onClose={jest.fn()} sessionId={sessionId} ask={ask} />);

        expect(screen.getByText("Stuck? Ask me anything. I won't judge. Much.")).toBeInTheDocument();
        expect(screen.queryByText('old answer')).not.toBeInTheDocument();
    });

    test('New chat resets the conversation without closing the panel', async () => {
        const ask = jest.fn().mockResolvedValue({text: 'old answer'});
        renderPanel(ask);

        fireEvent.change(screen.getByTestId('hint-input'), {target: {value: 'old question'}});
        fireEvent.click(screen.getByTestId('hint-send'));
        await screen.findByText('old answer');

        fireEvent.click(screen.getByRole('button', {name: 'New chat'}));

        expect(screen.getByTestId('hint-panel')).toBeInTheDocument();
        expect(screen.getByText("Stuck? Ask me anything. I won't judge. Much.")).toBeInTheDocument();
        expect(screen.queryByText('old answer')).not.toBeInTheDocument();
    });

    test('an answer already in flight cannot repopulate a new chat', async () => {
        let resolveAnswer!: (answer: {text: string}) => void;
        const ask = jest.fn(
            () => new Promise<{text: string}>((resolve) => (resolveAnswer = resolve)),
        );
        renderPanel(ask);

        fireEvent.change(screen.getByTestId('hint-input'), {target: {value: 'old question'}});
        fireEvent.click(screen.getByTestId('hint-send'));
        expect(screen.getByTestId('hint-pending')).toBeInTheDocument();

        fireEvent.click(screen.getByRole('button', {name: 'New chat'}));
        await act(async () => resolveAnswer({text: 'late answer'}));

        expect(screen.getByText("Stuck? Ask me anything. I won't judge. Much.")).toBeInTheDocument();
        expect(screen.queryByText('late answer')).not.toBeInTheDocument();
    });

    test('a refusal (isHint: false) is shown but never recorded', async () => {
        // e.g. the stale-session message: the server answers 200 with a system message, not a hint.
        const ask = jest
            .fn()
            .mockResolvedValue({text: "I can't find a game in progress.", isHint: false});
        renderPanel(ask);

        fireEvent.change(screen.getByTestId('hint-input'), {target: {value: 'help'}});
        fireEvent.click(screen.getByTestId('hint-send'));

        await waitFor(() => expect(screen.getByTestId('hint-error')).toBeInTheDocument());
        expect(screen.getByTestId('hint-error')).toHaveTextContent("can't find a game");
        // The question is restored for a clean retry.
        expect(screen.getByTestId('hint-input')).toHaveValue('help');
    });
});
