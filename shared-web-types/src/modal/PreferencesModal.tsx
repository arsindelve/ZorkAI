import React from 'react';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogTitle from '@mui/material/DialogTitle';
import Button from '@mui/material/Button';
import {
    Divider,
    FormControl,
    FormControlLabel,
    FormLabel,
    Radio,
    RadioGroup,
    Switch,
    Typography,
} from '@mui/material';
import TuneIcon from '@mui/icons-material/Tune';
import {useGameContext} from '../context/GameContext';
import {
    TRANSCRIPT_FONT_SIZE_LABELS,
    TRANSCRIPT_FONT_SIZES,
    TRANSCRIPT_FONT_SCALE,
    TranscriptFontSize,
    isTranscriptFontSize,
} from '../preferences/TranscriptFontSize';
import {
    TRANSCRIPT_FONTS,
    TRANSCRIPT_FONT_LABELS,
    TRANSCRIPT_FONT_DESCRIPTIONS,
    TRANSCRIPT_FONT_STACKS,
    TranscriptFont,
    isTranscriptFont,
} from '../preferences/TranscriptFont';
import {
    TRANSCRIPT_LINE_SPACINGS,
    TRANSCRIPT_LINE_SPACING_LABELS,
    TranscriptLineSpacing,
    isTranscriptLineSpacing,
} from '../preferences/TranscriptLineSpacing';
import {
    TRANSCRIPT_MARKERS,
    TRANSCRIPT_MARKER_LABELS,
    TRANSCRIPT_MARKER_DESCRIPTIONS,
    TranscriptMarker,
    isTranscriptMarker,
} from '../preferences/TranscriptMarker';
import {
    COMPASS_SIZE_LABELS,
    COMPASS_SIZES,
    CompassSize,
    isCompassSize,
} from '../preferences/CompassPreferences';

interface PreferencesModalProps {
    open: boolean;
    handleClose: () => void;
    /** The game's own base transcript size, used to preview each option at true scale. */
    baseFontSizePx: number;
}

const sectionLabelSx = {fontWeight: 'bold', color: 'text.primary', mb: 0.5} as const;

// MUI renders data-* props on the control's outer <span>, not on the <input> it wraps.
// Putting the test id on inputProps instead points it at the real form control, so its
// checked/disabled state is actually observable.
const testId = (id: string) => ({'data-testid': id}) as React.InputHTMLAttributes<HTMLInputElement>;

const PreferencesModal: React.FC<PreferencesModalProps> = ({
    open,
    handleClose,
    baseFontSizePx,
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
        animations,
        setAnimations,
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
    } = useGameContext();

    // Every control applies immediately rather than on a Save button: the game is
    // visible behind the dialog, so each change is its own preview.
    const handleFontSizeChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        const value = event.target.value;
        if (isTranscriptFontSize(value)) setTranscriptFontSize(value);
    };

    const handleFontChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        const value = event.target.value;
        if (isTranscriptFont(value)) setTranscriptFont(value);
    };

    const handleLineSpacingChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        const value = event.target.value;
        if (isTranscriptLineSpacing(value)) setTranscriptLineSpacing(value);
    };

    const handleMarkerChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        const value = event.target.value;
        if (isTranscriptMarker(value)) setTranscriptMarker(value);
    };

    const handleCompassSizeChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        const value = event.target.value;
        if (isCompassSize(value)) setCompassSize(value);
    };

    return (
        <Dialog
            data-testid="preferences-dialog"
            open={open}
            fullWidth={true}
            maxWidth="sm"
            onClose={handleClose}
            aria-labelledby="preferences-dialog-title"
            PaperProps={{
                style: {
                    borderRadius: '12px',
                    overflow: 'hidden',
                },
            }}
        >
            <DialogTitle
                id="preferences-dialog-title"
                className="bg-gradient-to-r from-gray-800 to-gray-900"
                sx={{
                    color: 'white',
                    display: 'flex',
                    alignItems: 'center',
                    gap: 1,
                    py: 2,
                }}
            >
                <TuneIcon fontSize="large" />
                <Typography variant="h5" component="span" fontWeight="bold">
                    Preferences
                </Typography>
            </DialogTitle>

            <DialogContent sx={{px: 4, pt: 3, pb: 2}}>
                {/* ----------------------------------------- transcript font size */}
                <FormControl sx={{display: 'block'}}>
                    <FormLabel id="transcript-font-size-label" sx={sectionLabelSx}>
                        Transcript font size
                    </FormLabel>
                    <RadioGroup
                        aria-labelledby="transcript-font-size-label"
                        name="transcript-font-size"
                        value={transcriptFontSize}
                        onChange={handleFontSizeChange}
                    >
                        {TRANSCRIPT_FONT_SIZES.map((size: TranscriptFontSize) => (
                            <FormControlLabel
                                key={size}
                                value={size}
                                control={<Radio inputProps={testId(`font-size-${size}`)} />}
                                label={
                                    <span
                                        style={{
                                            fontFamily: TRANSCRIPT_FONT_STACKS[transcriptFont],
                                            fontSize: `${Math.round(
                                                baseFontSizePx * TRANSCRIPT_FONT_SCALE[size],
                                            )}px`,
                                        }}
                                    >
                                        {TRANSCRIPT_FONT_SIZE_LABELS[size]}
                                    </span>
                                }
                            />
                        ))}
                    </RadioGroup>
                </FormControl>

                <Divider sx={{my: 2}} />

                {/* ---------------------------------------------- transcript font */}
                <FormControl sx={{display: 'block'}}>
                    <FormLabel id="transcript-font-label" sx={sectionLabelSx}>
                        Transcript font
                    </FormLabel>
                    <RadioGroup
                        aria-labelledby="transcript-font-label"
                        name="transcript-font"
                        value={transcriptFont}
                        onChange={handleFontChange}
                    >
                        {TRANSCRIPT_FONTS.map((font: TranscriptFont) => (
                            <FormControlLabel
                                key={font}
                                value={font}
                                control={<Radio inputProps={testId(`font-${font}`)} />}
                                label={
                                    <span>
                                        {/* Previewed in the font itself - the name of a
                                            typeface tells a player far less than seeing it. */}
                                        <span
                                            style={{
                                                fontFamily: TRANSCRIPT_FONT_STACKS[font],
                                                fontSize: '15px',
                                            }}
                                        >
                                            {TRANSCRIPT_FONT_LABELS[font]}
                                        </span>
                                        <Typography
                                            variant="caption"
                                            color="text.secondary"
                                            sx={{display: 'block', lineHeight: 1.3}}
                                        >
                                            {TRANSCRIPT_FONT_DESCRIPTIONS[font]}
                                        </Typography>
                                    </span>
                                }
                            />
                        ))}
                    </RadioGroup>
                </FormControl>

                <Divider sx={{my: 2}} />

                {/* ----------------------------------------------- line spacing */}
                <FormControl sx={{display: 'block'}}>
                    <FormLabel id="line-spacing-label" sx={sectionLabelSx}>
                        Line spacing
                    </FormLabel>
                    <RadioGroup
                        row
                        aria-labelledby="line-spacing-label"
                        name="line-spacing"
                        value={transcriptLineSpacing}
                        onChange={handleLineSpacingChange}
                    >
                        {TRANSCRIPT_LINE_SPACINGS.map((spacing: TranscriptLineSpacing) => (
                            <FormControlLabel
                                key={spacing}
                                value={spacing}
                                control={<Radio inputProps={testId(`line-spacing-${spacing}`)} />}
                                label={TRANSCRIPT_LINE_SPACING_LABELS[spacing]}
                            />
                        ))}
                    </RadioGroup>
                </FormControl>

                <Divider sx={{my: 2}} />

                {/* --------------------------------------------- command marker */}
                <FormControl sx={{display: 'block'}}>
                    <FormLabel id="transcript-marker-label" sx={sectionLabelSx}>
                        Label each command with
                    </FormLabel>
                    <RadioGroup
                        aria-labelledby="transcript-marker-label"
                        name="transcript-marker"
                        value={transcriptMarker}
                        onChange={handleMarkerChange}
                    >
                        {TRANSCRIPT_MARKERS.map((marker: TranscriptMarker) => (
                            <FormControlLabel
                                key={marker}
                                value={marker}
                                control={<Radio inputProps={testId(`marker-${marker}`)} />}
                                label={
                                    <span>
                                        {TRANSCRIPT_MARKER_LABELS[marker]}
                                        <Typography
                                            variant="caption"
                                            color="text.secondary"
                                            sx={{display: 'block', lineHeight: 1.3}}
                                        >
                                            {TRANSCRIPT_MARKER_DESCRIPTIONS[marker]}
                                        </Typography>
                                    </span>
                                }
                            />
                        ))}
                    </RadioGroup>
                </FormControl>

                <Divider sx={{my: 2}} />

                {/* ---------------------------------------------------- compass */}
                <FormControlLabel
                    control={
                        <Switch
                            checked={showCompass}
                            onChange={(event) => setShowCompass(event.target.checked)}
                            inputProps={testId('toggle-compass')}
                        />
                    }
                    label={<span style={{fontWeight: 'bold'}}>Show compass</span>}
                />

                <FormControl sx={{display: 'block', mt: 1, ml: 1}} disabled={!showCompass}>
                    <FormLabel
                        id="compass-size-label"
                        sx={{...sectionLabelSx, color: showCompass ? 'text.primary' : 'text.disabled'}}
                    >
                        Compass size
                    </FormLabel>
                    <RadioGroup
                        row
                        aria-labelledby="compass-size-label"
                        name="compass-size"
                        value={compassSize}
                        onChange={handleCompassSizeChange}
                    >
                        {COMPASS_SIZES.map((size: CompassSize) => (
                            <FormControlLabel
                                key={size}
                                value={size}
                                // Sizing a hidden compass does nothing, so the control
                                // is disabled rather than silently inert.
                                disabled={!showCompass}
                                control={<Radio inputProps={testId(`compass-size-${size}`)} />}
                                label={COMPASS_SIZE_LABELS[size]}
                            />
                        ))}
                    </RadioGroup>
                </FormControl>

                <Divider sx={{my: 2}} />

                {/* ------------------------------------------------ helper menus */}
                <FormLabel component="legend" sx={sectionLabelSx}>
                    Helper buttons
                </FormLabel>
                <div className="flex flex-col">
                    <FormControlLabel
                        control={
                            <Switch
                                checked={showVerbsMenu}
                                onChange={(event) => setShowVerbsMenu(event.target.checked)}
                                inputProps={testId('toggle-verbs-menu')}
                            />
                        }
                        label="Show Verbs menu"
                    />
                    <FormControlLabel
                        control={
                            <Switch
                                checked={showCommandsMenu}
                                onChange={(event) => setShowCommandsMenu(event.target.checked)}
                                inputProps={testId('toggle-commands-menu')}
                            />
                        }
                        label="Show Commands menu"
                    />
                    <FormControlLabel
                        control={
                            <Switch
                                checked={showLocationButton}
                                onChange={(event) => setShowLocationButton(event.target.checked)}
                                inputProps={testId('toggle-location-button')}
                            />
                        }
                        label="Show Location button"
                    />
                    <FormControlLabel
                        control={
                            <Switch
                                checked={showInventoryButton}
                                onChange={(event) => setShowInventoryButton(event.target.checked)}
                                inputProps={testId('toggle-inventory-button')}
                            />
                        }
                        label="Show Inventory button"
                    />
                </div>

                <Divider sx={{my: 2}} />

                {/* -------------------------------------------------- animations */}
                <FormControlLabel
                    control={
                        <Switch
                            checked={animations}
                            onChange={(event) => setAnimations(event.target.checked)}
                            inputProps={testId('toggle-animations')}
                        />
                    }
                    label={
                        <span>
                            <span style={{fontWeight: 'bold'}}>Animations</span>
                            <Typography
                                variant="caption"
                                color="text.secondary"
                                sx={{display: 'block', lineHeight: 1.3}}
                            >
                                Compass pulse, direction flashes and text fade-in
                            </Typography>
                        </span>
                    }
                />
            </DialogContent>

            <DialogActions sx={{p: 2, bgcolor: 'grey.100'}}>
                <Button
                    variant="contained"
                    onClick={handleClose}
                    data-testid="preferences-done"
                    sx={{borderRadius: '20px', px: 3}}
                >
                    Done
                </Button>
            </DialogActions>
        </Dialog>
    );
};

export default PreferencesModal;
