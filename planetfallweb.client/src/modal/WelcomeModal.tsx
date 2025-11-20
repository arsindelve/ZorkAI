import React from 'react';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogTitle from '@mui/material/DialogTitle';
import Button from '@mui/material/Button';
import { Typography, Paper, Box, Divider } from "@mui/material";
import EmojiObjectsIcon from '@mui/icons-material/EmojiObjects';
import VideogameAssetIcon from '@mui/icons-material/VideogameAsset';
import PlayArrowIcon from '@mui/icons-material/PlayArrow';

interface WelcomeDialogProps {
    open: boolean;
    handleClose: () => void;
    handleWatchVideo: () => void;
}

const WelcomeDialog: React.FC<WelcomeDialogProps> = ({open, handleClose, handleWatchVideo}) => (
    <Dialog
        open={open}
        onClose={handleClose}
        aria-labelledby="alert-dialog-title"
        aria-describedby="alert-dialog-description"
        maxWidth={"md"}
        fullWidth={true}
        PaperProps={{
            style: {
                borderRadius: '12px',
                overflow: 'hidden'
            }
        }}
        data-testid="welcome-modal"
    >
        <DialogTitle 
            id="alert-dialog-title"
            className="bg-gradient-to-r from-gray-800 to-gray-900"
            sx={{
                color: 'white',
                display: 'flex',
                alignItems: 'center',
                gap: 1,
                py: 2
            }}
        >
            <EmojiObjectsIcon fontSize="large" />
            <Typography variant="h5" component="span" fontWeight="bold">
                Welcome to Zork AI - A Modern Reimagining of the 1980s Classic!
            </Typography>
        </DialogTitle>
        <DialogContent id="alert-dialog-description" sx={{ pt: 3, pb: 1 }}>
            <Paper
                elevation={2}
                sx={{
                    p: 3,
                    mb: 3,
                    mt: 1,
                    borderRadius: '8px',
                    bgcolor: 'background.paper',
                    transition: 'all 0.2s',
                    '&:hover': {
                        transform: 'translateY(-2px)',
                        boxShadow: 4
                    }
                }}
            >
                <Typography variant="h6" fontWeight="bold" sx={{ mb: 2, color: 'grey.800', display: 'flex', alignItems: 'center', gap: 1 }}>
                    <VideogameAssetIcon /> About Zork AI
                </Typography>

                <Typography variant="body1" paragraph sx={{ color: 'grey.800' }}>
                    This is a modern re-imagining of the iconic text adventure game Zork I.
                    While the story remains true to the original, Zork AI introduces AI-powered parsing and dynamic
                    responses for a richer, more immersive experience. It understands complex commands far beyond what
                    the original could handle and even delivers witty, sarcastic replies when you try something unexpected.
                </Typography>

                <Typography variant="body1" paragraph sx={{ color: 'grey.800' }}>
                    Zork AI is interactive fiction. You control the story by typing commands in plain English in order to
                    explore, solve puzzles, and shape the adventure. To get started, type a command in the grey box below and press
                    Enter/Return. The game will process your input and continue the story based on your instructions.
                </Typography>

                <Divider sx={{ my: 2 }} />

                <Typography variant="h6" fontWeight="bold" sx={{ mb: 2, color: 'grey.800' }}>
                    Need inspiration? Try:
                </Typography>

                <Box sx={{ 
                    bgcolor: 'grey.100', 
                    p: 2, 
                    borderRadius: '8px',
                    mb: 2,
                    fontFamily: 'monospace'
                }}>
                    <ul style={{ listStyleType: 'disc', paddingLeft: '1.5rem', margin: 0 }}>
                        <li style={{ textTransform: 'uppercase', marginBottom: '0.5rem' }}>open the mailbox</li>
                        <li style={{ textTransform: 'uppercase', marginBottom: '0.5rem' }}>go south</li>
                        <li style={{ textTransform: 'uppercase', marginBottom: '0.5rem' }}>jump up and down</li>
                        <li style={{ textTransform: 'uppercase' }}>tell me about the great underground empire</li>
                    </ul>
                </Box>

                <Typography variant="body1" paragraph sx={{ color: 'grey.800', fontWeight: 'bold' }}>
                    See if you can make your way into the house—and from there, descend into the Great Underground Empire,
                    where the real adventure begins!
                </Typography>

                <Divider sx={{ my: 2 }} />

                <Typography variant="body2" sx={{ color: 'grey.600', fontStyle: 'italic' }}>
                    All credit for the original story, puzzles and Zork universe goes to <a
                    href={"https://en.wikipedia.org/wiki/Zork"} target={"_blank"} style={{ color: 'inherit', textDecoration: 'underline' }}>
                    Tim Anderson, Marc Blank, Bruce Daniels, and Dave Lebling - the brilliant minds behind the classic game.</a>
                </Typography>
            </Paper>
        </DialogContent>
        <DialogActions sx={{ p: 2, bgcolor: 'grey.100' }}>
            <Button 
                onClick={handleWatchVideo} 
                variant="contained"
                startIcon={<PlayArrowIcon />}
                sx={{ 
                    borderRadius: '20px',
                    px: 3,
                    bgcolor: 'grey.800',
                    '&:hover': {
                        bgcolor: 'grey.700'
                    }
                }}
            >
                Watch an intro video
            </Button>

            <Button 
                onClick={handleClose} 
                variant="outlined"
                data-testid="welcome-modal-close-button"
                sx={{ 
                    borderRadius: '20px',
                    px: 3,
                    borderColor: 'grey.600',
                    color: 'grey.800',
                    '&:hover': {
                        borderColor: 'grey.800',
                        bgcolor: 'grey.100'
                    }
                }}
            >
                Close
            </Button>
        </DialogActions>
    </Dialog>
);

export default WelcomeDialog;
