import React from 'react';
import {
    Box,
    Button,
    Dialog,
    DialogActions,
    DialogContent,
    DialogTitle,
    Divider,
    Paper,
    Typography,
} from '@mui/material';
import EmojiObjectsIcon from '@mui/icons-material/EmojiObjects';
import VideogameAssetIcon from '@mui/icons-material/VideogameAsset';

export interface WelcomeDialogProps {
    open: boolean;
    handleClose: () => void;
    title: string;
    aboutTitle: string;
    paragraphs: React.ReactNode[];
    commands: string[];
    commandsHeading?: string;
    attribution: React.ReactNode;
    closing?: React.ReactNode;
    primaryAction?: {label: string; icon?: React.ReactNode; onClick: () => void};
}

export default function WelcomeDialog({
    open,
    handleClose,
    title,
    aboutTitle,
    paragraphs,
    commands,
    commandsHeading = 'Try commands like:',
    attribution,
    closing,
    primaryAction,
}: WelcomeDialogProps) {
    return (
        <Dialog
            data-testid="welcome-modal"
            open={open}
            onClose={handleClose}
            aria-labelledby="welcome-dialog-title"
            aria-describedby="welcome-dialog-description"
            maxWidth="md"
            fullWidth
        >
            <DialogTitle
                id="welcome-dialog-title"
                sx={{
                    display: 'flex',
                    alignItems: 'center',
                    gap: 1,
                    bgcolor: 'grey.900',
                    color: 'white',
                }}
            >
                <EmojiObjectsIcon fontSize="large" />
                <Typography variant="h5" component="span" fontWeight="bold">
                    {title}
                </Typography>
            </DialogTitle>
            <DialogContent id="welcome-dialog-description" sx={{pt: 3, pb: 1}}>
                <Paper elevation={0} sx={{p: 3, bgcolor: 'grey.100', borderRadius: 2}}>
                    <Typography
                        variant="h6"
                        component="h2"
                        fontWeight="bold"
                        sx={{
                            mb: 2,
                            color: 'grey.800',
                            display: 'flex',
                            alignItems: 'center',
                            gap: 1,
                        }}
                    >
                        <VideogameAssetIcon /> {aboutTitle}
                    </Typography>
                    {paragraphs.map((paragraph, index) => (
                        <Typography key={index} variant="body1" paragraph sx={{color: 'grey.800'}}>
                            {paragraph}
                        </Typography>
                    ))}
                    <Divider sx={{my: 2}} />
                    <Typography variant="body1" fontWeight="bold" sx={{mb: 1, color: 'grey.800'}}>
                        {commandsHeading}
                    </Typography>
                    <Box sx={{bgcolor: 'grey.200', p: 2, borderRadius: 1, mb: 2}}>
                        <ul style={{listStyleType: 'disc', paddingLeft: '1.5rem', margin: 0}}>
                            {commands.map((command) => (
                                <li
                                    key={command}
                                    style={{textTransform: 'uppercase', marginBottom: '.5rem'}}
                                >
                                    {command}
                                </li>
                            ))}
                        </ul>
                    </Box>
                    {closing && (
                        <Typography
                            variant="body1"
                            paragraph
                            sx={{color: 'grey.800', fontWeight: 'bold'}}
                        >
                            {closing}
                        </Typography>
                    )}
                    <Divider sx={{my: 2}} />
                    <Typography variant="body2" sx={{color: 'grey.600', fontStyle: 'italic'}}>
                        {attribution}
                    </Typography>
                </Paper>
            </DialogContent>
            <DialogActions sx={{p: 2, bgcolor: 'grey.100'}}>
                {primaryAction && (
                    <Button
                        onClick={primaryAction.onClick}
                        variant="contained"
                        startIcon={primaryAction.icon}
                        sx={{borderRadius: '20px', px: 3}}
                    >
                        {primaryAction.label}
                    </Button>
                )}
                <Button
                    onClick={handleClose}
                    variant={primaryAction ? 'outlined' : 'contained'}
                    data-testid="welcome-modal-close-button"
                    sx={{borderRadius: '20px', px: 3}}
                >
                    Close
                </Button>
            </DialogActions>
        </Dialog>
    );
}
