import * as React from 'react';
import {
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    Button,
    FormControl,
    FormLabel,
    RadioGroup,
    FormControlLabel,
    Radio,
    Switch,
    Slider,
    Box,
    Typography,
    Divider,
    MenuItem,
    Select,
    InputLabel
} from '@mui/material';
import { useGameContext } from "../GameContext";
import DialogType from "../model/DialogType";
import { IUserPreferences, Verbosity, defaultUserPreferences } from "../model/UserPreferences";

interface PreferencesModalProps {
    userPreferences: IUserPreferences;
    onSave: (preferences: IUserPreferences) => void;
}

export default function PreferencesModal({ userPreferences, onSave }: PreferencesModalProps) {
    const { dialogToOpen, setDialogToOpen } = useGameContext();
    const [preferences, setPreferences] = React.useState<IUserPreferences>(userPreferences);

    const open = dialogToOpen === DialogType.Preferences;

    const handleClose = () => {
        setDialogToOpen(undefined);
    };

    const handleSave = () => {
        onSave(preferences);
        handleClose();
    };

    const handleReset = () => {
        setPreferences({ ...defaultUserPreferences });
    };

    const handleChange = (field: keyof IUserPreferences, value: any) => {
        setPreferences(prev => ({ ...prev, [field]: value }));
    };

    React.useEffect(() => {
        if (open) {
            setPreferences({ ...userPreferences });
        }
    }, [open, userPreferences]);

    return (
        <Dialog 
            open={open} 
            onClose={handleClose}
            maxWidth="md"
            fullWidth
            PaperProps={{
                sx: {
                    borderRadius: '12px',
                    minHeight: '60vh'
                }
            }}
        >
            <DialogTitle sx={{ 
                bgcolor: 'primary.main', 
                color: 'primary.contrastText',
                fontSize: '1.25rem',
                fontWeight: 'bold'
            }}>
                User Preferences
            </DialogTitle>
            
            <DialogContent sx={{ pt: 3 }}>
                {/* Game Settings */}
                <Typography variant="h6" gutterBottom sx={{ color: 'primary.main', fontWeight: 'bold' }}>
                    Game Settings
                </Typography>
                
                <FormControl component="fieldset" margin="normal" fullWidth>
                    <FormLabel component="legend">Room Description Verbosity</FormLabel>
                    <RadioGroup
                        value={preferences.verbosity}
                        onChange={(e) => handleChange('verbosity', parseInt(e.target.value))}
                        row
                    >
                        <FormControlLabel value={Verbosity.SuperBrief} control={<Radio />} label="Super Brief" />
                        <FormControlLabel value={Verbosity.Brief} control={<Radio />} label="Brief" />
                        <FormControlLabel value={Verbosity.Verbose} control={<Radio />} label="Verbose" />
                    </RadioGroup>
                </FormControl>

                <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', my: 2 }}>
                    <Typography>Enable AI-Enhanced Command Parsing</Typography>
                    <Switch
                        checked={preferences.enableAiParsing}
                        onChange={(e) => handleChange('enableAiParsing', e.target.checked)}
                    />
                </Box>

                <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', my: 2 }}>
                    <Typography>Enable AI-Generated Narrative Responses</Typography>
                    <Switch
                        checked={preferences.enableAiGeneration}
                        onChange={(e) => handleChange('enableAiGeneration', e.target.checked)}
                    />
                </Box>

                <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', my: 2 }}>
                    <Typography>Auto-Save Game</Typography>
                    <Switch
                        checked={preferences.autoSave}
                        onChange={(e) => handleChange('autoSave', e.target.checked)}
                    />
                </Box>

                {preferences.autoSave && (
                    <Box sx={{ my: 2 }}>
                        <Typography gutterBottom>Auto-Save Interval (minutes): {preferences.autoSaveIntervalMinutes}</Typography>
                        <Slider
                            value={preferences.autoSaveIntervalMinutes}
                            onChange={(_, value) => handleChange('autoSaveIntervalMinutes', value)}
                            min={1}
                            max={60}
                            step={1}
                            marks={[
                                { value: 1, label: '1' },
                                { value: 10, label: '10' },
                                { value: 30, label: '30' },
                                { value: 60, label: '60' }
                            ]}
                        />
                    </Box>
                )}

                <Divider sx={{ my: 3 }} />

                {/* Display Settings */}
                <Typography variant="h6" gutterBottom sx={{ color: 'primary.main', fontWeight: 'bold' }}>
                    Display Settings
                </Typography>

                <FormControl fullWidth margin="normal">
                    <InputLabel>Theme</InputLabel>
                    <Select
                        value={preferences.theme}
                        label="Theme"
                        onChange={(e) => handleChange('theme', e.target.value)}
                    >
                        <MenuItem value="dark">Dark</MenuItem>
                        <MenuItem value="light">Light</MenuItem>
                        <MenuItem value="auto">Auto</MenuItem>
                    </Select>
                </FormControl>

                <Box sx={{ my: 2 }}>
                    <Typography gutterBottom>Font Size: {preferences.fontSize}px</Typography>
                    <Slider
                        value={preferences.fontSize}
                        onChange={(_, value) => handleChange('fontSize', value)}
                        min={12}
                        max={24}
                        step={1}
                        marks={[
                            { value: 12, label: '12px' },
                            { value: 16, label: '16px' },
                            { value: 20, label: '20px' },
                            { value: 24, label: '24px' }
                        ]}
                    />
                </Box>

                <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', my: 2 }}>
                    <Typography>Show Compass/Navigation</Typography>
                    <Switch
                        checked={preferences.showCompass}
                        onChange={(e) => handleChange('showCompass', e.target.checked)}
                    />
                </Box>

                <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', my: 2 }}>
                    <Typography>Show Inventory Panel</Typography>
                    <Switch
                        checked={preferences.showInventoryPanel}
                        onChange={(e) => handleChange('showInventoryPanel', e.target.checked)}
                    />
                </Box>

                <Divider sx={{ my: 3 }} />

                {/* Audio Settings */}
                <Typography variant="h6" gutterBottom sx={{ color: 'primary.main', fontWeight: 'bold' }}>
                    Audio Settings
                </Typography>

                <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', my: 2 }}>
                    <Typography>Enable Sound Effects</Typography>
                    <Switch
                        checked={preferences.enableSounds}
                        onChange={(e) => handleChange('enableSounds', e.target.checked)}
                    />
                </Box>

                {preferences.enableSounds && (
                    <Box sx={{ my: 2 }}>
                        <Typography gutterBottom>Sound Volume: {preferences.soundVolume}%</Typography>
                        <Slider
                            value={preferences.soundVolume}
                            onChange={(_, value) => handleChange('soundVolume', value)}
                            min={0}
                            max={100}
                            step={5}
                            marks={[
                                { value: 0, label: '0%' },
                                { value: 50, label: '50%' },
                                { value: 100, label: '100%' }
                            ]}
                        />
                    </Box>
                )}

                <Divider sx={{ my: 3 }} />

                {/* Advanced Settings */}
                <Typography variant="h6" gutterBottom sx={{ color: 'primary.main', fontWeight: 'bold' }}>
                    Advanced Settings
                </Typography>

                <Box sx={{ my: 2 }}>
                    <Typography gutterBottom>Command History Size: {preferences.maxCommandHistory}</Typography>
                    <Slider
                        value={preferences.maxCommandHistory}
                        onChange={(_, value) => handleChange('maxCommandHistory', value)}
                        min={10}
                        max={200}
                        step={10}
                        marks={[
                            { value: 10, label: '10' },
                            { value: 50, label: '50' },
                            { value: 100, label: '100' },
                            { value: 200, label: '200' }
                        ]}
                    />
                </Box>
            </DialogContent>

            <DialogActions sx={{ px: 3, py: 2, gap: 1 }}>
                <Button 
                    onClick={handleReset} 
                    variant="outlined"
                    sx={{ borderRadius: '8px' }}
                >
                    Reset to Defaults
                </Button>
                <Button 
                    onClick={handleClose} 
                    variant="outlined"
                    sx={{ borderRadius: '8px' }}
                >
                    Cancel
                </Button>
                <Button 
                    onClick={handleSave} 
                    variant="contained"
                    sx={{ borderRadius: '8px' }}
                >
                    Save Preferences
                </Button>
            </DialogActions>
        </Dialog>
    );
}