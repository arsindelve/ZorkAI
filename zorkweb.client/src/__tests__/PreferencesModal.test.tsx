import React from 'react';
import {render, screen, fireEvent} from '@testing-library/react';
import {PreferencesModal, useGameContext} from '@zork-ai/shared-types';

describe('PreferencesModal', () => {
    const setTranscriptFontSize = jest.fn();
    const setTranscriptFont = jest.fn();
    const setTranscriptLineSpacing = jest.fn();
    const setTranscriptMarker = jest.fn();
    const setAnimations = jest.fn();
    const setShowCompass = jest.fn();
    const setCompassSize = jest.fn();
    const setShowVerbsMenu = jest.fn();
    const setShowCommandsMenu = jest.fn();
    const setShowLocationButton = jest.fn();
    const setShowInventoryButton = jest.fn();

    const renderModal = (overrides: Record<string, unknown> = {}) => {
        (useGameContext as jest.Mock).mockReturnValue({
            transcriptFontSize: 'medium',
            setTranscriptFontSize,
            transcriptFont: 'terminal',
            setTranscriptFont,
            transcriptLineSpacing: 'normal',
            setTranscriptLineSpacing,
            transcriptMarker: 'none',
            setTranscriptMarker,
            animations: true,
            setAnimations,
            showCompass: true,
            setShowCompass,
            compassSize: 'medium',
            setCompassSize,
            showVerbsMenu: true,
            setShowVerbsMenu,
            showCommandsMenu: true,
            setShowCommandsMenu,
            showLocationButton: true,
            setShowLocationButton,
            showInventoryButton: true,
            setShowInventoryButton,
            ...overrides,
        });

        return render(
            <PreferencesModal open={true} handleClose={jest.fn()} baseFontSizePx={15} />,
        );
    };

    beforeEach(() => {
        jest.clearAllMocks();
    });

    test('shows all five transcript font sizes', () => {
        renderModal();

        expect(screen.getByText('Extra Small')).toBeInTheDocument();
        expect(screen.getByText('Small')).toBeInTheDocument();
        expect(screen.getByText('Medium')).toBeInTheDocument();
        expect(screen.getByText('Large')).toBeInTheDocument();
        expect(screen.getByText('Extra Large')).toBeInTheDocument();
    });

    test('preselects the current font size', () => {
        renderModal({transcriptFontSize: 'large'});

        expect(screen.getByTestId('font-size-large')).toBeChecked();
        expect(screen.getByTestId('font-size-medium')).not.toBeChecked();
    });

    test('choosing a font size applies it immediately', () => {
        renderModal();

        fireEvent.click(screen.getByTestId('font-size-xlarge'));

        expect(setTranscriptFontSize).toHaveBeenCalledWith('xlarge');
    });

    test('the specimen renders at the chosen size', () => {
        // One live specimen replaces the per-option previews: it shows the real
        // transcript, so size, font and spacing are judged together.
        renderModal({transcriptFontSize: 'large'});

        expect(document.querySelector('.prefs__specimen')).toHaveStyle({fontSize: '17px'});
    });

    test('the specimen renders in the chosen font and spacing', () => {
        renderModal({
            transcriptFont: 'storybook',
            transcriptLineSpacing: 'roomy',
        });
        const specimen = document.querySelector('.prefs__specimen');

        expect(specimen).toHaveStyle({
            fontFamily: "Georgia, 'Iowan Old Style', 'Times New Roman', Times, serif",
        });
        expect(specimen).toHaveStyle({lineHeight: '1.8'});
    });

    test('the specimen shows the command label when one is chosen', () => {
        renderModal({transcriptMarker: 'number'});

        expect(document.querySelector('.prefs__specimen-marker')).toBeInTheDocument();
    });

    test('the specimen shows no label when the marker is off', () => {
        renderModal();

        expect(document.querySelector('.prefs__specimen-marker')).not.toBeInTheDocument();
    });

    test('offers all five transcript fonts', () => {
        renderModal();

        ['terminal', 'typewriter', 'storybook', 'modern', 'legible'].forEach((font) => {
            expect(screen.getByTestId(`font-${font}`)).toBeInTheDocument();
        });
    });

    test('preselects the current font', () => {
        renderModal({transcriptFont: 'storybook'});

        expect(screen.getByTestId('font-storybook')).toBeChecked();
        expect(screen.getByTestId('font-terminal')).not.toBeChecked();
    });

    test('choosing a font applies it immediately', () => {
        renderModal();

        fireEvent.click(screen.getByTestId('font-typewriter'));

        expect(setTranscriptFont).toHaveBeenCalledWith('typewriter');
    });

    test('previews each font in that font', () => {
        renderModal();

        expect(screen.getByText('Typewriter')).toHaveStyle({
            fontFamily: "'Courier New', Courier, 'Nimbus Mono PS', monospace",
        });
    });

    test('line spacing defaults to Normal and is changeable', () => {
        renderModal();

        expect(screen.getByTestId('line-spacing-normal')).toBeChecked();

        fireEvent.click(screen.getByTestId('line-spacing-roomy'));

        expect(setTranscriptLineSpacing).toHaveBeenCalledWith('roomy');
    });

    test('command marker defaults to None and is changeable', () => {
        renderModal();

        expect(screen.getByTestId('marker-none')).toBeChecked();

        fireEvent.click(screen.getByTestId('marker-number'));

        expect(setTranscriptMarker).toHaveBeenCalledWith('number');
    });

    test('animations default to on and can be turned off', () => {
        renderModal();

        expect(screen.getByTestId('toggle-animations')).toBeChecked();

        fireEvent.click(screen.getByTestId('toggle-animations'));

        expect(setAnimations).toHaveBeenCalledWith(false);
    });

    test('toggling the compass off applies immediately', () => {
        renderModal();

        fireEvent.click(screen.getByTestId('toggle-compass'));

        expect(setShowCompass).toHaveBeenCalledWith(false);
    });

    test('compass size is selectable while the compass is on', () => {
        renderModal();

        expect(screen.getByTestId('compass-size-large')).not.toBeDisabled();

        fireEvent.click(screen.getByTestId('compass-size-large'));

        expect(setCompassSize).toHaveBeenCalledWith('large');
    });

    test('compass size is disabled when the compass is off', () => {
        // Sizing a hidden compass does nothing, so the control must not look live.
        renderModal({showCompass: false});

        expect(screen.getByTestId('compass-size-small')).toBeDisabled();
        expect(screen.getByTestId('compass-size-medium')).toBeDisabled();
        expect(screen.getByTestId('compass-size-large')).toBeDisabled();
    });

    test.each([
        ['toggle-verbs-menu', () => setShowVerbsMenu],
        ['toggle-commands-menu', () => setShowCommandsMenu],
        ['toggle-location-button', () => setShowLocationButton],
        ['toggle-inventory-button', () => setShowInventoryButton],
    ])('%s turns its element off', (testId, getSetter) => {
        renderModal();

        fireEvent.click(screen.getByTestId(testId));

        expect(getSetter()).toHaveBeenCalledWith(false);
    });

    test('every toggle reflects its stored state', () => {
        renderModal({
            showVerbsMenu: false,
            showCommandsMenu: false,
            showLocationButton: false,
            showInventoryButton: false,
        });

        expect(screen.getByTestId('toggle-verbs-menu')).not.toBeChecked();
        expect(screen.getByTestId('toggle-commands-menu')).not.toBeChecked();
        expect(screen.getByTestId('toggle-location-button')).not.toBeChecked();
        expect(screen.getByTestId('toggle-inventory-button')).not.toBeChecked();
    });
});
