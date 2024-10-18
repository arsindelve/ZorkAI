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
                This is a re-imagining of the original 1980's classic text adventure game Zork I.
                The story is the same, but we've added AI parsing and response generation for
                a more immersive experience. Zork AI understands much more complex sentences than the
                original, and provides funny, sarcastic responses if you do something the original game
                did not expect....like "fly away to Geneva" or "this game is boring!"
                <br/> <br/>
                To get started, type your input in the grey box below, and press
                'return'. After a brief pause, the game will respond and continue the story.
                <br/> <br/>
                Need inspiration? try "open the mailbox", "go south", or "jump up and down". See if you can get into the
                house, and then into the Great Underground Empire below, where the adventure awaits.

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

