import React, {useEffect, useRef, useState} from "react";
import TipsAndUpdatesOutlinedIcon from "@mui/icons-material/TipsAndUpdatesOutlined";
import CloseRoundedIcon from "@mui/icons-material/CloseRounded";
import SendRoundedIcon from "@mui/icons-material/SendRounded";
import {HintExchange} from "../HintExchange";

/**
 * The hint side panel: a chat-style conversation with the game's snarky, incorporeal narrator.
 *
 * All behavior lives here so every game client gets it for free:
 *  - the conversation history (owned by the client — the /hint endpoint is stateless) is kept in
 *    state and persisted to localStorage per session, so it survives a refresh and resets
 *    naturally when the session id changes (restart);
 *  - failed asks are shown but NOT appended to the history, so they can't poison the pacing;
 *  - quick-ask chips, Enter-to-send, auto-scroll, and a pending indicator.
 *
 * Theming is via CSS variables (the shared <Compass> pattern) — each game maps these in its own
 * index.css. All have neutral fallbacks:
 *   --hint-accent          narrator accent (borders, glow, send button)
 *   --hint-user-accent     player-bubble accent
 *   --hint-badge           "costs no turn" badge color
 *   --hint-bg / --hint-bg-deep   panel gradient stops
 *   --hint-bubble          narrator bubble background
 *   --hint-text / --hint-muted   text colors
 *   --hint-narrator-font   narrator bubble font (the game-text font)
 *   --hint-heading-font    panel title font (the game's display font)
 */

type HintPanelProps = {
    open: boolean;
    onClose: () => void;
    sessionId: string;
    /** Sends a question (with the running history) to the game's hint endpoint. */
    ask: (question: string, history: HintExchange[]) => Promise<string>;
    /** Layout sizing/positioning from the host (width, height, responsive visibility). */
    className?: string;
    /** Quick-ask chips. Defaults suit any game. */
    quickAsks?: string[];
};

const storageKey = (sessionId: string) => `HintHistory-${sessionId}`;

function loadHistory(sessionId: string): HintExchange[] {
    try {
        const raw = localStorage.getItem(storageKey(sessionId));
        return raw ? (JSON.parse(raw) as HintExchange[]) : [];
    } catch {
        return [];
    }
}

export default function HintPanel({
    open,
    onClose,
    sessionId,
    ask,
    className,
    quickAsks = ["What should I do?", "I'm stuck", "Is this a dead end?", "Tell me more"]
}: HintPanelProps) {
    const [history, setHistory] = useState<HintExchange[]>(() => loadHistory(sessionId));
    const [question, setQuestion] = useState<string>("");
    const [pending, setPending] = useState<boolean>(false);
    const [pendingQuestion, setPendingQuestion] = useState<string>("");
    const [error, setError] = useState<string | null>(null);

    const scrollRef = useRef<HTMLDivElement>(null);
    const inputRef = useRef<HTMLInputElement>(null);

    // A restart regenerates the session id — pick up that session's (empty) history.
    useEffect(() => {
        setHistory(loadHistory(sessionId));
        setError(null);
    }, [sessionId]);

    useEffect(() => {
        try {
            localStorage.setItem(storageKey(sessionId), JSON.stringify(history));
        } catch {
            /* storage full/unavailable — the conversation still works for this page load */
        }
    }, [history, sessionId]);

    useEffect(() => {
        scrollRef.current?.scrollTo({top: scrollRef.current.scrollHeight, behavior: "smooth"});
    }, [history, pending, error, open]);

    useEffect(() => {
        if (open) window.setTimeout(() => inputRef.current?.focus(), 100);
    }, [open]);

    const submit = async (text: string) => {
        const q = text.trim();
        if (!q || pending) return;

        setQuestion("");
        setError(null);
        setPending(true);
        setPendingQuestion(q);

        try {
            const revealed = await ask(q, history);
            setHistory(prev => [...prev, {question: q, revealed}]);
        } catch {
            // Show the failure in-voice, keep it OUT of the history, and let the player resend.
            setError("The hint system appears to be off sulking somewhere. Try again in a moment.");
            setQuestion(q);
        } finally {
            setPending(false);
            setPendingQuestion("");
        }
    };

    if (!open) return null;

    const accent = "var(--hint-accent, #f59e0b)";
    const userAccent = "var(--hint-user-accent, #38bdf8)";
    const text = "var(--hint-text, #e2e8f0)";
    const muted = "var(--hint-muted, #94a3b8)";
    const narratorFont = "var(--hint-narrator-font, ui-monospace, monospace)";

    const bubbleBase: React.CSSProperties = {
        maxWidth: "92%",
        fontSize: "13px",
        lineHeight: 1.6,
        padding: "8px 11px",
        animation: "hintFadeIn 0.4s ease-out forwards"
    };

    const narratorBubble: React.CSSProperties = {
        ...bubbleBase,
        alignSelf: "flex-start",
        background: "var(--hint-bubble, rgba(30, 41, 59, 0.85))",
        borderLeft: `3px solid ${accent}`,
        borderRadius: "0 13px 13px 13px",
        color: text,
        fontFamily: narratorFont
    };

    const playerBubble: React.CSSProperties = {
        ...bubbleBase,
        alignSelf: "flex-end",
        background: `color-mix(in srgb, ${userAccent} 16%, transparent)`,
        border: `1px solid color-mix(in srgb, ${userAccent} 40%, transparent)`,
        borderRadius: "13px 13px 3px 13px",
        color: text
    };

    return (
        <div
            className={className}
            data-testid="hint-panel"
            style={{
                display: "flex",
                flexDirection: "column",
                borderRadius: "10px",
                border: `1px solid color-mix(in srgb, ${accent} 45%, transparent)`,
                background: "linear-gradient(135deg, var(--hint-bg, #1e293b) 0%, var(--hint-bg-deep, #0f172a) 100%)",
                boxShadow: `0 0 34px color-mix(in srgb, ${accent} 12%, transparent)`,
                overflow: "hidden"
            }}
        >
            <style>{`
                @keyframes hintFadeIn { from { opacity: 0; transform: translateY(8px); } to { opacity: 1; transform: translateY(0); } }
                @keyframes hintDot { 0%, 80%, 100% { opacity: 0.25; } 40% { opacity: 1; } }
            `}</style>

            <div
                style={{
                    display: "flex",
                    alignItems: "center",
                    gap: "8px",
                    padding: "11px 12px",
                    borderBottom: `1px solid color-mix(in srgb, ${accent} 25%, transparent)`,
                    flex: "none"
                }}
            >
                <TipsAndUpdatesOutlinedIcon fontSize="small" style={{color: "var(--hint-badge, #fbbf24)"}}/>
                <div style={{flex: 1, minWidth: 0}}>
                    <div
                        style={{
                            color: accent,
                            fontSize: "16px",
                            fontWeight: 600,
                            lineHeight: 1.1,
                            fontFamily: "var(--hint-heading-font, inherit)",
                            textShadow: `0 0 10px color-mix(in srgb, ${accent} 50%, transparent)`
                        }}
                    >
                        Hints
                    </div>
                    <div style={{color: muted, fontSize: "11px"}}>Ask the narrator</div>
                </div>
                <span
                    style={{
                        fontSize: "10px",
                        color: "var(--hint-badge, #fbbf24)",
                        background: "color-mix(in srgb, var(--hint-badge, #fbbf24) 16%, transparent)",
                        border: "1px solid color-mix(in srgb, var(--hint-badge, #fbbf24) 40%, transparent)",
                        padding: "2px 7px",
                        borderRadius: "999px",
                        whiteSpace: "nowrap"
                    }}
                >
                    Costs no turn
                </span>
                <button
                    onClick={onClose}
                    aria-label="Close hints"
                    data-testid="hint-close"
                    style={{background: "none", border: "none", cursor: "pointer", padding: "2px", display: "flex", color: muted}}
                >
                    <CloseRoundedIcon fontSize="small"/>
                </button>
            </div>

            <div
                ref={scrollRef}
                data-testid="hint-conversation"
                style={{
                    flex: 1,
                    minHeight: 0,
                    overflowY: "auto",
                    padding: "12px",
                    display: "flex",
                    flexDirection: "column",
                    gap: "11px"
                }}
            >
                {history.length === 0 && !pending && (
                    <div
                        style={{
                            fontStyle: "italic",
                            fontSize: "12.5px",
                            color: muted,
                            textAlign: "center",
                            lineHeight: 1.5,
                            padding: "6px 4px"
                        }}
                    >
                        Stuck? Ask me anything. I won't judge. Much.
                    </div>
                )}

                {history.map((exchange, index) => (
                    <React.Fragment key={index}>
                        <div style={playerBubble} data-testid="hint-question">{exchange.question}</div>
                        <div style={narratorBubble} data-testid="hint-answer">{exchange.revealed}</div>
                    </React.Fragment>
                ))}

                {pending && (
                    <>
                        <div style={playerBubble}>{pendingQuestion}</div>
                        <div style={{...narratorBubble, display: "flex", gap: "5px", alignItems: "center"}}
                             data-testid="hint-pending" aria-label="The narrator is thinking">
                            {[0, 1, 2].map(i => (
                                <span
                                    key={i}
                                    style={{
                                        width: "6px",
                                        height: "6px",
                                        borderRadius: "50%",
                                        background: accent,
                                        animation: `hintDot 1.2s ${i * 0.2}s infinite`
                                    }}
                                />
                            ))}
                        </div>
                    </>
                )}

                {error && (
                    <div style={{...narratorBubble, borderLeftColor: "var(--hint-warning, #ef4444)"}}
                         data-testid="hint-error">
                        {error}
                    </div>
                )}
            </div>

            <div style={{display: "flex", flexWrap: "wrap", gap: "6px", padding: "0 12px 10px", flex: "none"}}>
                {quickAsks.map(chip => (
                    <button
                        key={chip}
                        onClick={() => submit(chip)}
                        disabled={pending}
                        data-testid="hint-chip"
                        style={{
                            fontSize: "11.5px",
                            color: `color-mix(in srgb, ${accent} 75%, white)`,
                            background: "none",
                            border: `1px solid color-mix(in srgb, ${accent} 40%, transparent)`,
                            borderRadius: "999px",
                            padding: "4px 10px",
                            cursor: pending ? "default" : "pointer",
                            opacity: pending ? 0.5 : 1
                        }}
                    >
                        {chip}
                    </button>
                ))}
            </div>

            <div
                style={{
                    display: "flex",
                    alignItems: "center",
                    gap: "8px",
                    padding: "10px 12px",
                    borderTop: `1px solid color-mix(in srgb, ${accent} 20%, transparent)`,
                    flex: "none"
                }}
            >
                <input
                    ref={inputRef}
                    value={question}
                    onChange={e => setQuestion(e.target.value)}
                    onKeyDown={e => {
                        if (e.key === "Enter") submit(question);
                    }}
                    placeholder="Ask for a hint…"
                    disabled={pending}
                    data-testid="hint-input"
                    style={{
                        flex: 1,
                        minWidth: 0,
                        background: "color-mix(in srgb, var(--hint-bg-deep, #0f172a) 80%, transparent)",
                        border: `1px solid color-mix(in srgb, ${userAccent} 30%, transparent)`,
                        borderRadius: "999px",
                        padding: "8px 13px",
                        color: text,
                        fontSize: "12.5px",
                        outline: "none"
                    }}
                />
                <button
                    onClick={() => submit(question)}
                    disabled={pending || !question.trim()}
                    aria-label="Send hint question"
                    data-testid="hint-send"
                    style={{
                        width: "34px",
                        height: "34px",
                        flex: "none",
                        borderRadius: "50%",
                        border: "none",
                        background: accent,
                        display: "flex",
                        alignItems: "center",
                        justifyContent: "center",
                        cursor: pending || !question.trim() ? "default" : "pointer",
                        opacity: pending || !question.trim() ? 0.5 : 1,
                        boxShadow: `0 0 14px color-mix(in srgb, ${accent} 45%, transparent)`
                    }}
                >
                    <SendRoundedIcon style={{fontSize: "18px", color: "var(--hint-bg-deep, #0f172a)"}}/>
                </button>
            </div>
        </div>
    );
}
