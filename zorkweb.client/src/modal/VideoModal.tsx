import React from 'react';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogTitle from '@mui/material/DialogTitle';
import Button from '@mui/material/Button';

interface VideoDialogProps {
    open: boolean;
    handleClose: () => void;
}

const VideoDialog: React.FC<VideoDialogProps> = ({open, handleClose}) => (
    <Dialog
        open={open}
        onClose={handleClose}
        aria-labelledby="alert-dialog-title"
        aria-describedby="alert-dialog-description"
        maxWidth={false} // Disable default width limitations
        fullWidth // Full width dialog
        PaperProps={{
            style: {height: "90vh", width: "90vw"}, // Set almost full screen dimensions
        }}
    >
        <DialogTitle id="alert-dialog-title">
            {"Welcome to Zork AI"}
        </DialogTitle>
        <DialogContent
            style={{display: "flex", justifyContent: "center", alignItems: "center", padding: 0}} // Center content
        >
            <video
                src={'https://zorkai-assets.s3.us-east-1.amazonaws.com/zorkintro.mp4'} // Use the provided video URL
                controls
                autoPlay // Enable auto play
                style={{width: "100%", height: "100%"}} // Make the video fully responsive
            />
        </DialogContent>
        <DialogActions>
            <Button onClick={handleClose} autoFocus>
                Close
            </Button>
        </DialogActions>
    </Dialog>
);

export default VideoDialog;