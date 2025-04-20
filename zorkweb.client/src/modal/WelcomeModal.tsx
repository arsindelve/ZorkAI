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
    handleWatchVideo: () => void;
}

const WelcomeDialog: React.FC<WelcomeDialogProps> = ({open, handleClose, handleWatchVideo}) => (
    <Dialog
        open={open}
        onClose={handleClose}
        aria-labelledby="alert-dialog-title"
        aria-describedby="alert-dialog-description"
        maxWidth={false} // Disable preset max-width limits
        PaperProps={{
            style: {width: "70%", maxWidth: "none"}, // Set custom width (80% of the viewport)
        }}
        data-testid="welcome-modal"  // Added test id for modal root
    >
        <DialogTitle id="alert-dialog-title">
            {"Welcome to Zork AI - A Modern Reimagining of the 1980s Classic!"}
        </DialogTitle>
        <DialogContent>
            <DialogContentText id="alert-dialog-description">
                This is a modern re-imagining of the iconic text adventure game Zork I.
                While the story remains true to the original, Zork AI introduces AI-powered parsing and dynamic
                responses for a richer, more immersive experience. It understands complex commands far beyond what
                the original could handle and even delivers witty, sarcastic replies when you try something unexpected.
                <br/> <br/>
                Zork AI is interactive fiction. You control the story by typing commands in plain English in order to
                explore,
                solve puzzles, and shape the adventure. To get started, type a command in the grey box below and press
                Enter/Return. The game will process your input and continue the story based on your instructions.
                <br/> <br/>
                Need inspiration? Try:
                <br/> <br/>
                <ul className="list-disc pl-6 m-0">
                    <li className="uppercase">open the mailbox</li>
                    <li className="uppercase">go south</li>
                    <li className="uppercase">jump up and down</li>
                    <li className="uppercase">tell me about the great underground empire</li>
                </ul>
                <br/>
                See if you can make your way into the house—and from there, descend into the Great Underground Empire,
                where the real adventure
                begins!
                <br/> <br/>
                All credit for the original story, puzzles and Zork universe goes to <a
                href={"https://en.wikipedia.org/wiki/Zork"} target={"_blank"}>Tim Anderson, Marc Blank, Bruce Daniels,
                and Dave Lebling - the brilliant minds behind the classic game.</a>
            </DialogContentText>
        </DialogContent>
        <DialogActions>
            <Button onClick={handleWatchVideo} autoFocus>
                Watch an intro video
            </Button>

            <Button onClick={handleClose} autoFocus data-testid="welcome-modal-close-button">
                Close
            </Button>

        </DialogActions>
    </Dialog>
);

export default WelcomeDialog;