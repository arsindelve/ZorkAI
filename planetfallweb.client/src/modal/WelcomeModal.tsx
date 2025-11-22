import React from 'react';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogTitle from '@mui/material/DialogTitle';
import Button from '@mui/material/Button';
import { Typography, Paper, Box, Divider } from "@mui/material";
import EmojiObjectsIcon from '@mui/icons-material/EmojiObjects';
import VideogameAssetIcon from '@mui/icons-material/VideogameAsset';

interface WelcomeDialogProps {
    open: boolean;
    handleClose: () => void;
}

const WelcomeDialog: React.FC<WelcomeDialogProps> = ({open, handleClose}) => (
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
                Welcome to Planetfall AI - A Modern Reimagining of the 1983 Classic!
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
                    <VideogameAssetIcon /> About Planetfall AI
                </Typography>

                <Typography variant="body1" paragraph sx={{ color: 'grey.800' }}>
                    This is a modern re-imagining of the beloved 1983 science fiction text adventure game Planetfall.
                    You play as a lowly Ensign Seventh Class who crash-lands on the mysterious planet Resida after escaping
                    the doomed spaceship Feinstein. While the story, puzzles, and humor remain faithful to the original,
                    Planetfall AI introduces AI-powered parsing and dynamic responses for a richer, more immersive experience.
                </Typography>

                <Typography variant="body1" paragraph sx={{ color: 'grey.800' }}>
                    The game understands natural language commands far beyond what the 1983 version could handle—including
                    complex sentences, context-aware interactions, and even witty responses when you try something creative.
                    And along the way, you'll meet Floyd—everyone's favorite robot companion.
                </Typography>

                <Typography variant="body1" paragraph sx={{ color: 'grey.800' }}>
                    Planetfall AI is interactive fiction. You control the story by typing commands in plain English to
                    explore locations, manipulate objects, solve puzzles, and interact with characters. To get started,
                    type a command in the input box below and press Enter/Return. The game will process your input and
                    continue the adventure based on your actions.
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
                        <li style={{ textTransform: 'uppercase', marginBottom: '0.5rem' }}>look around</li>
                        <li style={{ textTransform: 'uppercase', marginBottom: '0.5rem' }}>take the kit</li>
                        <li style={{ textTransform: 'uppercase', marginBottom: '0.5rem' }}>go west</li>
                        <li style={{ textTransform: 'uppercase', marginBottom: '0.5rem' }}>talk to Floyd</li>
                        <li style={{ textTransform: 'uppercase' }}>tell me about this place</li>
                    </ul>
                </Box>


                <Divider sx={{ my: 2 }} />

                <Typography variant="body2" sx={{ color: 'grey.600', fontStyle: 'italic' }}>
                    All credit for the original story, puzzles, and the Planetfall universe goes to <a
                    href={"https://en.wikipedia.org/wiki/Steve_Meretzky"} target={"_blank"} style={{ color: 'inherit', textDecoration: 'underline' }}>
                    Steve Meretzky</a>, the brilliant mind behind this beloved 1983 Infocom classic.
                </Typography>
            </Paper>
        </DialogContent>
        <DialogActions sx={{ p: 2, bgcolor: 'grey.100' }}>
            <Button
                onClick={handleClose}
                variant="contained"
                data-testid="welcome-modal-close-button"
                sx={{
                    borderRadius: '20px',
                    px: 3,
                    bgcolor: 'grey.800',
                    '&:hover': {
                        bgcolor: 'grey.700'
                    }
                }}
            >
                Close
            </Button>
        </DialogActions>
    </Dialog>
);

export default WelcomeDialog;
