import React from 'react';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogTitle from '@mui/material/DialogTitle';
import Button from '@mui/material/Button';
import { Typography } from '@mui/material';
import PlayArrowIcon from '@mui/icons-material/PlayArrow';

interface VideoDialogProps {
    open: boolean;
    handleClose: () => void;
}

const VideoDialog: React.FC<VideoDialogProps> = ({open, handleClose}) => (
    <Dialog
        open={open}
        onClose={handleClose}
        aria-labelledby="video-dialog-title"
        aria-describedby="video-dialog-description"
        maxWidth="lg"
        fullWidth={true}
        PaperProps={{
            style: {
                borderRadius: '12px',
                overflow: 'hidden',
                maxHeight: '90vh'
            }
        }}
    >
        <DialogTitle 
            id="video-dialog-title"
            className="bg-gradient-to-r from-gray-800 to-gray-900"
            sx={{
                color: 'white',
                display: 'flex',
                alignItems: 'center',
                gap: 1,
                py: 2
            }}
        >
            <PlayArrowIcon fontSize="large" />
            <Typography variant="h5" component="span" fontWeight="bold">
                Welcome to Zork AI
            </Typography>
        </DialogTitle>
        <DialogContent
            sx={{
                display: "flex", 
                justifyContent: "center", 
                alignItems: "center", 
                padding: 0,
                height: "70vh"
            }}
        >
            <video
                src={'https://zorkai-assets.s3.us-east-1.amazonaws.com/zorkintro.mp4'}
                controls
                autoPlay
                style={{width: "100%", height: "100%"}}
            />
        </DialogContent>
        <DialogActions sx={{ p: 2, bgcolor: 'grey.100' }}>
            <Button 
                onClick={handleClose} 
                variant="outlined"
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
                autoFocus
            >
                Close
            </Button>
        </DialogActions>
    </Dialog>
);

export default VideoDialog;
