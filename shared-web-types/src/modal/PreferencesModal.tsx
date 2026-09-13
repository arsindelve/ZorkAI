import React from 'react';
import Dialog from '@mui/material/Dialog';
import {useGameContext} from '../context/GameContext';
import {
    TRANSCRIPT_FONT_SIZE_LABELS,
    TRANSCRIPT_FONT_SIZES,
    isTranscriptFontSize,
    transcriptFontSizePx,
} from '../preferences/TranscriptFontSize';
import {
    TRANSCRIPT_FONTS,
    TRANSCRIPT_FONT_LABELS,
    TRANSCRIPT_FONT_STACKS,
    TranscriptFont,
    isTranscriptFont,
} from '../preferences/TranscriptFont';
import {
    TRANSCRIPT_LINE_SPACINGS,
    TRANSCRIPT_LINE_SPACING_LABELS,
    isTranscriptLineSpacing,
    transcriptLineHeight,
} from '../preferences/TranscriptLineSpacing';
import {
    TRANSCRIPT_MARKERS,
    TRANSCRIPT_MARKER_LABELS,
    formatTranscriptMarker,
    isTranscriptMarker,
} from '../preferences/TranscriptMarker';
import {COMPASS_SIZE_LABELS, COMPASS_SIZES, isCompassSize} from '../preferences/CompassPreferences';

// Ships its own CSS, the way <Compass> does: the palette comes from --pref-* custom
// properties each client sets in its own :root, so one dialog wears each game's colors.
// The fallbacks are Zork's gold, so it still looks right if a client defines none.
const PREFERENCES_STYLES = `
.prefs {
    --gold: var(--pref-accent, #c49a4c);
    --gold-bright: var(--pref-accent-bright, #e3c179);
    --ink: var(--pref-text-color, #ede4d3);
    --dim: var(--pref-text-dim, #9c9384);
    --edge: var(--pref-border, rgba(196, 154, 76, 0.22));
    --raised: var(--pref-surface-raised, #221d16);
    background: var(--pref-surface, #17140f);
    color: var(--ink);
    border: 1px solid var(--edge);
    border-radius: 14px;
    overflow: hidden;
}

/* An engraved plate rather than a Material title bar, echoing the game's masthead. */
.prefs__bar {
    display: flex;
    align-items: baseline;
    justify-content: space-between;
    gap: 12px;
    padding: 15px 22px 13px;
    border-bottom: 1px solid var(--edge);
    background: linear-gradient(180deg, rgba(196, 154, 76, 0.1) 0%, transparent 100%);
}
.prefs__title {
    font-family: Platypi, Georgia, serif;
    font-size: 1.12rem;
    font-weight: 500;
    letter-spacing: 0.04em;
    margin: 0;
}
.prefs__hint {
    font-size: 0.68rem;
    color: var(--dim);
}

/* The specimen: the transcript itself, not a description of it. It replaces the
   per-option previews that made this dialog too tall to fit on screen. */
.prefs__specimen {
    margin: 16px 22px 2px;
    padding: 15px 17px;
    background: var(--pref-specimen-bg, #0c0a09);
    border: 1px solid var(--edge);
    border-radius: 10px;
}
.prefs__specimen p {
    margin: 0;
}
.prefs__specimen-echo {
    color: var(--gold);
    font-weight: 800;
    margin-bottom: 0.35em !important;
}
.prefs__specimen-marker {
    opacity: 0.5;
    font-weight: 400;
    margin-right: 0.6em;
}
.prefs__specimen-room {
    color: var(--gold-bright);
    letter-spacing: 0.14em;
    text-transform: uppercase;
    margin-bottom: 0.3em !important;
}
.prefs__specimen-body {
    opacity: 0.92;
}

.prefs__body {
    padding: 10px 22px 2px;
}

/* Eyebrows borrow the MOVES/SCORE treatment already in the game's header. */
.prefs__group {
    font-family: Platypi, Georgia, serif;
    font-weight: 500;
    font-size: 0.64rem;
    text-transform: uppercase;
    letter-spacing: 0.18em;
    color: var(--dim);
    margin: 16px 0 9px;
    display: flex;
    align-items: center;
    gap: 10px;
}
.prefs__group::after {
    content: '';
    flex: 1;
    height: 1px;
    background: var(--edge);
}

.prefs__row {
    display: flex;
    align-items: center;
    gap: 14px;
    flex-wrap: wrap;
}
.prefs__row + .prefs__row {
    margin-top: 7px;
}
.prefs__row-label {
    flex: 0 0 104px;
    font-size: 0.78rem;
    color: var(--dim);
}
.prefs__row--off .prefs__row-label {
    opacity: 0.45;
}

/* Segmented control: one row instead of four stacked radios. Built on real radio
   inputs, so checked state and arrow-key navigation come from the platform. */
.prefs__seg {
    display: flex;
    flex-wrap: wrap;
    gap: 3px;
    padding: 3px;
    background: var(--raised);
    border: 1px solid var(--edge);
    border-radius: 9px;
}
.prefs__seg input,
.prefs__chip input {
    position: absolute;
    width: 1px;
    height: 1px;
    margin: -1px;
    opacity: 0;
    pointer-events: none;
}
.prefs__seg label {
    padding: 5px 10px;
    border-radius: 6px;
    font-size: 0.77rem;
    color: var(--pref-text-muted, #bdb3a1);
    cursor: pointer;
    white-space: nowrap;
    user-select: none;
    transition: background-color 0.15s, color 0.15s;
}
.prefs__seg label:hover {
    color: var(--ink);
    background: rgba(196, 154, 76, 0.08);
}
.prefs__seg input:checked + label {
    background: var(--gold);
    color: #1a1206;
    font-weight: 600;
}
.prefs__seg input:focus-visible + label,
.prefs__chip input:focus-visible + label {
    outline: 2px solid var(--gold-bright);
    outline-offset: 2px;
}
.prefs__seg input:disabled + label {
    opacity: 0.35;
    cursor: not-allowed;
}
.prefs__seg input:disabled + label:hover {
    background: none;
    color: var(--dim);
}

/* Toggles as chips that light up: six switch tracks in a column shouted louder than
   the settings they controlled. */
.prefs__chips {
    display: flex;
    flex-wrap: wrap;
    gap: 7px;
}
.prefs__chip label {
    display: inline-flex;
    align-items: center;
    gap: 7px;
    padding: 6px 13px 6px 10px;
    border: 1px solid var(--edge);
    border-radius: 999px;
    font-size: 0.77rem;
    color: var(--dim);
    cursor: pointer;
    user-select: none;
    transition: background-color 0.15s, color 0.15s, border-color 0.15s;
}
.prefs__chip label::before {
    content: '';
    width: 7px;
    height: 7px;
    border-radius: 50%;
    background: var(--dim);
    opacity: 0.45;
    transition: background-color 0.15s, opacity 0.15s, box-shadow 0.15s;
}
.prefs__chip label:hover {
    color: var(--ink);
    border-color: var(--gold);
}
.prefs__chip input:checked + label {
    color: var(--ink);
    border-color: var(--gold);
    background: rgba(196, 154, 76, 0.12);
}
.prefs__chip input:checked + label::before {
    background: var(--gold-bright);
    opacity: 1;
    box-shadow: 0 0 7px var(--gold);
}

.prefs__foot {
    display: flex;
    justify-content: flex-end;
    padding: 14px 22px 16px;
    margin-top: 14px;
    border-top: 1px solid var(--edge);
}
.prefs__done {
    font-family: Roboto, system-ui, -apple-system, 'Segoe UI', Helvetica, Arial, sans-serif;
    font-size: 0.86rem;
    letter-spacing: 0.01em;
    padding: 9px 30px;
    border-radius: 999px;
    border: 1px solid var(--gold);
    background: var(--gold);
    color: #1a1206;
    font-weight: 600;
    cursor: pointer;
    transition: background-color 0.15s, box-shadow 0.15s;
}
.prefs__done:hover {
    background: var(--gold-bright);
    box-shadow: 0 0 16px rgba(196, 154, 76, 0.35);
}
.prefs__done:focus-visible {
    outline: 2px solid var(--gold-bright);
    outline-offset: 3px;
}

@media (max-width: 520px) {
    .prefs__bar,
    .prefs__body,
    .prefs__foot {
        padding-left: 15px;
        padding-right: 15px;
    }
    .prefs__specimen {
        margin-left: 15px;
        margin-right: 15px;
    }
    .prefs__row-label {
        flex-basis: 100%;
    }
}
`;

interface PreferencesModalProps {
    open: boolean;
    handleClose: () => void;
    /** The game's own base transcript size, so the specimen renders at true scale. */
    baseFontSizePx: number;
    /** A few lines of this game's own prose for the specimen to render. */
    specimenCommand?: string;
    specimenRoom?: string;
    specimenBody?: string;
}

// data-* props land on a control's outer element in MUI; here they go straight on the
// input, so checked and disabled state are observable.
const testId = (id: string) => ({'data-testid': id}) as React.InputHTMLAttributes<HTMLInputElement>;

interface SegmentedProps<T extends string> {
    name: string;
    value: T;
    options: readonly T[];
    labels: Record<T, string>;
    onChange: (value: string) => void;
    disabled?: boolean;
    testIdPrefix: string;
    ariaLabel: string;
    optionStyle?: (option: T) => React.CSSProperties | undefined;
}

function Segmented<T extends string>({
    name,
    value,
    options,
    labels,
    onChange,
    disabled,
    testIdPrefix,
    ariaLabel,
    optionStyle,
}: SegmentedProps<T>) {
    return (
        <div className="prefs__seg" role="radiogroup" aria-label={ariaLabel}>
            {options.map((option) => (
                <React.Fragment key={option}>
                    <input
                        type="radio"
                        id={`${name}-${option}`}
                        name={name}
                        value={option}
                        checked={value === option}
                        disabled={disabled}
                        onChange={(event) => onChange(event.target.value)}
                        {...testId(`${testIdPrefix}-${option}`)}
                    />
                    <label htmlFor={`${name}-${option}`} style={optionStyle?.(option)}>
                        {labels[option]}
                    </label>
                </React.Fragment>
            ))}
        </div>
    );
}

interface ChipProps {
    id: string;
    label: string;
    checked: boolean;
    onChange: (checked: boolean) => void;
    testIdName: string;
}

const Chip: React.FC<ChipProps> = ({id, label, checked, onChange, testIdName}) => (
    <span className="prefs__chip">
        <input
            type="checkbox"
            id={id}
            checked={checked}
            onChange={(event) => onChange(event.target.checked)}
            {...testId(testIdName)}
        />
        <label htmlFor={id}>{label}</label>
    </span>
);

const PreferencesModal: React.FC<PreferencesModalProps> = ({
    open,
    handleClose,
    baseFontSizePx,
    specimenCommand = 'open the mailbox',
    specimenRoom = 'West of House',
    specimenBody = 'Opening the small mailbox reveals a leaflet.',
}) => {
    const {
        transcriptFontSize,
        setTranscriptFontSize,
        transcriptFont,
        setTranscriptFont,
        transcriptLineSpacing,
        setTranscriptLineSpacing,
        transcriptMarker,
        setTranscriptMarker,
        showCompass,
        setShowCompass,
        compassSize,
        setCompassSize,
        showVerbsMenu,
        setShowVerbsMenu,
        showCommandsMenu,
        setShowCommandsMenu,
        showLocationButton,
        setShowLocationButton,
        showInventoryButton,
        setShowInventoryButton,
        animations,
        setAnimations,
    } = useGameContext();

    // Every control applies immediately. The specimen is the preview, so a Save button
    // would only put a step between making a choice and seeing it.
    const specimenStyle: React.CSSProperties = {
        fontFamily: TRANSCRIPT_FONT_STACKS[transcriptFont],
        fontSize: `${transcriptFontSizePx(baseFontSizePx, transcriptFontSize)}px`,
        lineHeight: transcriptLineHeight(transcriptLineSpacing),
    };

    // A fixed command number keeps the specimen steady while you compare options.
    const marker = formatTranscriptMarker(transcriptMarker, 12);

    return (
        <Dialog
            data-testid="preferences-dialog"
            open={open}
            fullWidth
            maxWidth="sm"
            onClose={handleClose}
            aria-labelledby="preferences-dialog-title"
            PaperProps={{
                sx: {
                    backgroundColor: 'transparent',
                    backgroundImage: 'none',
                    boxShadow: '0 24px 60px rgba(0, 0, 0, 0.6)',
                    borderRadius: '14px',
                    // Just past MUI's `sm`, so the five font options sit on one line
                    // beside their label instead of wrapping under it.
                    maxWidth: '664px',
                },
            }}
        >
            <style>{PREFERENCES_STYLES}</style>
            <div className="prefs">
                <div className="prefs__bar">
                    <h2 className="prefs__title" id="preferences-dialog-title">
                        Preferences
                    </h2>
                    <span className="prefs__hint">Saved on this device</span>
                </div>

                <div className="prefs__specimen" style={specimenStyle} aria-hidden="true">
                    <p className="prefs__specimen-echo">
                        {marker && <span className="prefs__specimen-marker">{marker}</span>}
                        &gt; {specimenCommand}
                    </p>
                    <p className="prefs__specimen-room">{specimenRoom}</p>
                    <p className="prefs__specimen-body">{specimenBody}</p>
                </div>

                <div className="prefs__body">
                    <div className="prefs__group">Reading</div>

                    <div className="prefs__row">
                        <span className="prefs__row-label">Font</span>
                        <Segmented
                            name="transcript-font"
                            value={transcriptFont}
                            options={TRANSCRIPT_FONTS}
                            labels={TRANSCRIPT_FONT_LABELS}
                            onChange={(value) =>
                                isTranscriptFont(value) && setTranscriptFont(value)
                            }
                            testIdPrefix="font"
                            ariaLabel="Font"
                            // Each name set in its own face - the fastest way to judge one.
                            optionStyle={(font: TranscriptFont) => ({
                                fontFamily: TRANSCRIPT_FONT_STACKS[font],
                            })}
                        />
                    </div>

                    <div className="prefs__row">
                        <span className="prefs__row-label">Size</span>
                        <Segmented
                            name="transcript-font-size"
                            value={transcriptFontSize}
                            options={TRANSCRIPT_FONT_SIZES}
                            labels={TRANSCRIPT_FONT_SIZE_LABELS}
                            onChange={(value) =>
                                isTranscriptFontSize(value) && setTranscriptFontSize(value)
                            }
                            testIdPrefix="font-size"
                            ariaLabel="Size"
                        />
                    </div>

                    <div className="prefs__row">
                        <span className="prefs__row-label">Spacing</span>
                        <Segmented
                            name="line-spacing"
                            value={transcriptLineSpacing}
                            options={TRANSCRIPT_LINE_SPACINGS}
                            labels={TRANSCRIPT_LINE_SPACING_LABELS}
                            onChange={(value) =>
                                isTranscriptLineSpacing(value) && setTranscriptLineSpacing(value)
                            }
                            testIdPrefix="line-spacing"
                            ariaLabel="Spacing"
                        />
                    </div>

                    <div className="prefs__row">
                        <span className="prefs__row-label">Label commands</span>
                        <Segmented
                            name="transcript-marker"
                            value={transcriptMarker}
                            options={TRANSCRIPT_MARKERS}
                            labels={TRANSCRIPT_MARKER_LABELS}
                            onChange={(value) =>
                                isTranscriptMarker(value) && setTranscriptMarker(value)
                            }
                            testIdPrefix="marker"
                            ariaLabel="Label commands"
                        />
                    </div>

                    <div className="prefs__group">On screen</div>

                    <div className="prefs__chips">
                        <Chip
                            id="pref-compass"
                            label="Compass"
                            checked={showCompass}
                            onChange={setShowCompass}
                            testIdName="toggle-compass"
                        />
                        <Chip
                            id="pref-verbs"
                            label="Verbs"
                            checked={showVerbsMenu}
                            onChange={setShowVerbsMenu}
                            testIdName="toggle-verbs-menu"
                        />
                        <Chip
                            id="pref-commands"
                            label="Commands"
                            checked={showCommandsMenu}
                            onChange={setShowCommandsMenu}
                            testIdName="toggle-commands-menu"
                        />
                        <Chip
                            id="pref-location"
                            label="Location"
                            checked={showLocationButton}
                            onChange={setShowLocationButton}
                            testIdName="toggle-location-button"
                        />
                        <Chip
                            id="pref-inventory"
                            label="Inventory"
                            checked={showInventoryButton}
                            onChange={setShowInventoryButton}
                            testIdName="toggle-inventory-button"
                        />
                        <Chip
                            id="pref-animations"
                            label="Animations"
                            checked={animations}
                            onChange={setAnimations}
                            testIdName="toggle-animations"
                        />
                    </div>

                    <div
                        className={`prefs__row${showCompass ? '' : ' prefs__row--off'}`}
                        style={{marginTop: 14}}
                    >
                        <span className="prefs__row-label">Compass size</span>
                        <Segmented
                            name="compass-size"
                            value={compassSize}
                            options={COMPASS_SIZES}
                            labels={COMPASS_SIZE_LABELS}
                            onChange={(value) => isCompassSize(value) && setCompassSize(value)}
                            // Sizing a hidden compass does nothing, so this is disabled
                            // rather than silently inert.
                            disabled={!showCompass}
                            testIdPrefix="compass-size"
                            ariaLabel="Compass size"
                        />
                    </div>
                </div>

                <div className="prefs__foot">
                    <button
                        type="button"
                        className="prefs__done"
                        onClick={handleClose}
                        data-testid="preferences-done"
                    >
                        Done
                    </button>
                </div>
            </div>
        </Dialog>
    );
};

export default PreferencesModal;
