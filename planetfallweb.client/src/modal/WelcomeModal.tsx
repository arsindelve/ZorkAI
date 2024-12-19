import React from 'react';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import DialogTitle from '@mui/material/DialogTitle';
import Button from '@mui/material/Button';

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
    >
        <DialogTitle id="alert-dialog-title">
            {"Welcome to Planetfall AI"}
        </DialogTitle>
        <DialogContent>
            <DialogContentText id="alert-dialog-description">
                This is a re-imagining of (and a celebration of) the original 1980's classic sci-fi game Planetfall by Steve Meretzky.
                The story is the same, but we've added AI enhanced parsing and response generation for
                a more immersive experience. Using AI, the narrator will understand and respond to
                anything you type, always in character and always keeping you in the bounds of the original game.
                To get started, type your input in the grey box below, and press
                'return'. 
                
                Looking for inspiration? Trying typing "STARBOARD" (or "EAST") to explore the ship,
                or type "CLEAN THE FLOORS" to see if you can get through today's chores. If you're feeling
                adventurous, try "FLY THE SHIP" or "LAY DOWN FOR A NAP". 
                
                Click the menu on the top right for more information. Enjoy!
            </DialogContentText>
        </DialogContent>
        <DialogActions>
            <Button onClick={handleClose} autoFocus>
                Close
            </Button>
        </DialogActions>
    </Dialog>
);

export default WelcomeDialog;

