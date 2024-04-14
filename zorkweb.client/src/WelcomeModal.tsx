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
            {"Welcome to Zork AI"}
        </DialogTitle>
        <DialogContent>
            <DialogContentText id="alert-dialog-description">
                This is a re-imagining of the original 1980's classic adventure game Zork I. 
                The story is the same, but we've added AI enhanced parsing and response generation for
                a more immersive experience. To get started, type your input in the grey box below, and press
                'return'. Click the menu on the top left for more information. Enjoy!
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