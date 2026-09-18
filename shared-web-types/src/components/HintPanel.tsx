import React, {useCallback, useEffect, useRef, useState} from 'react';
import TipsAndUpdatesOutlinedIcon from '@mui/icons-material/TipsAndUpdatesOutlined';
import CloseRoundedIcon from '@mui/icons-material/CloseRounded';
import SendRoundedIcon from '@mui/icons-material/SendRounded';
import AddCommentOutlinedIcon from '@mui/icons-material/AddCommentOutlined';
import {HintExchange} from '../HintExchange';
import {HintAnswer, toExchange} from '../utils/HintServer';

/**
 * The hint side panel: a chat-style conversation with the game's snarky, incorporeal narrator.
 *
 * All behavior lives here so every game client gets it for free:
 *  - the conversation history (owned by the client — the /hint endpoint is stateless) lives only
 *    for the current panel opening; closing the panel or choosing New chat discards it;
 *  - failed asks are shown but NOT appended to the history, so they can't poison the pacing;
 *  - Enter-to-send, auto-scroll, and a pending indicator.
 *
 * Theming is via CSS variables (the shared <Compass> pattern) — each game maps these in its own
 * index.css. All have neutral fallbacks:
 *   --hint-accent          narrator accent (borders, glow, send button)
 *   --hint-user-accent     player-bubble accent
 *   --hint-badge           narrator icon color
 *   --hint-bg / --hint-bg-deep   panel gradient stops
 *   --hint-bubble          narrator bubble background
 *   --hint-text / --hint-muted   text colors
 */

type HintPanelProps = {
    open: boolean;
    onClose: () => void;
    sessionId: string;
    /** Sends a question (with the running history) to the game's hint endpoint. */
    ask: (question: string, history: HintExchange[]) => Promise<HintAnswer>;
    /** Layout sizing/positioning from the host (width, height, responsive visibility). */
    className?: string;
};

const UI_FONT_STACK = "Roboto, system-ui, -apple-system, 'Segoe UI', Helvetica, Arial, sans-serif";

function renderHintText(value: string): React.ReactNode[] {
    return value.split(/(\*\*[^*]+\*\*)/g).map((part, index) =>
        part.startsWith('**') && part.endsWith('**') ? (
            <strong key={index} style={{color: 'var(--hint-badge, #fbbf24)', fontWeight: 700}}>
                {part.slice(2, -2)}
            </strong>
        ) : (
            part
        ),
    );
}

export default function HintPanel({open, onClose, sessionId, ask, className}: HintPanelProps) {
    const [history, setHistory] = useState<HintExchange[]>([]);
    const [question, setQuestion] = useState<string>('');
    const [pending, setPending] = useState<boolean>(false);
    const [pendingQuestion, setPendingQuestion] = useState<string>('');
    const [error, setError] = useState<string | null>(null);

    const scrollRef = useRef<HTMLDivElement>(null);
    const inputRef = useRef<HTMLInputElement>(null);
    const conversationGeneration = useRef(0);
    const wasOpen = useRef(open);

    const resetConversation = useCallback(() => {
        conversationGeneration.current += 1;
        setHistory([]);
        setQuestion('');
        setPending(false);
        setPendingQuestion('');
        setError(null);
    }, []);

    // Restarting the game or closing the panel always ends the current hint conversation.
    useEffect(() => {
        resetConversation();
    }, [sessionId, resetConversation]);

    useEffect(() => {
        if (wasOpen.current && !open) {
            resetConversation();
        }
        wasOpen.current = open;
    }, [open, resetConversation]);

    useEffect(() => {
        scrollRef.current?.scrollTo?.({
            top: scrollRef.current.scrollHeight,
            behavior: 'smooth',
        });
    }, [history, pending, error, open]);

    useEffect(() => {
        if (open) window.setTimeout(() => inputRef.current?.focus(), 100);
    }, [open]);

    const submit = async (text: string) => {
        const q = text.trim();
        if (!q || pending) return;

        setQuestion('');
        setError(null);
        setPending(true);
        setPendingQuestion(q);
        const generation = conversationGeneration.current;

        try {
            const answer = await ask(q, history);
            if (generation !== conversationGeneration.current) return;
            if (answer.isHint === false) {
                // A refusal/system message: show it, but keep it OUT of the recorded conversation —
                // replaying it to the narrator would pollute the disclosure pacing.
                setError(answer.text);
                setQuestion(q);
            } else {
                // Recorded with the kind/topic/rung the endpoint returned: the next "more" climbs from here.
                setHistory((prev) => [...prev, toExchange(q, answer)]);
            }
        } catch {
            if (generation !== conversationGeneration.current) return;
            // Show the failure in-voice, keep it OUT of the history, and let the player resend.
            setError('The hint system appears to be off sulking somewhere. Try again in a moment.');
            setQuestion(q);
        } finally {
            if (generation === conversationGeneration.current) {
                setPending(false);
                setPendingQuestion('');
            }
        }
    };

    if (!open) return null;

    const accent = 'var(--hint-accent, #f59e0b)';
    const userAccent = 'var(--hint-user-accent, #38bdf8)';
    const text = 'var(--hint-text, #e2e8f0)';
    const muted = 'var(--hint-muted, #94a3b8)';

    const bubbleBase: React.CSSProperties = {
        maxWidth: '88%',
        fontSize: '13.5px',
        lineHeight: 1.55,
        padding: '11px 13px',
        animation: 'hintFadeIn 0.28s ease-out forwards',
    };

    const narratorBubble: React.CSSProperties = {
        ...bubbleBase,
        alignSelf: 'flex-start',
        background: 'color-mix(in srgb, var(--hint-bubble, #1e293b) 82%, transparent)',
        border: `1px solid color-mix(in srgb, ${accent} 18%, transparent)`,
        borderRadius: '4px 16px 16px 16px',
        color: text,
        boxShadow: '0 8px 24px rgba(0, 0, 0, 0.16)',
    };

    const playerBubble: React.CSSProperties = {
        ...bubbleBase,
        alignSelf: 'flex-end',
        background: `color-mix(in srgb, ${userAccent} 18%, transparent)`,
        border: `1px solid color-mix(in srgb, ${userAccent} 32%, transparent)`,
        borderRadius: '16px 16px 4px 16px',
        color: text,
    };

    return (
        <div
            className={className}
            data-testid="hint-panel"
            style={{
                display: 'flex',
                flexDirection: 'column',
                borderRadius: '12px',
                border: `1px solid color-mix(in srgb, ${accent} 28%, transparent)`,
                background:
                    'linear-gradient(180deg, color-mix(in srgb, var(--hint-bg, #1e293b) 94%, black) 0%, var(--hint-bg-deep, #0f172a) 100%)',
                boxShadow: '0 20px 50px rgba(0, 0, 0, 0.28)',
                overflow: 'hidden',
                fontFamily: UI_FONT_STACK,
            }}
        >
            <style>{`
                @keyframes hintFadeIn { from { opacity: 0; transform: translateY(8px); } to { opacity: 1; transform: translateY(0); } }
                @keyframes hintDot { 0%, 80%, 100% { opacity: 0.25; } 40% { opacity: 1; } }
            `}</style>

            <div
                style={{
                    display: 'flex',
                    alignItems: 'center',
                    gap: '11px',
                    padding: '13px 14px',
                    borderBottom: `1px solid color-mix(in srgb, ${accent} 16%, transparent)`,
                    background: 'rgba(255, 255, 255, 0.025)',
                    flex: 'none',
                }}
            >
                <div
                    style={{
                        width: '34px',
                        height: '34px',
                        borderRadius: '10px',
                        display: 'grid',
                        placeItems: 'center',
                        color: 'var(--hint-badge, #fbbf24)',
                        background: `color-mix(in srgb, ${accent} 14%, transparent)`,
                        border: `1px solid color-mix(in srgb, ${accent} 24%, transparent)`,
                    }}
                >
                    <TipsAndUpdatesOutlinedIcon style={{fontSize: '20px'}} />
                </div>
                <div style={{flex: 1}} />
                <button
                    onClick={resetConversation}
                    aria-label="New chat"
                    data-testid="hint-new-chat"
                    style={{
                        height: '30px',
                        borderRadius: '8px',
                        background: 'rgba(255, 255, 255, 0.035)',
                        border: '1px solid rgba(255, 255, 255, 0.06)',
                        cursor: 'pointer',
                        padding: '0 9px',
                        display: 'flex',
                        gap: '5px',
                        alignItems: 'center',
                        color: muted,
                        fontFamily: UI_FONT_STACK,
                        fontSize: '11px',
                    }}
                >
                    <AddCommentOutlinedIcon style={{fontSize: '16px'}} />
                    New chat
                </button>
                <button
                    onClick={() => {
                        resetConversation();
                        onClose();
                    }}
                    aria-label="Close hints"
                    data-testid="hint-close"
                    style={{
                        width: '30px',
                        height: '30px',
                        borderRadius: '8px',
                        background: 'rgba(255, 255, 255, 0.035)',
                        border: '1px solid rgba(255, 255, 255, 0.06)',
                        cursor: 'pointer',
                        padding: 0,
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        color: muted,
                    }}
                >
                    <CloseRoundedIcon fontSize="small" />
                </button>
            </div>

            <div
                ref={scrollRef}
                data-testid="hint-conversation"
                style={{
                    flex: 1,
                    minHeight: 0,
                    overflowY: 'auto',
                    padding: '16px 14px',
                    display: 'flex',
                    flexDirection: 'column',
                    gap: '12px',
                }}
            >
                {history.length === 0 && !pending && (
                    <div
                        style={{
                            margin: 'auto',
                            maxWidth: '230px',
                            color: muted,
                            textAlign: 'center',
                            lineHeight: 1.55,
                            padding: '24px 12px',
                        }}
                    >
                        <TipsAndUpdatesOutlinedIcon
                            style={{
                                fontSize: '28px',
                                color: accent,
                                opacity: 0.72,
                                marginBottom: '8px',
                            }}
                        />
                        <div style={{color: text, fontSize: '14px', fontWeight: 600}}>
                            Stuck? Ask me anything. I won't judge. Much.
                        </div>
                    </div>
                )}

                {history.map((exchange, index) => (
                    <React.Fragment key={index}>
                        <div style={playerBubble} data-testid="hint-question">
                            {exchange.question}
                        </div>
                        <div style={narratorBubble} data-testid="hint-answer">
                            {renderHintText(exchange.revealed)}
                        </div>
                    </React.Fragment>
                ))}

                {pending && (
                    <>
                        <div style={playerBubble}>{pendingQuestion}</div>
                        <div
                            style={{
                                ...narratorBubble,
                                display: 'flex',
                                gap: '5px',
                                alignItems: 'center',
                            }}
                            data-testid="hint-pending"
                            aria-label="The narrator is thinking"
                        >
                            {[0, 1, 2].map((i) => (
                                <span
                                    key={i}
                                    style={{
                                        width: '6px',
                                        height: '6px',
                                        borderRadius: '50%',
                                        background: accent,
                                        animation: `hintDot 1.2s ${i * 0.2}s infinite`,
                                    }}
                                />
                            ))}
                        </div>
                    </>
                )}

                {error && (
                    <div
                        style={{
                            ...narratorBubble,
                            borderColor:
                                'color-mix(in srgb, var(--hint-warning, #ef4444) 55%, transparent)',
                        }}
                        data-testid="hint-error"
                    >
                        {error}
                    </div>
                )}
            </div>

            <div
                style={{
                    display: 'flex',
                    alignItems: 'center',
                    gap: '9px',
                    padding: '12px',
                    borderTop: `1px solid color-mix(in srgb, ${accent} 14%, transparent)`,
                    background: 'rgba(0, 0, 0, 0.1)',
                    flex: 'none',
                }}
            >
                <input
                    ref={inputRef}
                    value={question}
                    onChange={(e) => setQuestion(e.target.value)}
                    onKeyDown={(e) => {
                        if (e.key === 'Enter') submit(question);
                    }}
                    placeholder="Ask for a hint…"
                    disabled={pending}
                    data-testid="hint-input"
                    style={{
                        flex: 1,
                        minWidth: 0,
                        height: '42px',
                        boxSizing: 'border-box',
                        background: 'rgba(0, 0, 0, 0.2)',
                        border: `1px solid color-mix(in srgb, ${accent} 22%, transparent)`,
                        borderRadius: '11px',
                        padding: '0 13px',
                        color: text,
                        fontSize: '12.5px',
                        fontFamily: UI_FONT_STACK,
                        outline: 'none',
                    }}
                />
                <button
                    onClick={() => submit(question)}
                    disabled={pending || !question.trim()}
                    aria-label="Send hint question"
                    data-testid="hint-send"
                    style={{
                        width: '42px',
                        height: '42px',
                        flex: 'none',
                        borderRadius: '11px',
                        border: 'none',
                        background: accent,
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        cursor: pending || !question.trim() ? 'default' : 'pointer',
                        opacity: pending || !question.trim() ? 0.5 : 1,
                        boxShadow: `0 8px 18px color-mix(in srgb, ${accent} 18%, transparent)`,
                    }}
                >
                    <SendRoundedIcon
                        style={{fontSize: '19px', color: 'var(--hint-bg-deep, #0f172a)'}}
                    />
                </button>
            </div>
        </div>
    );
}
